using NationalInstruments;
using NationalInstruments.MeasurementLink.Discovery.V1;
using NationalInstruments.Measurements.Data.V1;
using NationalInstruments.Measurements.Metadata.V1;
using NationalInstruments.Protobuf.Types;
using static NationalInstruments.Measurements.Data.V1.DataStoreService;
using static NationalInstruments.Measurements.Metadata.V1.MetadataStoreService;

// This using statement will ensure that the client stub factory is properly disposed.
using var clientStubFactory = new GrpcClientStubFactory();
var dataStoreServiceClient = clientStubFactory.CreateClient<DataStoreServiceClient>();
var metadataStoreServiceClient = clientStubFactory.CreateClient<MetadataStoreServiceClient>();

// Register multiple hardware schemas.
Console.WriteLine("Registering hardware schemas...");
var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
var cableSchemaId = await metadataStoreServiceClient.RegisterSchemaFromFileAsync(Path.Combine(appDirectory, "cable_schema.toml"));
var socketSchemaId = await metadataStoreServiceClient.RegisterSchemaFromFileAsync(Path.Combine(appDirectory, "socket_schema.toml"));
var scopeSchemaId = await metadataStoreServiceClient.RegisterSchemaFromFileAsync(Path.Combine(appDirectory, "scope_schema.toml"));
var testResultSchemaId = await metadataStoreServiceClient.RegisterSchemaFromFileAsync(Path.Combine(appDirectory, "test_result_schema.toml"));

// Create a hardware item that follows the cable schema.
Console.WriteLine("Creating hardware items with extension attributes...");
var cable = new HardwareItem
{
    Manufacturer = "NI",
    Model = "cable",
    SerialNumber = "7u2349",
    SchemaId = cableSchemaId
};
cable.Extension["cable_length"] = new ExtensionValue { StringValue = "1.5" };
cable.Extension["manufacture_date"] = new ExtensionValue { StringValue = "2023-01-01" };

// Create a hardware item that follows the socket schema.
var socket = new HardwareItem
{
    Manufacturer = "NI",
    Model = "socket",
    SchemaId = socketSchemaId,
};
socket.Extension["socket_number"] = new ExtensionValue { StringValue = "1.5" };
socket.Extension["manufacture_date"] = new ExtensionValue { StringValue = "2024-05-01" };

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
var socketId = await metadataStoreServiceClient.CreateHardwareItemAsync(socket);
var scopeId = await metadataStoreServiceClient.CreateHardwareItemAsync(scope);

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
    Dt = 0.001,
    T0 = PrecisionDateTime.UtcNow.ToPrecisionTimestamp(),
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

// Query to find all measurements that use a hardware item with a cable_length
// of 1.5 and have the name "scope reading".
Console.WriteLine("Querying for published measurements...");
var oDataQuery = "$filter=testresult/hardwareitems/any (h: h/extension/cable_length eq '1.5') and Name eq 'scope reading'";
var publishedMeasurements = await dataStoreServiceClient.QueryMeasurementsAsync(oDataQuery);
var foundMeasurement = publishedMeasurements.FirstOrDefault();
if (foundMeasurement is not null)
{
    var retrievedWaveform = await dataStoreServiceClient.ReadMeasurementValueAsync<DoubleAnalogWaveform>(foundMeasurement.Id);
    Console.WriteLine($"Retrieved waveform with data {retrievedWaveform.YData}.");
}
else
{
    Console.WriteLine("No measurements found matching the query criteria.");
}
