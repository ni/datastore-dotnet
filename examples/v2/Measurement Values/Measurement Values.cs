using NationalInstruments.DataStore.V2.Utilities;
using NationalInstruments.Measurements.Data.V2;
using NationalInstruments.Protobuf.Types;

const double submittedScalarValue = 42.0;
double[] submittedVectorValues = [1.25, 2.5, 5.0];
const string testResultNamePrefix = "V2 Measurement Values Test Result";
const string stepNamePrefix = "V2 Measurement Values Step";
const string scalarMeasurementNamePrefix = "V2 Measurement Values Scalar";
const string vectorMeasurementNamePrefix = "V2 Measurement Values Vector";

var runSuffix = Guid.NewGuid().ToString("N");
var testResultName = WithRunSuffix(testResultNamePrefix, runSuffix);
var stepName = WithRunSuffix(stepNamePrefix, runSuffix);
var scalarMeasurementName = WithRunSuffix(scalarMeasurementNamePrefix, runSuffix);
var vectorMeasurementName = WithRunSuffix(vectorMeasurementNamePrefix, runSuffix);

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
Console.WriteLine($"Created TestResult ID: {testResult.Id}");

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
Console.WriteLine($"Created Step ID: {step.Id}");

var scalarMeasurement = await CreateMeasurementAsync(
    step.Id,
    scalarMeasurementName,
    MeasurementValues.From(submittedScalarValue));
var vectorMeasurement = await CreateMeasurementAsync(
    step.Id,
    vectorMeasurementName,
    MeasurementValues.From(
        new Vector
        {
            DoubleArray = new DoubleArray
            {
                Values = { submittedVectorValues },
            },
        }));

var scalarResponse = await dataStoreServiceClient.ReadMeasurementValuesAsync(
    new ReadMeasurementValuesRequest
    {
        MeasurementId = scalarMeasurement.Id,
    });
EnsureScalarValue(scalarResponse.Value, scalarMeasurement.Id, submittedScalarValue);

var vectorResponse = await dataStoreServiceClient.ReadMeasurementValuesAsync(
    new ReadMeasurementValuesRequest
    {
        MeasurementId = vectorMeasurement.Id,
    });
EnsureDoubleVectorValue(vectorResponse.Value, vectorMeasurement.Id, submittedVectorValues);

Console.WriteLine($"Verified scalar measurement value: {submittedScalarValue}");
Console.WriteLine($"Verified vector measurement values: {string.Join(", ", submittedVectorValues)}");

async Task<Measurement> CreateMeasurementAsync(
    string stepId,
    string name,
    MeasurementValue value)
{
    var measurement = await dataStoreServiceClient.CreateMeasurementAsync(
        new CreateMeasurementRequest
        {
            Measurement = new Measurement
            {
                StepId = stepId,
                Name = name,
                Value = value,
            },
        });
    EnsureResource(measurement, $"Measurement '{name}'");
    Console.WriteLine($"Created Measurement ID: {measurement.Id}");
    return measurement;
}

static void EnsureScalarValue(MeasurementValue? value, string measurementId, double expectedValue)
{
    if (value is null)
    {
        throw new InvalidOperationException(
            $"Measurement '{measurementId}' returned no value; expected Scalar.DoubleValue={expectedValue}.");
    }

    if (value.ValueCase != MeasurementValue.ValueOneofCase.Scalar)
    {
        throw new InvalidOperationException(
            $"Measurement '{measurementId}' returned union arm '{value.ValueCase}'; expected 'Scalar'.");
    }

    if (value.Scalar.DoubleValue != expectedValue)
    {
        throw new InvalidOperationException(
            $"Measurement '{measurementId}' returned Scalar.DoubleValue={value.Scalar.DoubleValue}; expected {expectedValue}.");
    }
}

static void EnsureDoubleVectorValue(
    MeasurementValue? value,
    string measurementId,
    IReadOnlyList<double> expectedValues)
{
    if (value is null)
    {
        throw new InvalidOperationException(
            $"Measurement '{measurementId}' returned no value; expected Vector.DoubleArray=[{string.Join(", ", expectedValues)}].");
    }

    if (value.ValueCase != MeasurementValue.ValueOneofCase.Vector)
    {
        throw new InvalidOperationException(
            $"Measurement '{measurementId}' returned union arm '{value.ValueCase}'; expected 'Vector'.");
    }

    if (value.Vector.ValueCase != Vector.ValueOneofCase.DoubleArray)
    {
        throw new InvalidOperationException(
            $"Measurement '{measurementId}' returned Vector arm '{value.Vector.ValueCase}'; expected 'DoubleArray'.");
    }

    var actualValues = value.Vector.DoubleArray.Values;
    if (!actualValues.SequenceEqual(expectedValues))
    {
        throw new InvalidOperationException(
            $"Measurement '{measurementId}' returned Vector.DoubleArray=[{string.Join(", ", actualValues)}]; expected [{string.Join(", ", expectedValues)}].");
    }
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
                $"Usage: Measurement Values [{endpointOption} <absolute-http-or-https-uri>]");
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
