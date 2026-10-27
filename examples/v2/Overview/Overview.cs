using NationalInstruments.DataStore.V2.Utilities;
using NationalInstruments.Measurements.Data.V2;
using NationalInstruments.Protobuf.Types;

const double submittedMeasurementValue = 42.0;
const string submittedConditionName = "operating-mode";
const string submittedConditionValue = "nominal";

using var dataStoreContext = new DataStoreContext(ParseEndpoint(args));
var dataStoreServiceClient = dataStoreContext.CreateDataStoreClient();

var testResult = await dataStoreServiceClient.CreateTestResultAsync(
    new CreateTestResultRequest
    {
        TestResult = new TestResult
        {
            Name = "V2 Overview Test Result",
        },
    });
EnsureResource(testResult, "TestResult");
Console.WriteLine($"Created TestResult ID: {testResult.Id}");

var step = await dataStoreServiceClient.CreateStepAsync(
    new CreateStepRequest
    {
        Step = new Step
        {
            TestResultId = testResult.Id,
            Name = "V2 Overview Step",
        },
    });
EnsureResource(step, "Step");
Console.WriteLine($"Created Step ID: {step.Id}");

var submittedCondition = new Condition
{
    Name = submittedConditionName,
    Value = new ConditionValue
    {
        Scalar = new Scalar
        {
            StringValue = submittedConditionValue,
        },
    },
};
var conditionResponse = await dataStoreServiceClient.AddConditionAsync(
    new AddConditionRequest
    {
        StepId = step.Id,
        Condition = submittedCondition,
    });
EnsureResource(conditionResponse.Step, "Step after adding condition");

var measurement = await dataStoreServiceClient.CreateMeasurementAsync(
    new CreateMeasurementRequest
    {
        Measurement = new Measurement
        {
            StepId = step.Id,
            Name = "V2 Overview Scalar Measurement",
            Value = MeasurementValues.From(submittedMeasurementValue),
        },
    });
EnsureResource(measurement, "Measurement");
Console.WriteLine($"Created Measurement ID: {measurement.Id}");

var retrievedTestResult = await dataStoreServiceClient.GetTestResultAsync(
    new GetTestResultRequest
    {
        TestResultId = testResult.Id,
    });
EnsureResource(retrievedTestResult, $"TestResult '{testResult.Id}'");

var retrievedStep = await dataStoreServiceClient.GetStepAsync(
    new GetStepRequest
    {
        StepId = step.Id,
    });
EnsureResource(retrievedStep, $"Step '{step.Id}'");

var retrievedMeasurement = await dataStoreServiceClient.GetMeasurementAsync(
    new GetMeasurementRequest
    {
        MeasurementId = measurement.Id,
    });
EnsureResource(retrievedMeasurement, $"Measurement '{measurement.Id}'");

var measurementValues = await dataStoreServiceClient.ReadMeasurementValuesAsync(
    new ReadMeasurementValuesRequest
    {
        MeasurementId = measurement.Id,
    });
var retrievedMeasurementValue = measurementValues.Value?.Scalar?.DoubleValue;
if (retrievedMeasurementValue != submittedMeasurementValue)
{
    throw new InvalidOperationException(
        $"Measurement '{measurement.Id}' scalar value did not match the submitted value.");
}

if (!retrievedStep.Conditions.Contains(submittedCondition) ||
    !retrievedMeasurement.Conditions.Contains(submittedCondition))
{
    throw new InvalidOperationException(
        $"Measurement '{measurement.Id}' did not contain the condition submitted to Step '{step.Id}'.");
}

Console.WriteLine($"Verified scalar measurement value: {retrievedMeasurementValue}");
Console.WriteLine($"Verified condition propagation: {submittedConditionName}={submittedConditionValue}");

static Uri ParseEndpoint(string[] arguments)
{
    const string endpointOption = "--endpoint";
    var endpoint = "http://localhost:58371";

    for (var index = 0; index < arguments.Length; index++)
    {
        if (arguments[index] != endpointOption || index + 1 == arguments.Length)
        {
            throw new ArgumentException($"Usage: Overview [{endpointOption} <absolute-http-or-https-uri>]");
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
