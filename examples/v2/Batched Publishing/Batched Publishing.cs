using Grpc.Core;
using NationalInstruments.DataStore.V2.Utilities;
using NationalInstruments.Measurements.Data.V2;
using NationalInstruments.Protobuf.Types;

const string testResultNamePrefix = "V2 Batched Publishing Test Result";
const string stepNamePrefix = "V2 Batched Publishing Step";
const string measurementNamePrefix = "V2 Batched Publishing Measurement";
const string rejectedMeasurementNamePrefix = "V2 Batched Publishing Rejected Measurement";

var runSuffix = Guid.NewGuid().ToString("N");
var testResultName = WithRunSuffix(testResultNamePrefix, runSuffix);
var stepName = WithRunSuffix(stepNamePrefix, runSuffix);
var measurementNames = Enumerable.Range(1, 3)
    .Select(index => WithRunSuffix($"{measurementNamePrefix} {index}", runSuffix))
    .ToArray();
var rejectedMeasurementName = WithRunSuffix(rejectedMeasurementNamePrefix, runSuffix);

using var dataStoreContext = new DataStoreContext(ParseEndpoint(args));
var dataStoreServiceClient = dataStoreContext.CreateDataStoreClient();
Console.WriteLine($"Run suffix: {runSuffix}");

var testResult = await dataStoreServiceClient.CreateTestResultAsync(
    new CreateTestResultRequest
    {
        TestResult = new TestResult
        {
            Name = testResultName,
        },
    });
EnsureResource(testResult, $"TestResult '{testResultName}'");

var step = await dataStoreServiceClient.CreateStepAsync(
    new CreateStepRequest
    {
        Step = new Step
        {
            TestResultId = testResult.Id,
            Name = stepName,
        },
    });
EnsureResource(step, $"Step '{stepName}'");

var batchResponse = await dataStoreServiceClient.BatchCreateMeasurementsAsync(
    new BatchCreateMeasurementsRequest
    {
        Requests =
        {
            measurementNames.Select(name => new CreateMeasurementRequest
            {
                Measurement = CreateMeasurement(step.Id, name),
            }),
        },
    }).ResponseAsync;

EnsureBatchResponse(batchResponse.Measurements, measurementNames, step.Id);
await EnsureReadBackAsync(batchResponse.Measurements, measurementNames, step.Id);
Console.WriteLine($"Verified ordered batch creation of {batchResponse.Measurements.Count} measurements.");

await AssertInvalidBatchIsAtomicAsync(step.Id, rejectedMeasurementName);
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

static void EnsureBatchResponse(
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
