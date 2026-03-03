using NationalInstruments.DataStore.Utilities;
using NationalInstruments.MeasurementLink.Discovery.V1;
using NationalInstruments.Measurements.Data.V1;
using NationalInstruments.Measurements.Metadata.V1;
using NationalInstruments.SystemConfiguration;
using static NationalInstruments.Measurements.Data.V1.DataStoreService;
using static NationalInstruments.Measurements.Metadata.V1.MetadataStoreService;
using NiSystemConfiguration = NationalInstruments.SystemConfiguration.SystemConfiguration;

// Initialize DataStoreContext to set up isolated environment
using var dataStoreContext = new DataStoreContext();

// This using statement will ensure that the client stub factory is properly disposed.
using var clientStubFactory = new GrpcClientStubFactory();
var dataStoreServiceClient = clientStubFactory.CreateClient<DataStoreServiceClient>();
var metadataStoreServiceClient = clientStubFactory.CreateClient<MetadataStoreServiceClient>();

Console.WriteLine("Scanning system for metadata...");
var systemMetadata = DetectSystemResources();

Console.WriteLine("\nPublishing detected system metadata...");
var testResultId = await PublishEmptyTestResultAsync(
    systemMetadata,
    dataStoreServiceClient,
    metadataStoreServiceClient);

var testResult = await dataStoreServiceClient.GetTestResultAsync(testResultId);
Console.WriteLine($"\nTestResult ID: {testResult.Id}");
Console.WriteLine($"Operator: {testResult.OperatorId}");
Console.WriteLine($"Test Station: {testResult.TestStationId}");
Console.WriteLine($"Installed Software: {testResult.SoftwareItemIds.Count}");
Console.WriteLine($"Available Hardware: {testResult.HardwareItemIds.Count}");

static SystemMetadata DetectSystemResources()
{
    using var session = new NiSystemConfiguration("localhost");
    var @operator = CreateOperatorForCurrentUser();
    var testStation = CreateTestStationForLocalMachine(session);
    var hardwareItems = CreateHardwareItems(session);
    var softwareItems = CreateSoftwareItems(session);

    var systemMetadata = new SystemMetadata(@operator, testStation, hardwareItems, softwareItems);
    return systemMetadata;
}

static async Task<string> PublishEmptyTestResultAsync(
    SystemMetadata systemMetadata,
    DataStoreServiceClient dataStoreServiceClient,
    MetadataStoreServiceClient metadataStoreServiceClient)
{
    var operatorId = await metadataStoreServiceClient.CreateOperatorAsync(systemMetadata.Operator);
    var testStationId = await metadataStoreServiceClient.CreateTestStationAsync(systemMetadata.TestStation);
    var hardwareItemIds = new List<string>();
    var softwareItemIds = new List<string>();
    foreach (HardwareItem item in systemMetadata.HardwareItems)
    {
        var hardwareItemId = await metadataStoreServiceClient.CreateHardwareItemAsync(item);
        hardwareItemIds.Add(hardwareItemId);
    }
    foreach (SoftwareItem item in systemMetadata.SoftwareItems)
    {
        var softwareItemId = await metadataStoreServiceClient.CreateSoftwareItemAsync(item);
        softwareItemIds.Add(softwareItemId);
    }

    var testResult = new TestResult
    {
        Name = "System Configuration Snapshot",
        OperatorId = operatorId,
        TestStationId = testStationId,
    };
    testResult.HardwareItemIds.AddRange(hardwareItemIds);
    testResult.SoftwareItemIds.AddRange(softwareItemIds);
    return await dataStoreServiceClient.CreateTestResultAsync(testResult);
}

static Operator CreateOperatorForCurrentUser()
{
    string userName = Environment.UserName;
    Console.WriteLine($"Creating operator for current user: {userName}...");
    return new Operator { Name = userName, Role = "Unknown" };
}

static TestStation CreateTestStationForLocalMachine(NiSystemConfiguration session)
{
    Console.WriteLine("Getting System Configuration Resource...");
    var system = session.GetSystemResource();
    var hostname = ValueOrUnknown(system.Hostname);
    Console.WriteLine($"Creating test station for local machine: {hostname}...");
    return new TestStation { Name = hostname };
}

static IReadOnlyList<HardwareItem> CreateHardwareItems(NiSystemConfiguration session)
{
    var filter = new Filter(session, FilterMode.MatchValuesAll)
    {
        IsNIProduct = true,
        IsDevice = true,
        IsPresent = IsPresentType.Present,
    };
    var hardware = session.FindHardware(filter);
    Console.WriteLine($"\n--- Hardware Items ({hardware.Count}) ---");
    var hardwareItems = new List<HardwareItem>();

    foreach (var resource in hardware)
    {
        var productName = "Unknown Hardware";
        var vendorName = "N/A";
        var serialNumber = "N/A";
        if (resource is HardwareResource hw)
        {
            productName = ValueOrUnknown(hw.ProductName);
            vendorName = ValueOrUnknown(hw.VendorName);
            serialNumber = ValueOrUnknown(hw.SerialNumber);
        }

        Console.WriteLine($"{productName}");
        var hardwareItem = new HardwareItem
        {
            Model = productName,
            Manufacturer = vendorName,
            SerialNumber = serialNumber,
        };
        hardwareItems.Add(hardwareItem);
    }
    return hardwareItems;
}

static IReadOnlyList<SoftwareItem> CreateSoftwareItems(NiSystemConfiguration session)
{
    using var installed = session.GetInstalledSoftwareComponents();
    Console.WriteLine($"\n--- Installed Software ({installed.Count}) ---");

    var sorted = installed
        .OrderBy(component => component.Title, StringComparer.OrdinalIgnoreCase)
        .ThenBy(component => component.DisplayVersion, StringComparer.OrdinalIgnoreCase)
        .ToList();

    var softwareItems = new List<SoftwareItem>();
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
        softwareItems.Add(softwareItem);
    }
    return softwareItems;
}

static string ValueOrUnknown(string? value)
{
    return string.IsNullOrWhiteSpace(value) ? "Unknown" : value;
}

public class SystemMetadata
{
    public SystemMetadata(
        Operator operatorValue,
        TestStation testStation,
        IReadOnlyList<HardwareItem> hardwareItems,
        IReadOnlyList<SoftwareItem> softwareItems)
    {
        Operator = operatorValue;
        TestStation = testStation;
        HardwareItems = hardwareItems;
        SoftwareItems = softwareItems;
    }

    public Operator Operator { get; }
    public TestStation TestStation { get; }
    public IReadOnlyList<HardwareItem> HardwareItems { get; }
    public IReadOnlyList<SoftwareItem> SoftwareItems { get; }
}
