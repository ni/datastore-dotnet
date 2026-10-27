using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using NationalInstruments.DataStore.V2.Utilities;
using NationalInstruments.Measurements.Data.V2;
using NationalInstruments.Protobuf.Types;

const string testResultNamePrefix = "V2 Update Delete Test Result";
const string stepNamePrefix = "V2 Update Delete Step";
const string singularMeasurementNamePrefix = "V2 Update Delete Singular Measurement";
const string batchMeasurementNamePrefix = "V2 Update Delete Batch Measurement";
const string updatedMeasurementNamePrefix = "V2 Update Delete Updated Measurement";
const double immutableReplacementValue = 99.0;

var runSuffix = Guid.NewGuid().ToString("N");
var testResultName = WithRunSuffix(testResultNamePrefix, runSuffix);
var stepName = WithRunSuffix(stepNamePrefix, runSuffix);
var singularMeasurementName = WithRunSuffix(singularMeasurementNamePrefix, runSuffix);
var batchMeasurementName = WithRunSuffix(batchMeasurementNamePrefix, runSuffix);
var updatedMeasurementName = WithRunSuffix(updatedMeasurementNamePrefix, runSuffix);

using var dataStoreContext = new DataStoreContext(ParseEndpoint(args));
var dataStoreServiceClient = dataStoreContext.CreateDataStoreClient();
Console.WriteLine($"Run suffix: {runSuffix}");

var testResult = await CreateTestResultAsync(testResultName);
var step = await CreateStepAsync(testResult.Id, stepName);
var singularMeasurement = await CreateMeasurementAsync(step.Id, singularMeasurementName);
var batchMeasurement = await CreateMeasurementAsync(step.Id, batchMeasurementName);

var updateMask = new FieldMask
{
    Paths = { "name" },
};
await dataStoreServiceClient.UpdateMeasurementAsync(
    new UpdateMeasurementRequest
    {
        Measurement = new Measurement
        {
            Id = singularMeasurement.Id,
            Name = updatedMeasurementName,
        },
        UpdateMask = updateMask,
    });

var updatedMeasurement = await dataStoreServiceClient.GetMeasurementAsync(
    new GetMeasurementRequest
    {
        MeasurementId = singularMeasurement.Id,
    });
if (updatedMeasurement.Name != updatedMeasurementName)
{
    throw new InvalidOperationException(
        $"Measurement '{singularMeasurement.Id}' returned Name='{updatedMeasurement.Name}'; expected '{updatedMeasurementName}'.");
}

Console.WriteLine($"Verified mutable update: Name='{updatedMeasurementName}'");

await AssertImmutableValueUpdateIsRejectedAsync(singularMeasurement.Id);
Console.WriteLine("Verified immutable update rejection: Value");

await dataStoreServiceClient.DeleteMeasurementAsync(
    new DeleteMeasurementRequest
    {
        MeasurementId = singularMeasurement.Id,
    });
await AssertMeasurementIsDeletedAsync(singularMeasurement.Id);
Console.WriteLine($"Verified singular delete: {singularMeasurement.Id}");

await dataStoreServiceClient.BatchDeleteMeasurementsAsync(
    new BatchDeleteMeasurementsRequest
    {
        Requests =
        {
            new DeleteMeasurementRequest
            {
                MeasurementId = batchMeasurement.Id,
            },
        },
    });
await AssertMeasurementIsDeletedAsync(batchMeasurement.Id);
Console.WriteLine($"Verified batch delete: {batchMeasurement.Id}");

async Task<TestResult> CreateTestResultAsync(string name)
{
    var testResult = await dataStoreServiceClient.CreateTestResultAsync(
        new CreateTestResultRequest
        {
            TestResult = new TestResult
            {
                Name = name,
            },
        });
    EnsureResource(testResult, $"TestResult '{name}'");
    return testResult;
}

async Task<Step> CreateStepAsync(string testResultId, string name)
{
    var step = await dataStoreServiceClient.CreateStepAsync(
        new CreateStepRequest
        {
            Step = new Step
            {
                TestResultId = testResultId,
                Name = name,
            },
        });
    EnsureResource(step, $"Step '{name}'");
    return step;
}

async Task<Measurement> CreateMeasurementAsync(string stepId, string name)
{
    var measurement = await dataStoreServiceClient.CreateMeasurementAsync(
        new CreateMeasurementRequest
        {
            Measurement = new Measurement
            {
                StepId = stepId,
                Name = name,
                Value = MeasurementValues.From(1.0),
            },
        });
    EnsureResource(measurement, $"Measurement '{name}'");
    return measurement;
}

async Task AssertImmutableValueUpdateIsRejectedAsync(string measurementId)
{
    try
    {
        await dataStoreServiceClient.UpdateMeasurementAsync(
            new UpdateMeasurementRequest
            {
                Measurement = new Measurement
                {
                    Id = measurementId,
                    Value = MeasurementValues.From(immutableReplacementValue),
                },
                UpdateMask = new FieldMask
                {
                    Paths = { "value" },
                },
            });
    }
    catch (RpcException exception)
    {
        if (exception.StatusCode is StatusCode.InvalidArgument or StatusCode.FailedPrecondition &&
            !string.IsNullOrWhiteSpace(exception.Status.Detail))
        {
            return;
        }

        throw new InvalidOperationException(
            $"Measurement '{measurementId}' returned an uninformative immutable-Value update failure: " +
            $"StatusCode={exception.StatusCode}, Detail='{exception.Status.Detail}'.",
            exception);
    }

    throw new InvalidOperationException(
        $"Measurement '{measurementId}' accepted a Value update, but Value is immutable after creation.");
}

async Task AssertMeasurementIsDeletedAsync(string measurementId)
{
    try
    {
        await dataStoreServiceClient.GetMeasurementAsync(
            new GetMeasurementRequest
            {
                MeasurementId = measurementId,
            });
    }
    catch (RpcException exception) when (exception.StatusCode == StatusCode.NotFound)
    {
        return;
    }

    throw new InvalidOperationException(
        $"Measurement '{measurementId}' was still readable after it was deleted.");
}

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
                $"Usage: Update and Delete [{endpointOption} <absolute-http-or-https-uri>]");
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
