using NationalInstruments.DataStore.Utilities;
using NationalInstruments.MeasurementLink.Discovery.V1;
using NationalInstruments.Measurements.Data.V1;
using NationalInstruments.Measurements.Metadata.V1;
using NationalInstruments.SystemConfiguration;
using static NationalInstruments.Measurements.Data.V1.DataStoreService;
using static NationalInstruments.Measurements.Metadata.V1.MetadataStoreService;
using NiSystemConfiguration = NationalInstruments.SystemConfiguration.SystemConfiguration;

const string Target = "localhost";

// Initialize DataStoreContext to set up isolated environment
using var dataStoreContext = new DataStoreContext();

// This using statement will ensure that the client stub factory is properly disposed.
using var clientStubFactory = new GrpcClientStubFactory();
var dataStoreServiceClient = clientStubFactory.CreateClient<DataStoreServiceClient>();
var metadataStoreServiceClient = clientStubFactory.CreateClient<MetadataStoreServiceClient>();

Console.WriteLine("=== NI System Configuration Query ===");
Console.WriteLine($"Target: {Target}");

try
{
    var operatorId = await CreateOperatorForCurrentUserAsync(metadataStoreServiceClient);

    Console.WriteLine("Starting session with NI System Configuration...");
    using var session = new NiSystemConfiguration(Target);
    var testStationId = await CreateTestStationForCurrentMachineAsync(session, metadataStoreServiceClient);
    var hardwareItemIds = await CreateHardwareItemsAsync(session, metadataStoreServiceClient);
    var softwareItemIds = await CreateSoftwareItemsAsync(session, metadataStoreServiceClient);
    var testResultId = await CreateTestResultFromIdsAsync(operatorId, testStationId, hardwareItemIds, softwareItemIds, dataStoreServiceClient);

    var testResult = await dataStoreServiceClient.GetTestResultAsync(testResultId);
    Console.WriteLine("\n=== Retrieved Test Result ===");
    Console.WriteLine($"TestResult ID: {testResult.Id}");
    Console.WriteLine($"Operator: {testResult.OperatorId}");
    Console.WriteLine($"Test Station: {testResult.TestStationId}");
    Console.WriteLine($"Installed Software Count: {testResult.SoftwareItemIds.Count}");
    Console.WriteLine($"Hardware Item Count: {testResult.HardwareItemIds.Count}");
}
catch (SystemConfigurationException ex)
{
    Console.WriteLine($"System Configuration error (0x{ex.ErrorCode:X}): {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unhandled error: {ex.Message}");
}

static async Task<string> CreateOperatorForCurrentUserAsync(MetadataStoreServiceClient metadataStoreServiceClient)
{
    string userName = Environment.UserName;
    Console.WriteLine($"Creating operator for current user: {userName}...");
    var @operator = new Operator { Name = userName, Role = "Unknown" };
    return await metadataStoreServiceClient.CreateOperatorAsync(@operator);
}

static async Task<string> CreateTestStationForCurrentMachineAsync(NiSystemConfiguration session, MetadataStoreServiceClient metadataStoreServiceClient)
{
    Console.WriteLine("Getting System Configuration Resource...");
    var system = session.GetSystemResource();
    var hostname = ValueOrUnknown(SafeGet(() => system.Hostname));
    Console.WriteLine($"Creating test station for current machine: {hostname}...");
    var testStation = new TestStation { Name = hostname };
    return await metadataStoreServiceClient.CreateTestStationAsync(testStation);
}

static async Task<IEnumerable<string>> CreateHardwareItemsAsync(NiSystemConfiguration session, MetadataStoreServiceClient metadataStoreServiceClient)
{
    var filter = new Filter(session, FilterMode.MatchValuesAll)
    {
        IsNIProduct = true,
        IsDevice = true,
        IsPresent = IsPresentType.Present,
    };
    var hardware = session.FindHardware(filter);
    Console.WriteLine($"\n--- Hardware Items ({hardware.Count}) ---");
    var hardwareItemIds = new List<string>();

    foreach (var resource in hardware)
    {
        var productName = "Unknown Hardware";
        var vendorName = "N/A";
        var serialNumber = "N/A";
        if (resource is HardwareResource hw)
        {
            productName = ValueOrUnknown(SafeGet(() => hw.ProductName));
            vendorName = ValueOrUnknown(SafeGet(() => hw.VendorName));
            serialNumber = ValueOrUnknown(SafeGet(() => hw.SerialNumber));
        }

        Console.WriteLine($"{productName}");
        var hardwareItem = new HardwareItem
        {
            Model = productName,
            Manufacturer = vendorName,
            SerialNumber = serialNumber,
        };
        var hardwareItemId = await metadataStoreServiceClient.CreateHardwareItemAsync(hardwareItem);
        hardwareItemIds.Add(hardwareItemId);
    }
    return hardwareItemIds;
}

static async Task<IEnumerable<string>> CreateSoftwareItemsAsync(NiSystemConfiguration session, MetadataStoreServiceClient metadataStoreServiceClient)
{
    using var installed = session.GetInstalledSoftwareComponents();
    Console.WriteLine($"\n--- Installed Software ({installed.Count}) ---");

    var sorted = installed
        .OrderBy(component => component.Title, StringComparer.OrdinalIgnoreCase)
        .ThenBy(component => component.DisplayVersion, StringComparer.OrdinalIgnoreCase)
        .ToList();

    var softwareItemIds = new List<string>();
    foreach (var component in sorted)
    {
        var title = ValueOrUnknown(component.Title);
        var version = !string.IsNullOrWhiteSpace(component.DisplayVersion)
            ? component.DisplayVersion
            : ValueOrUnknown(component.Version);

        Console.WriteLine($"{title} {version}");
        var softwareItem = new SoftwareItem
        {
            Product = title,
            Version = version,
        };
        var softwareItemId = await metadataStoreServiceClient.CreateSoftwareItemAsync(softwareItem);
        softwareItemIds.Add(softwareItemId);
    }
    return softwareItemIds;
}

static async Task<string> CreateTestResultFromIdsAsync(
    string operatorId,
    string testStationId,
    IEnumerable<string> hardwareItemIds,
    IEnumerable<string> softwareItemIds,
    DataStoreServiceClient dataStoreServiceClient)
{
    var testResult = new TestResult
    {
        Name = "System Configuration Snapshot",
        OperatorId = operatorId,
        TestStationId = testStationId,
        HardwareItemIds = { hardwareItemIds },
        SoftwareItemIds = { softwareItemIds },
    };
    return await dataStoreServiceClient.CreateTestResultAsync(testResult);
}

static string? SafeGet(Func<string> getter)
{
    try
    {
        return getter();
    }
    catch (SystemConfigurationException)
    {
        return null;
    }
}

static string ValueOrUnknown(string? value)
{
    return string.IsNullOrWhiteSpace(value) ? "Unknown" : value;
}
