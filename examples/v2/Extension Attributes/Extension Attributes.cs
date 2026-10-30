using Google.Protobuf.WellKnownTypes;
using NationalInstruments.DataStore.V2.Utilities;
using NationalInstruments.Measurements.Data.V2;
using NationalInstruments.Measurements.Metadata.V1;

const string extensionKey = "operator";
const string extensionValue = "example-operator";
const string testResultNamePrefix = "V2 Extension Attributes Test Result";

var runSuffix = Guid.NewGuid().ToString("N");
var testResultName = $"{testResultNamePrefix} {runSuffix}";

using var dataStoreContext = new DataStoreContext(ParseEndpoint(args));
var metadataStoreServiceClient = dataStoreContext.CreateMetadataStoreClient();
var dataStoreServiceClient = dataStoreContext.CreateDataStoreClient();

var schemaPath = Path.Combine(AppContext.BaseDirectory, "test_result_schema.toml");
var schemaId = await metadataStoreServiceClient.RegisterSchemaFromFileAsync(schemaPath);
if (string.IsNullOrWhiteSpace(schemaId))
{
    throw new InvalidOperationException("V1 metadata schema registration returned an empty schema ID.");
}

Console.WriteLine($"Registered V1 metadata schema ID: {schemaId}");

var testResult = await dataStoreServiceClient.CreateTestResultAsync(
    new CreateTestResultRequest
    {
        TestResult = new TestResult
        {
            Name = testResultName,
            SchemaId = schemaId,
            Extension = new Struct
            {
                Fields =
                {
                    [extensionKey] = Value.ForString(extensionValue),
                },
            },
        },
    });
EnsureResource(testResult, $"TestResult '{testResultName}'");
Console.WriteLine($"Created V2 TestResult ID: {testResult.Id}");

var retrievedTestResult = await dataStoreServiceClient.GetTestResultAsync(
    new GetTestResultRequest
    {
        TestResultId = testResult.Id,
    });
EnsureResource(retrievedTestResult, $"TestResult '{testResult.Id}'");

EnsureExtensionValue(retrievedTestResult, schemaId, extensionKey, extensionValue);
Console.WriteLine($"Verified V2 extension: {extensionKey}={extensionValue}");

static void EnsureExtensionValue(
    TestResult testResult,
    string expectedSchemaId,
    string expectedKey,
    string expectedValue)
{
    if (testResult.SchemaId != expectedSchemaId)
    {
        throw new InvalidOperationException(
            $"TestResult '{testResult.Id}' returned SchemaId '{testResult.SchemaId}'; expected '{expectedSchemaId}'.");
    }

    if (!testResult.Extension.Fields.TryGetValue(expectedKey, out var actualValue))
    {
        throw new InvalidOperationException(
            $"TestResult '{testResult.Id}' did not contain extension key '{expectedKey}'.");
    }

    if (actualValue.KindCase != Value.KindOneofCase.StringValue ||
        actualValue.StringValue != expectedValue)
    {
        throw new InvalidOperationException(
            $"TestResult '{testResult.Id}' returned extension '{expectedKey}' as '{actualValue}'; expected string '{expectedValue}'.");
    }
}

static Uri ParseEndpoint(string[] arguments)
{
    const string endpointOption = "--endpoint";
    var endpoint = "http://localhost:58371";

    for (var index = 0; index < arguments.Length; index++)
    {
        if (arguments[index] != endpointOption || index + 1 == arguments.Length)
        {
            throw new ArgumentException(
                $"Usage: Extension Attributes [{endpointOption} <absolute-http-or-https-uri>]");
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
