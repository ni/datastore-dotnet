using NationalInstruments;
using NationalInstruments.MeasurementLink.Discovery.V1;
using NationalInstruments.Measurements.Data.V1;
using NationalInstruments.Measurements.Metadata.V1;
using NationalInstruments.Protobuf.Types;
using static NationalInstruments.Measurements.Data.V1.DataStoreService;
using static NationalInstruments.Measurements.Metadata.V1.MetadataStoreService;

var clientStubFactory = new GrpcClientStubFactory();
var dataStoreServiceClient = clientStubFactory.CreateClient<DataStoreServiceClient>();
var metadataStoreServiceClient = clientStubFactory.CreateClient<MetadataStoreServiceClient>();

// Register multiple hardware schemas.
Console.WriteLine("Registering hardware schemas...");
var currentFilePath = GetCurrentFilePath();
var currentDirectory = Path.GetDirectoryName(currentFilePath) ?? currentFilePath;
var cableSchemaPath = Path.Combine(currentDirectory, "cable_schema.toml");
var socketSchemaPath = Path.Combine(currentDirectory, "socket_schema.toml");
var scopeSchemaPath = Path.Combine(currentDirectory, "scope_schema.toml");
var testResultSchemaPath = Path.Combine(currentDirectory, "test_result_schema.toml");
var cableSchemaId = await metadataStoreServiceClient.RegisterSchemaFromFileAsync(cableSchemaPath);
var socketSchemaId = await metadataStoreServiceClient.RegisterSchemaFromFileAsync(socketSchemaPath);
var scopeSchemaId = await metadataStoreServiceClient.RegisterSchemaFromFileAsync(scopeSchemaPath);
var testResultSchemaId = await metadataStoreServiceClient.RegisterSchemaFromFileAsync(testResultSchemaPath);
Console.WriteLine("Finished registering hardware schemas.");

// Create a hardware item that follows the cable schema.
Console.WriteLine("Creating hardware items with extension attributes...");
var cable = new HardwareItem
{
    Manufacturer = "NI",
    Model = "cable",
    SerialNumber = "7u2349",
    SchemaId = cableSchemaId
};
cable.Extension.Add("cable_length", new ExtensionValue { StringValue = "1.5" });
cable.Extension.Add("manufacture_date", new ExtensionValue { StringValue = "2023-01-01" });

// Create a hardware item that follows the socket schema.
var socket = new HardwareItem
{
    Manufacturer = "NI",
    Model = "socket",
    SchemaId = socketSchemaId,
};
socket.Extension.Add("socket_number", new ExtensionValue { StringValue = "1.5" });
socket.Extension.Add("manufacture_date", new ExtensionValue { StringValue = "2024-05-01" });

// Create a hardware item that follows the scope schema.
var scope = new HardwareItem
{
    Manufacturer = "NI",
    Model = "PXIe-5171",
    SerialNumber = "1933B4E",
    SchemaId = scopeSchemaId,
};

// Create a test result referencing the new HardwareItems.
var cableId = await metadataStoreServiceClient.CreateHardwareItemAsync(cable);
var socketId = await metadataStoreServiceClient.CreateHardwareItemAsync(cable);
var scopeId = await metadataStoreServiceClient.CreateHardwareItemAsync(cable);
Console.WriteLine("Finished creating hardware items.");

Console.WriteLine("Creating test result and publishing waveform data...");
var testResultId = await dataStoreServiceClient.CreateTestResultAsync(new TestResult
{
    Name = "scope measurements",
    SchemaId = testResultSchemaId,
    HardwareItemIds = { cableId, socketId, scopeId }
});

// Create some AnalogWaveform data to publish
var doubleWaveform = new DoubleAnalogWaveform
{
    Dt = 0.001
};
doubleWaveform.YData.AddRange(new double[] { 1.0, 2.0, 3.0 });

// Create a Step and publish the waveform data.
var stepId = await dataStoreServiceClient.CreateStepAsync(new Step
{
    Name = "initial step",
    TestResultId = testResultId,
});
await dataStoreServiceClient.PublishMeasurementAsync("scope reading", doubleWaveform, PrecisionDateTime.UtcNow, stepId);
Console.WriteLine("Finished publishing waveform data.");

// Query for the published data using an OData query.
Console.WriteLine("Querying for published measurements...");
var publishedMeasurements = await dataStoreServiceClient.QueryMeasurementsAsync($"TestResultId eq {testResultId} and Name eq 'scope reading'");
if (publishedMeasurements.Count > 0)
{
    var measurement = publishedMeasurements[0];
    var retrievedWaveform = await dataStoreServiceClient.ReadMeasurementValueAsync<DoubleAnalogWaveform>(measurement.Id);
    Console.WriteLine($"Retrieved waveform with data {retrievedWaveform.YData}.");
}

// Cleanup
clientStubFactory.Dispose();

static string GetCurrentFilePath([System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
{
    return filePath;
}
