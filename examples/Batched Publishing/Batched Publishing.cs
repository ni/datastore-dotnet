using NationalInstruments.DataStore.Utilities;
using NationalInstruments.MeasurementLink.Discovery.V1;
using NationalInstruments.Measurements.Data.V1;
using NationalInstruments.Measurements.Metadata.V1;
using NationalInstruments.Protobuf.Types;
using static NationalInstruments.Measurements.Data.V1.DataStoreService;
using static NationalInstruments.Measurements.Metadata.V1.MetadataStoreService;

// Initialize DataStoreContext to set up isolated environment
using var dataStoreContext = new DataStoreContext();

// This using statement will ensure that the client stub factory is properly disposed.
using var clientStubFactory = new GrpcClientStubFactory();
var dataStoreServiceClient = clientStubFactory.CreateClient<DataStoreServiceClient>();
var metadataStoreServiceClient = clientStubFactory.CreateClient<MetadataStoreServiceClient>();

// Create a test result to associate with the batch of conditions.
Console.WriteLine("Creating test result...");
var testResult = new TestResult { Name = "Batched Publishing Example" };
var testResultId = await dataStoreServiceClient.CreateTestResultAsync(testResult);

// Create a step to associate with the batch of conditions.
Console.WriteLine("Creating step...");
var stepId = await dataStoreServiceClient.CreateStepAsync(new Step
{
    Name = "batch publishing step",
    TestResultId = testResultId,
});

// Publishing a lists of standard types (double, int, etc.) in a single call.
Console.WriteLine("Publishing batch of double condition values...");
var doubleConditionValues = new double[] { 1.25, 2.5, 3.75, 5.0 };
var doubleConditionId = await dataStoreServiceClient.PublishConditionBatchAsync(
    "Example Double Condition",
    "Setup",
    doubleConditionValues,
    stepId);

Console.WriteLine("Publishing batch of integer condition values...");
var intConditionValues = new int[] { 1, 2, 3, 4 };
var intConditionId = await dataStoreServiceClient.PublishConditionBatchAsync(
    "Example Integer Condition",
    "Setup",
    intConditionValues,
    stepId);

Console.WriteLine("Publishing batch of string condition values...");
var stringConditionValues = new string[] { "cold", "ambient", "warm", "hot" };
var stringConditionId = await dataStoreServiceClient.PublishConditionBatchAsync(
    "Example String Condition",
    "Setup",
    stringConditionValues,
    stepId);

Console.WriteLine("Publishing batch of boolean condition values...");
var boolConditionValues = new bool[] { true, false, true, false };
var boolConditionId = await dataStoreServiceClient.PublishConditionBatchAsync(
    "Example Bool Condition",
    "Setup",
    boolConditionValues,
    stepId);

Console.WriteLine($"Published Example Double Condition ID: {doubleConditionId}");
Console.WriteLine($"Published Example Integer Condition ID: {intConditionId}");
Console.WriteLine($"Published Example String Condition ID: {stringConditionId}");
Console.WriteLine($"Published Example Bool Condition ID: {boolConditionId}");

// Publish a batch of double values using a Vector.
Console.WriteLine("Publishing batch of double values using a Vector...");
var vector = new Vector { DoubleArray = new DoubleArray() };
vector.DoubleArray.Values.Add(new double[] { 0.5, 1.0, 1.5, 2.0 });
var vectorConditionId = await dataStoreServiceClient.PublishConditionBatchAsync(
    "Example Vector Condition",
    "Setup",
    vector,
    stepId);

Console.WriteLine($"Published Example Vector Condition ID: {vectorConditionId}");

// Read back the published conditions
var readBackDoubleVector = await dataStoreServiceClient.ReadConditionValueAsync<Vector>(doubleConditionId);
var readBackIntVector = await dataStoreServiceClient.ReadConditionValueAsync<Vector>(intConditionId);
var readBackStringVector = await dataStoreServiceClient.ReadConditionValueAsync<Vector>(stringConditionId);
var readBackBoolVector = await dataStoreServiceClient.ReadConditionValueAsync<Vector>(boolConditionId);

Console.WriteLine($"Read Example Double Condition: {readBackDoubleVector}");
Console.WriteLine($"Read Example Integer Condition: {readBackIntVector}");
Console.WriteLine($"Read Example String Condition: {readBackStringVector}");
Console.WriteLine($"Read Example Bool Condition: {readBackBoolVector}");