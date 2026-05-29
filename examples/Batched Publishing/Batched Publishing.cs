using NationalInstruments;
using NationalInstruments.DataStore.Utilities;
using NationalInstruments.MeasurementLink.Discovery.V1;
using NationalInstruments.Measurements.Data.V1;
using NationalInstruments.Protobuf.Types;
using static NationalInstruments.Measurements.Data.V1.DataStoreService;

// Initialize DataStoreContext to set up isolated environment
using var dataStoreContext = new DataStoreContext();

// This using statement will ensure that the client stub factory is properly disposed.
using var clientStubFactory = new GrpcClientStubFactory();
var dataStoreServiceClient = clientStubFactory.CreateClient<DataStoreServiceClient>();

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

// ── Batch Publishing Condition Values ──────────────────────────────────────

// PublishConditionBatchAsync can be used to publish the value of a given condition
// across N parametric iterations at once. This is equivalent to calling
// PublishConditionAsync for that same condition N times.
//
// Condition values may be supplied as either an IEnumerable or a Vector.
// Supported element types of the IEnumerable are double, int, string, and bool.
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

// Supplying a Vector allows the client to specify additional information, such as units.
Console.WriteLine("Publishing batch of double values using a Vector...");
var vector = new Vector { DoubleArray = new DoubleArray() };
vector.DoubleArray.Values.Add(new double[] { 0.5, 1.0, 1.5, 2.0 });
var vectorConditionId = await dataStoreServiceClient.PublishConditionBatchAsync(
    "Example Vector Condition",
    "Setup",
    vector,
    stepId);

Console.WriteLine($"Published Example Vector Condition ID: {vectorConditionId}");

// Condition values published via PublishConditionBatchAsync are read back in the
// same manner as values published individually via PublishConditionAsync.
// More specifically, they are read back as a Vector containing all N iterations
// of parametric data published for a particular condition.
var readBackDoubleVector = await dataStoreServiceClient.ReadConditionValueAsync<Vector>(doubleConditionId);
var readBackIntVector = await dataStoreServiceClient.ReadConditionValueAsync<Vector>(intConditionId);
var readBackStringVector = await dataStoreServiceClient.ReadConditionValueAsync<Vector>(stringConditionId);
var readBackBoolVector = await dataStoreServiceClient.ReadConditionValueAsync<Vector>(boolConditionId);
var readBackVectorCondition = await dataStoreServiceClient.ReadConditionValueAsync<Vector>(vectorConditionId);

Console.WriteLine($"Read Example Double Condition: {readBackDoubleVector}");
Console.WriteLine($"Read Example Integer Condition: {readBackIntVector}");
Console.WriteLine($"Read Example String Condition: {readBackStringVector}");
Console.WriteLine($"Read Example Bool Condition: {readBackBoolVector}");
Console.WriteLine($"Read Example Vector Condition: {readBackVectorCondition}");

// ── Batch Publishing Measurement Values ──────────────────────────────────────

// Scalar measurements: batch publish standard types in a single call.
// Each call returns a list of IDs. For scalar values only one ID is returned.
Console.WriteLine("Publishing batch of scalar double measurement values...");
var doubleMeasurementIds = await dataStoreServiceClient.PublishMeasurementBatchAsync(
    "Example Double Measurement",
    new double[] { 0.125, 0.25, 0.5, 1.0 },
    stepId);

Console.WriteLine("Publishing batch of scalar integer measurement values...");
var intMeasurementIds = await dataStoreServiceClient.PublishMeasurementBatchAsync(
    "Example Integer Measurement",
    new int[] { 10, 20, 30, 40 },
    stepId);

Console.WriteLine("Publishing batch of scalar string measurement values...");
var stringMeasurementIds = await dataStoreServiceClient.PublishMeasurementBatchAsync(
    "Example String Measurement",
    new string[] { "nominal", "warning", "critical", "retest" },
    stepId);

Console.WriteLine("Publishing batch of scalar boolean measurement values...");
var boolMeasurementIds = await dataStoreServiceClient.PublishMeasurementBatchAsync(
    "Example Bool Measurement",
    new bool[] { false, false, true, true },
    stepId);

Console.WriteLine($"Published Example Double Measurement IDs: {string.Join(", ", doubleMeasurementIds)}");
Console.WriteLine($"Published Example Integer Measurement IDs: {string.Join(", ", intMeasurementIds)}");
Console.WriteLine($"Published Example String Measurement IDs: {string.Join(", ", stringMeasurementIds)}");
Console.WriteLine($"Published Example Bool Measurement IDs: {string.Join(", ", boolMeasurementIds)}");

// Supplying a Vector allows specifying additional information such as units.
Console.WriteLine("Publishing batch of scalar measurements using a Vector...");
var measurementVector = new Vector { DoubleArray = new DoubleArray() };
measurementVector.DoubleArray.Values.Add(new double[] { 1.2, 1.4, 1.6, 1.8 });
var vectorMeasurementIds = await dataStoreServiceClient.PublishMeasurementBatchAsync(
    "Example Published-As-Vector Measurement",
    measurementVector,
    stepId);

Console.WriteLine($"Published Example Published-As-Vector Measurement IDs: {string.Join(", ", vectorMeasurementIds)}");

// Scalar batch measurements are read back as a Vector containing all N iterations.
var readBackDoubleMeasurement = await dataStoreServiceClient.ReadMeasurementValueAsync<Vector>(doubleMeasurementIds[0]);
var readBackIntMeasurement = await dataStoreServiceClient.ReadMeasurementValueAsync<Vector>(intMeasurementIds[0]);
var readBackStringMeasurement = await dataStoreServiceClient.ReadMeasurementValueAsync<Vector>(stringMeasurementIds[0]);
var readBackBoolMeasurement = await dataStoreServiceClient.ReadMeasurementValueAsync<Vector>(boolMeasurementIds[0]);
var readBackVectorMeasurement = await dataStoreServiceClient.ReadMeasurementValueAsync<Vector>(vectorMeasurementIds[0]);

Console.WriteLine($"Read Double Measurement: {readBackDoubleMeasurement}");
Console.WriteLine($"Read Integer Measurement: {readBackIntMeasurement}");
Console.WriteLine($"Read String Measurement: {readBackStringMeasurement}");
Console.WriteLine($"Read Bool Measurement: {readBackBoolMeasurement}");
Console.WriteLine($"Read Published-As-Vector Measurement: {readBackVectorMeasurement}");

// Non-scalar measurements: batch publish two DoubleAnalogWaveform values.
// Each element corresponds to a single parametric iteration, so N IDs are returned.
Console.WriteLine("Publishing batch of DoubleAnalogWaveform measurement values...");
var waveforms = new DoubleAnalogWaveform[]
{
    new DoubleAnalogWaveform { Dt = 0.001, T0 = PrecisionDateTime.UtcNow.ToPrecisionTimestamp() },
    new DoubleAnalogWaveform { Dt = 0.001, T0 = PrecisionDateTime.UtcNow.ToPrecisionTimestamp() },
};
waveforms[0].YData.AddRange(new double[] { 0.0, 0.25, 0.5, 0.75 });
waveforms[1].YData.AddRange(new double[] { 1.0, 0.85, 0.65, 0.4 });
var waveformMeasurementIds = await dataStoreServiceClient.PublishMeasurementBatchAsync(
    "Example AnalogWaveform Measurement",
    waveforms,
    stepId);

// Non-scalar measurements: batch publish two Vector values.
Console.WriteLine("Publishing batch of Vector measurement values...");
var vectorMeasurements = new Vector[]
{
    new Vector { DoubleArray = new DoubleArray() },
    new Vector { DoubleArray = new DoubleArray() },
};
vectorMeasurements[0].DoubleArray.Values.Add(new double[] { 1.0, 1.25, 1.5 });
vectorMeasurements[1].DoubleArray.Values.Add(new double[] { 2.0, 2.25, 2.5 });
var vectorNonScalarMeasurementIds = await dataStoreServiceClient.PublishMeasurementBatchAsync(
    "Example Vector Measurement",
    (IEnumerable<Vector>)vectorMeasurements,
    stepId);

Console.WriteLine($"Published Example AnalogWaveform Measurement IDs: {string.Join(", ", waveformMeasurementIds)}");
Console.WriteLine($"Published Example Vector Measurement IDs: {string.Join(", ", vectorNonScalarMeasurementIds)}");

// Non-scalar batch measurements are read back individually.
// The ParametricIndex of each PublishedMeasurement indicates its publish iteration.
var publishedWaveform0 = await dataStoreServiceClient.GetMeasurementAsync(waveformMeasurementIds[0]);
var publishedWaveform1 = await dataStoreServiceClient.GetMeasurementAsync(waveformMeasurementIds[1]);
var publishedVector0 = await dataStoreServiceClient.GetMeasurementAsync(vectorNonScalarMeasurementIds[0]);
var publishedVector1 = await dataStoreServiceClient.GetMeasurementAsync(vectorNonScalarMeasurementIds[1]);

var readBackWaveform0 = await dataStoreServiceClient.ReadMeasurementValueAsync<DoubleAnalogWaveform>(publishedWaveform0.Id);
var readBackWaveform1 = await dataStoreServiceClient.ReadMeasurementValueAsync<DoubleAnalogWaveform>(publishedWaveform1.Id);
var readBackVector0 = await dataStoreServiceClient.ReadMeasurementValueAsync<Vector>(publishedVector0.Id);
var readBackVector1 = await dataStoreServiceClient.ReadMeasurementValueAsync<Vector>(publishedVector1.Id);

Console.WriteLine($"{publishedWaveform0.Name} at Parametric Index {publishedWaveform0.ParametricIndex}: {readBackWaveform0.YData}");
Console.WriteLine($"{publishedWaveform1.Name} at Parametric Index {publishedWaveform1.ParametricIndex}: {readBackWaveform1.YData}");
Console.WriteLine($"{publishedVector0.Name} at Parametric Index {publishedVector0.ParametricIndex}: {readBackVector0}");
Console.WriteLine($"{publishedVector1.Name} at Parametric Index {publishedVector1.ParametricIndex}: {readBackVector1}");