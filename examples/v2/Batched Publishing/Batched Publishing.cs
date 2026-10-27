using Grpc.Core;
using NationalInstruments.DataStore.V2.Utilities;
using NationalInstruments.Measurements.Data.V2;
using NationalInstruments.Protobuf.Types;

const string testResultNamePrefix = "V2 Batched Publishing Test Result";
const string stepNamePrefix = "V2 Batched Publishing Step";
const string measurementNamePrefix = "V2 Batched Publishing Measurement";
const string rejectedMeasurementNamePrefix = "V2 Batched Publishing Rejected Measurement";

var runSuffix = Guid.NewGuid().ToString("N");
var testResultNames = Enumerable.Range(1, 2)
    .Select(index => WithRunSuffix($"{testResultNamePrefix} {index}", runSuffix))
    .ToArray();
var stepNames = Enumerable.Range(1, 2)
    .Select(index => WithRunSuffix($"{stepNamePrefix} {index}", runSuffix))
    .ToArray();
var measurementNames = Enumerable.Range(1, 3)
    .Select(index => WithRunSuffix($"{measurementNamePrefix} {index}", runSuffix))
    .ToArray();
var rejectedMeasurementName = WithRunSuffix(rejectedMeasurementNamePrefix, runSuffix);

using var dataStoreContext = new DataStoreContext(ParseEndpoint(args));
var dataStoreServiceClient = dataStoreContext.CreateDataStoreClient();
Console.WriteLine($"Run suffix: {runSuffix}");

var testResultBatchResponse = await dataStoreServiceClient.BatchCreateTestResultsAsync(
    new BatchCreateTestResultsRequest
    {
        Requests =
        {
            testResultNames.Select(name => new CreateTestResultRequest
            {
                TestResult = new TestResult
                {
                    Name = name,
                },
            }),
        },
    }).ResponseAsync;
EnsureTestResultBatchResponse(testResultBatchResponse.TestResults, testResultNames);

var stepBatchResponse = await dataStoreServiceClient.BatchCreateStepsAsync(
    new BatchCreateStepsRequest
    {
        Requests =
        {
            stepNames.Select(
                (name, index) => new CreateStepRequest
                {
                    Step = new Step
                    {
                        TestResultId = testResultBatchResponse.TestResults[index].Id,
                        Name = name,
                    },
                }),
        },
    }).ResponseAsync;
EnsureStepBatchResponse(
    stepBatchResponse.Steps,
    stepNames,
    testResultBatchResponse.TestResults.Select(testResult => testResult.Id).ToArray());

var batchResponse = await dataStoreServiceClient.BatchCreateMeasurementsAsync(
    new BatchCreateMeasurementsRequest
    {
        Requests =
        {
            measurementNames.Select(name => new CreateMeasurementRequest
            {
                Measurement = CreateMeasurement(stepBatchResponse.Steps[0].Id, name),
            }),
        },
    }).ResponseAsync;

EnsureMeasurementBatchResponse(
    batchResponse.Measurements,
    measurementNames,
    stepBatchResponse.Steps[0].Id);
await EnsureReadBackAsync(
    batchResponse.Measurements,
    measurementNames,
    stepBatchResponse.Steps[0].Id);
Console.WriteLine(
    $"Verified ordered batch creation of {testResultBatchResponse.TestResults.Count} test results, " +
    $"{stepBatchResponse.Steps.Count} steps, and {batchResponse.Measurements.Count} measurements.");

await AssertInvalidBatchIsAtomicAsync(stepBatchResponse.Steps[0].Id, rejectedMeasurementName);
Console.WriteLine("Verified invalid batch failure was atomic and included validation context.");

async Task EnsureReadBackAsync(
    IReadOnlyList<Measurement> createdMeasurements,
    IReadOnlyList<string> expectedNames,
    string expectedStepId)
{
    for (var index = 0; index < createdMeasurements.Count; index++)
    {
        var responseMeasurement = createdMeasurements[index];
        var readBackMeasurement = await dataStoreServiceClient.GetMeasurementAsync(
            new GetMeasurementRequest
            {
                MeasurementId = responseMeasurement.Id,
            });
        EnsureResource(readBackMeasurement, $"Measurement '{responseMeasurement.Id}' read-back");

        if (readBackMeasurement.Id != responseMeasurement.Id ||
            readBackMeasurement.Name != expectedNames[index] ||
            readBackMeasurement.StepId != expectedStepId)
        {
            throw new InvalidOperationException(
                $"Measurement read-back at index {index} did not match the batch response. " +
                $"Expected Id='{responseMeasurement.Id}', Name='{expectedNames[index]}', StepId='{expectedStepId}'; " +
                $"actual Id='{readBackMeasurement.Id}', Name='{readBackMeasurement.Name}', StepId='{readBackMeasurement.StepId}'.");
        }
    }
}

async Task AssertInvalidBatchIsAtomicAsync(string validStepId, string validMeasurementName)
{
    try
    {
        await dataStoreServiceClient.BatchCreateMeasurementsAsync(
            new BatchCreateMeasurementsRequest
            {
                Requests =
                {
                    new CreateMeasurementRequest
                    {
                        Measurement = CreateMeasurement(validStepId, validMeasurementName),
                    },
                    new CreateMeasurementRequest
                    {
                        Measurement = CreateMeasurement(string.Empty, $"{validMeasurementName} Invalid"),
                    },
                },
            }).ResponseAsync;
    }
    catch (RpcException exception)
    {
        if (exception.StatusCode is not (StatusCode.InvalidArgument or StatusCode.FailedPrecondition) ||
            string.IsNullOrWhiteSpace(exception.Status.Detail))
        {
            throw new InvalidOperationException(
                $"Invalid batch returned unhelpful validation context: " +
                $"StatusCode={exception.StatusCode}, Detail='{exception.Status.Detail}'.",
                exception);
        }

        var rejectedMeasurementQuery = await dataStoreServiceClient.QueryMeasurementsAsync(
            new QueryMeasurementsRequest
            {
                OdataQuery = FilterByName(validMeasurementName),
            }).ResponseAsync;

        if (rejectedMeasurementQuery.Measurements.Count != 0)
        {
            throw new InvalidOperationException(
                $"Invalid batch partially created '{validMeasurementName}'. " +
                $"Expected no matching measurements, but query returned IDs: " +
                $"{string.Join(", ", rejectedMeasurementQuery.Measurements.Select(measurement => measurement.Id))}. " +
                $"Validation detail: '{exception.Status.Detail}'.");
        }

        return;
    }

    throw new InvalidOperationException(
        $"Invalid batch unexpectedly succeeded. Expected validation failure for request with an empty StepId.");
}

static Measurement CreateMeasurement(string stepId, string name) =>
    new()
    {
        StepId = stepId,
        Name = name,
        Value = MeasurementValues.From(1.0),
    };

static void EnsureTestResultBatchResponse(
    IReadOnlyList<TestResult> testResults,
    IReadOnlyList<string> expectedNames)
{
    if (testResults.Count != expectedNames.Count)
    {
        throw new InvalidOperationException(
            $"Batch response contained {testResults.Count} test results; expected {expectedNames.Count}.");
    }

    for (var index = 0; index < testResults.Count; index++)
    {
        var testResult = testResults[index];
        if (string.IsNullOrWhiteSpace(testResult.Id) || testResult.Name != expectedNames[index])
        {
            throw new InvalidOperationException(
                $"TestResult batch response at index {index} did not preserve the requested resource. " +
                $"Expected Name='{expectedNames[index]}' and a non-empty ID; " +
                $"actual Id='{testResult.Id}', Name='{testResult.Name}'.");
        }
    }
}

static void EnsureStepBatchResponse(
    IReadOnlyList<Step> steps,
    IReadOnlyList<string> expectedNames,
    IReadOnlyList<string> expectedTestResultIds)
{
    if (steps.Count != expectedNames.Count)
    {
        throw new InvalidOperationException(
            $"Batch response contained {steps.Count} steps; expected {expectedNames.Count}.");
    }

    for (var index = 0; index < steps.Count; index++)
    {
        var step = steps[index];
        if (string.IsNullOrWhiteSpace(step.Id) ||
            step.Name != expectedNames[index] ||
            step.TestResultId != expectedTestResultIds[index])
        {
            throw new InvalidOperationException(
                $"Step batch response at index {index} did not preserve the requested resource. " +
                $"Expected Name='{expectedNames[index]}', TestResultId='{expectedTestResultIds[index]}', and a non-empty ID; " +
                $"actual Id='{step.Id}', Name='{step.Name}', TestResultId='{step.TestResultId}'.");
        }
    }
}

static void EnsureMeasurementBatchResponse(
    IReadOnlyList<Measurement> measurements,
    IReadOnlyList<string> expectedNames,
    string expectedStepId)
{
    if (measurements.Count != expectedNames.Count)
    {
        throw new InvalidOperationException(
            $"Batch response contained {measurements.Count} measurements; expected {expectedNames.Count}.");
    }

    var measurementIds = new HashSet<string>(StringComparer.Ordinal);
    for (var index = 0; index < measurements.Count; index++)
    {
        var measurement = measurements[index];
        if (string.IsNullOrWhiteSpace(measurement.Id) ||
            !measurementIds.Add(measurement.Id) ||
            measurement.Name != expectedNames[index] ||
            measurement.StepId != expectedStepId)
        {
            throw new InvalidOperationException(
                $"Batch response at index {index} did not preserve the requested resource. " +
                $"Expected Name='{expectedNames[index]}', StepId='{expectedStepId}', and a unique non-empty ID; " +
                $"actual Id='{measurement.Id}', Name='{measurement.Name}', StepId='{measurement.StepId}'.");
        }
    }
}

static string FilterByName(string name) => $"$filter=Name eq '{name}'";

static string WithRunSuffix(string namePrefix, string runSuffix) => $"{namePrefix} {runSuffix}";

static Uri ParseEndpoint(string[] arguments)
{
    const string endpointOption = "--endpoint";
    var endpoint = "http://localhost:58371";

    for (var index = 0; index < arguments.Length; index++)
    {
        if (arguments[index] != endpointOption || index + 1 == arguments.Length)
        {
            throw new ArgumentException(
                $"Usage: Batched Publishing [{endpointOption} <absolute-http-or-https-uri>]");
        }

        endpoint = arguments[++index];
    }

    return new Uri(endpoint, UriKind.Absolute);
}

static void EnsureResource<TResource>(TResource? resource, string resourceDescription)
    where TResource : class
{
    if (resource is null)
    {
        throw new InvalidOperationException($"{resourceDescription} was missing from the service response.");
    }
}
