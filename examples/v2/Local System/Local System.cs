using NationalInstruments.DataStore.V2.Utilities;
using NationalInstruments.Measurements.Data.V2;
using NationalInstruments.Measurements.Metadata.V1;
using NationalInstruments.SystemConfiguration;
using NiSystemConfiguration = NationalInstruments.SystemConfiguration.SystemConfiguration;

const string testResultNamePrefix = "V2 Local System Snapshot";

var runSuffix = Guid.NewGuid().ToString("N");
var testResultName = $"{testResultNamePrefix} {runSuffix}";

using var dataStoreContext = new DataStoreContext(ParseEndpoint(args));
var metadataStoreServiceClient = dataStoreContext.CreateMetadataStoreClient();
var dataStoreServiceClient = dataStoreContext.CreateDataStoreClient();

Console.WriteLine("Discovering local system metadata...");
var discoveredMetadata = DiscoverSystemMetadata();
var metadataRelationshipIds = await CreateMetadataAsync(
    discoveredMetadata,
    metadataStoreServiceClient);

var testResult = await dataStoreServiceClient.CreateTestResultAsync(
    new CreateTestResultRequest
    {
        TestResult = new TestResult
        {
            Name = testResultName,
            OperatorId = metadataRelationshipIds.OperatorId,
            TestStationId = metadataRelationshipIds.TestStationId,
            HardwareItemIds = { metadataRelationshipIds.HardwareItemIds },
            SoftwareItemIds = { metadataRelationshipIds.SoftwareItemIds },
        },
    });
EnsureResource(testResult, $"TestResult '{testResultName}'");
Console.WriteLine($"Created isolated V2 TestResult ID: {testResult.Id}");

var retrievedTestResult = await dataStoreServiceClient.GetTestResultAsync(
    new GetTestResultRequest
    {
        TestResultId = testResult.Id,
    });
EnsureResource(retrievedTestResult, $"TestResult '{testResult.Id}'");

EnsureMetadataRelationships(retrievedTestResult, metadataRelationshipIds);
Console.WriteLine(
    $"Verified V2 metadata relationships: Operator={metadataRelationshipIds.OperatorId}, " +
    $"TestStation={metadataRelationshipIds.TestStationId}, " +
    $"Hardware={metadataRelationshipIds.HardwareItemIds.Count}, " +
    $"Software={metadataRelationshipIds.SoftwareItemIds.Count}");

static DiscoveredSystemMetadata DiscoverSystemMetadata()
{
    using var session = new NiSystemConfiguration("localhost");
    var system = session.GetSystemResource();
    var hardware = session.FindHardware(
        new Filter(session, FilterMode.MatchValuesAll)
        {
            IsNIProduct = true,
            IsDevice = true,
            IsPresent = IsPresentType.Present,
        });
    using var installedSoftware = session.GetInstalledSoftwareComponents();

    var hardwareItems = new List<HardwareItem>();
    foreach (var resource in hardware)
    {
        var productName = "Unknown Hardware";
        var manufacturer = "N/A";
        var serialNumber = "N/A";
        if (resource is HardwareResource hardwareResource)
        {
            productName = ValueOrUnknown(hardwareResource.ProductName);
            manufacturer = ValueOrUnknown(hardwareResource.VendorName);
            serialNumber = ValueOrUnknown(hardwareResource.SerialNumber);
        }

        hardwareItems.Add(
            new HardwareItem
            {
                Model = productName,
                Manufacturer = manufacturer,
                SerialNumber = serialNumber,
            });
    }
    var softwareItems = installedSoftware
        .OrderBy(component => component.Title, StringComparer.OrdinalIgnoreCase)
        .ThenBy(component => component.DisplayVersion, StringComparer.OrdinalIgnoreCase)
        .Select(
            component => new SoftwareItem
            {
                Product = ValueOrUnknown(component.Title),
                Version = string.IsNullOrWhiteSpace(component.DisplayVersion)
                    ? ValueOrUnknown(component.Version)
                    : component.DisplayVersion,
            })
        .ToList();

    Console.WriteLine(
        $"Discovered user '{Environment.UserName}', test station '{ValueOrUnknown(system.Hostname)}', " +
        $"{hardwareItems.Count} hardware item(s), and {softwareItems.Count} software item(s).");

    return new DiscoveredSystemMetadata(
        new Operator
        {
            Name = Environment.UserName,
            Role = "Unknown",
        },
        new TestStation
        {
            Name = ValueOrUnknown(system.Hostname),
        },
        hardwareItems,
        softwareItems);
}

static async Task<MetadataRelationshipIds> CreateMetadataAsync(
    DiscoveredSystemMetadata discoveredMetadata,
    MetadataStoreService.MetadataStoreServiceClient metadataStoreServiceClient)
{
    var operatorId = await metadataStoreServiceClient.CreateOperatorAsync(discoveredMetadata.Operator);
    var testStationId = await metadataStoreServiceClient.CreateTestStationAsync(discoveredMetadata.TestStation);
    var hardwareItemIds = new List<string>();
    var softwareItemIds = new List<string>();

    foreach (var hardwareItem in discoveredMetadata.HardwareItems)
    {
        hardwareItemIds.Add(await metadataStoreServiceClient.CreateHardwareItemAsync(hardwareItem));
    }

    foreach (var softwareItem in discoveredMetadata.SoftwareItems)
    {
        softwareItemIds.Add(await metadataStoreServiceClient.CreateSoftwareItemAsync(softwareItem));
    }

    EnsureId(operatorId, "operator");
    EnsureId(testStationId, "test station");
    EnsureIds(hardwareItemIds, "hardware item");
    EnsureIds(softwareItemIds, "software item");

    return new MetadataRelationshipIds(operatorId, testStationId, hardwareItemIds, softwareItemIds);
}

static void EnsureMetadataRelationships(
    TestResult testResult,
    MetadataRelationshipIds expectedRelationshipIds)
{
    if (testResult.OperatorId != expectedRelationshipIds.OperatorId ||
        testResult.TestStationId != expectedRelationshipIds.TestStationId ||
        !testResult.HardwareItemIds.SequenceEqual(expectedRelationshipIds.HardwareItemIds) ||
        !testResult.SoftwareItemIds.SequenceEqual(expectedRelationshipIds.SoftwareItemIds))
    {
        throw new InvalidOperationException(
            $"TestResult '{testResult.Id}' did not preserve the submitted V1 metadata relationship IDs.");
    }
}

static void EnsureId(string id, string description)
{
    if (string.IsNullOrWhiteSpace(id))
    {
        throw new InvalidOperationException($"V1 metadata creation returned an empty {description} ID.");
    }
}

static void EnsureIds(IEnumerable<string> ids, string description)
{
    foreach (var id in ids)
    {
        EnsureId(id, description);
    }
}

static string ValueOrUnknown(string? value) =>
    string.IsNullOrWhiteSpace(value) ? "Unknown" : value;

static Uri ParseEndpoint(string[] arguments)
{
    const string endpointOption = "--endpoint";
    var endpoint = "http://localhost:58371";

    for (var index = 0; index < arguments.Length; index++)
    {
        if (arguments[index] != endpointOption || index + 1 == arguments.Length)
        {
            throw new ArgumentException(
                $"Usage: Local System [{endpointOption} <absolute-http-or-https-uri>]");
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

internal sealed record DiscoveredSystemMetadata(
    Operator Operator,
    TestStation TestStation,
    IReadOnlyList<HardwareItem> HardwareItems,
    IReadOnlyList<SoftwareItem> SoftwareItems);

internal sealed record MetadataRelationshipIds(
    string OperatorId,
    string TestStationId,
    IReadOnlyList<string> HardwareItemIds,
    IReadOnlyList<string> SoftwareItemIds);
