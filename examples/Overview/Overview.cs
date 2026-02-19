using NationalInstruments;
using NationalInstruments.MeasurementLink.Discovery.V1;
using NationalInstruments.Measurements.Data.V1;
using NationalInstruments.Measurements.Metadata.V1;
using NationalInstruments.Protobuf.Types;
using static NationalInstruments.Measurements.Data.V1.DataStoreService;
using static NationalInstruments.Measurements.Metadata.V1.MetadataStoreService;

using var clientStubFactory = new GrpcClientStubFactory();
var dataStoreServiceClient = clientStubFactory.CreateClient<DataStoreServiceClient>();
var metadataStoreServiceClient = clientStubFactory.CreateClient<MetadataStoreServiceClient>();

// Register a metadata schema.
Console.WriteLine("Registering metadata schema...");
var currentFilePath = GetCurrentFilePath();
var currentDirectory = Path.GetDirectoryName(currentFilePath) ?? throw new InvalidOperationException("Unable to determine directory path");
var schemaPath = Path.Combine(currentDirectory, "sample_schema.json");
var schemaId = await metadataStoreServiceClient.RegisterSchemaFromFileAsync(schemaPath);

// Invalid creation of operator - no badge number provided.
// This will result in an exception that we will catch below.
Console.WriteLine("Creating operator...");
var @operator = new Operator
{
    Name = "John Doe",
    SchemaId = schemaId,
};
string? operatorId = null;
try
{
    operatorId = await metadataStoreServiceClient.CreateOperatorAsync(@operator);
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to create operator: {ex.Message}\n");
}

// Add the required badge number and recreate.
Console.WriteLine("Adding badge number and retrying...\n");
@operator.Extension.Add("badge_number", new ExtensionValue { StringValue = "emp-128256" });
operatorId = await metadataStoreServiceClient.CreateOperatorAsync(@operator);

// Create a test station with an invalid location extension attribute.
// This will result in an exception that we will catch below.
Console.WriteLine("Creating test station...");
var testStation = new TestStation
{
    Name = "TestStation_12",
    SchemaId = schemaId,
};
testStation.Extension.Add("location", new ExtensionValue { StringValue = "Texas" });
string? testStationId = null;
try
{
    testStationId = await metadataStoreServiceClient.CreateTestStationAsync(testStation);
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to create test station: {ex.Message}\n");
}

// Fix the location extension attribute and recreate the test station.
Console.WriteLine("Fixing location and retrying...\n");
testStation.Extension["location"] = new ExtensionValue { StringValue = "USA" };
testStationId = await metadataStoreServiceClient.CreateTestStationAsync(testStation);

// Create a software item with an invalid license extension attribute.
// This will result in an exception that we will catch below.
Console.WriteLine("Creating software item...");
var softwareItem = new SoftwareItem
{
    Product = "Windows",
    Version = "10.0.19044",
    SchemaId = schemaId,
};
softwareItem.Extension.Add("license", new ExtensionValue { StringValue = "enterprise_LIC" });
string? softwareItemId = null;
try
{
    softwareItemId = await metadataStoreServiceClient.CreateSoftwareItemAsync(softwareItem);
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to create software item: {ex.Message}\n");
}

// Fix the license extension attribute and recreate the software item.
Console.WriteLine("Fixing license and retrying...\n");
softwareItem.Extension["license"] = new ExtensionValue { StringValue = "LIC_enterprise" };
softwareItemId = await metadataStoreServiceClient.CreateSoftwareItemAsync(softwareItem);

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
testResult.Extension.Add("session_file_path", new ExtensionValue { StringValue = "C:\\my_test_description.xlsx" });
var testResultId = await dataStoreServiceClient.CreateTestResultAsync(testResult);

// Publish waveform data for the created test result.
Console.WriteLine("Publishing waveform data for the created test result...");
var doubleWaveform = new DoubleAnalogWaveform
{
    Dt = 0.001
};
doubleWaveform.YData.AddRange(new double[] { 1.0, 2.0, 3.0 });
var stepId = await dataStoreServiceClient.CreateStepAsync(new Step
{
    Name = "initial step",
    TestResultId = testResultId,
});
var measurementId = await dataStoreServiceClient.PublishMeasurementAsync("data publish sample", doubleWaveform, PrecisionDateTime.UtcNow, stepId);

// Query for the published data using an OData query.
Console.WriteLine("Retrieving the published measurement...");
var publishedMeasurement = await dataStoreServiceClient.GetMeasurementAsync(measurementId);
if (publishedMeasurement != null)
{
    var retrievedWaveform = await dataStoreServiceClient.ReadMeasurementValueAsync<DoubleAnalogWaveform>(publishedMeasurement.Id);
    Console.WriteLine($"Found waveform with data {retrievedWaveform.YData}.");
}
else
{
    Console.WriteLine("Measurement not found.");
}

static string GetCurrentFilePath([System.Runtime.CompilerServices.CallerFilePath] string filePath = "")
{
    return filePath;
}
