using NationalInstruments;
using NationalInstruments.DataStore.Utilities;
using NationalInstruments.MeasurementLink.Discovery.V1;
using NationalInstruments.Measurements.Data.V1;
using NationalInstruments.Measurements.Metadata.V1;
using NationalInstruments.Protobuf.Types;
using static NationalInstruments.Measurements.Data.V1.DataStoreService;
using static NationalInstruments.Measurements.Metadata.V1.MetadataStoreService;

// Initialize DataStoreContext to set up isolated environment
using var context = new DataStoreContext();

// This using statement will ensure that the client stub factory is properly disposed.
using var clientStubFactory = new GrpcClientStubFactory();
var dataStoreServiceClient = clientStubFactory.CreateClient<DataStoreServiceClient>();
var metadataStoreServiceClient = clientStubFactory.CreateClient<MetadataStoreServiceClient>();

// Register a metadata schema.
Console.WriteLine("Registering metadata schema...");
var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
var schemaId = await metadataStoreServiceClient.RegisterSchemaFromFileAsync(Path.Combine(appDirectory, "sample_schema.json"));

// Create an operator with a badge number extension attribute.
Console.WriteLine("Creating operator...");
var @operator = new Operator
{
    Name = "John Doe",
    SchemaId = schemaId,
};
@operator.Extension["badge_number"] = new ExtensionValue { StringValue = "emp-128256" };
var operatorId = await metadataStoreServiceClient.CreateOperatorAsync(@operator);

// Create a test station with a valid location extension attribute.
Console.WriteLine("Creating test station...");
var testStation = new TestStation
{
    Name = "TestStation_12",
    SchemaId = schemaId,
};
testStation.Extension["location"] = new ExtensionValue { StringValue = "USA" };
var testStationId = await metadataStoreServiceClient.CreateTestStationAsync(testStation);

// Create a software item with a valid license extension attribute.
Console.WriteLine("Creating software item...");
var softwareItem = new SoftwareItem
{
    Product = "Windows",
    Version = "10.0.19044",
    SchemaId = schemaId,
};
softwareItem.Extension["license"] = new ExtensionValue { StringValue = "LIC_enterprise" };
var softwareItemId = await metadataStoreServiceClient.CreateSoftwareItemAsync(softwareItem);

// Create a test result that includes the created metadata objects.
Console.WriteLine("Creating test result with references to created metadata objects...");
var testResult = new TestResult
{
    Name = "TestResult_1",
    OperatorId = operatorId,
    TestStationId = testStationId,
    SchemaId = schemaId,
};
testResult.SoftwareItemIds.Add(softwareItemId);
testResult.Extension["session_file_path"] = new ExtensionValue { StringValue = "C:\\my_test_description.xlsx" };
var testResultId = await dataStoreServiceClient.CreateTestResultAsync(testResult);

// Publish waveform data for the created test result.
Console.WriteLine("Publishing waveform data for the created test result...");
var doubleWaveform = new DoubleAnalogWaveform
{
    Dt = 0.001,
    T0 = PrecisionDateTime.UtcNow.ToPrecisionTimestamp(),
};
doubleWaveform.YData.AddRange(new double[] { 1.0, 2.0, 3.0 });
var stepId = await dataStoreServiceClient.CreateStepAsync(new Step
{
    Name = "initial step",
    TestResultId = testResultId,
});
var measurementId = await dataStoreServiceClient.PublishMeasurementAsync("data publish sample", doubleWaveform, PrecisionDateTime.UtcNow, stepId);

// Retrieve the published measurement by ID.
Console.WriteLine("Retrieving the published measurement...");
var publishedMeasurement = await dataStoreServiceClient.GetMeasurementAsync(measurementId);
if (publishedMeasurement is not null)
{
    var retrievedWaveform = await dataStoreServiceClient.ReadMeasurementValueAsync<DoubleAnalogWaveform>(publishedMeasurement.Id);
    Console.WriteLine($"Found waveform with data {retrievedWaveform.YData}.");
}
else
{
    Console.WriteLine("Measurement not found.");
}
