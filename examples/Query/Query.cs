using NationalInstruments;
using NationalInstruments.MeasurementLink.Discovery.V1;
using NationalInstruments.Measurements.Data.V1;
using NationalInstruments.Measurements.Metadata.V1;
using NationalInstruments.Protobuf.Types;
using static NationalInstruments.Measurements.Data.V1.DataStoreService;
using static NationalInstruments.Measurements.Metadata.V1.MetadataStoreService;

// Alias constants
const string AliasOperatorAlex = "Operator_Alex_Smith";
const string AliasOperatorJordan = "Operator_Jordan_Chen";
const string AliasOperatorTaylor = "Operator_Taylor_Johnson";
const string AliasStationA1 = "Station_A1";
const string AliasStationB2 = "Station_B2";
const string AliasStationC3 = "Station_C3";
const string AliasDmm = "DMM_PXIe4081";
const string AliasScope = "Scope_PXIe5171";
const string AliasUutPowerSupply = "UUT_PowerSupply_v2_1";
const string AliasUutAudioAmplifier = "UUT_AudioAmplifier_v1_3";
const string AliasSoftwarePython = "Software_Python_3_11_5";
const string AliasSoftwarePytest = "Software_pytest_7_4_0";
const string AliasSoftwareNIDaq = "Software_NI_DAQmx_23_3_0";
const string AliasUutInstancePs1 = "UUT_Instance_PS_001";
const string AliasUutInstancePs2 = "UUT_Instance_PS_002";
const string AliasUutInstanceAmp1 = "UUT_Instance_AMP_001";
const string AllItemsQuery = "";

// Initialize clients
// This using statement will ensure that the client stub factory is properly disposed.
using var clientStubFactory = new GrpcClientStubFactory();
var dataStoreServiceClient = clientStubFactory.CreateClient<DataStoreServiceClient>();
var metadataStoreServiceClient = clientStubFactory.CreateClient<MetadataStoreServiceClient>();
var random = new Random();

Console.WriteLine("=== NI Measurement Data Store Query Examples ===\n");


// Menu-driven interface
while (true)
{
    Console.WriteLine("""

    Select an operation:
    1. Publish Sample Data
    2. Query Measurements
    3. Query Metadata
    4. Exit

    Enter your choice (1-4): 
    """);

    string? choice = Console.ReadLine();

    switch (choice)
    {
        case "1":
            await PublishSampleDataAsync(dataStoreServiceClient, metadataStoreServiceClient, random);
            break;
        case "2":
            await QueryMeasurementsAsync(dataStoreServiceClient, metadataStoreServiceClient, random);
            break;
        case "3":
            await QueryMetadataAsync(metadataStoreServiceClient);
            break;
        case "4":
            Console.WriteLine("\nExiting...");
            return;
        default:
            Console.WriteLine("Invalid choice. Please try again.");
            break;
    }
}

static async Task PublishSampleDataAsync(
    DataStoreServiceClient dataStoreServiceClient,
    MetadataStoreServiceClient metadataStoreServiceClient,
    Random random)
{
    Console.WriteLine("\nStarting sample data creation...");
    Console.WriteLine("Creating comprehensive test metadata and measurements\n");

    try
    {
        // Create Operators
        Console.WriteLine("Creating operators...");
        var operator1 = new Operator { Name = "Alex Smith", Role = "Test Engineer" };
        await metadataStoreServiceClient.CreateOperatorAsync(operator1);
        await metadataStoreServiceClient.CreateAliasAsync(AliasOperatorAlex, operator1);

        var operator2 = new Operator { Name = "Jordan Chen", Role = "Senior Test Engineer" };
        await metadataStoreServiceClient.CreateOperatorAsync(operator2);
        await metadataStoreServiceClient.CreateAliasAsync(AliasOperatorJordan, operator2);

        var operator3 = new Operator { Name = "Taylor Johnson", Role = "Lab Technician" };
        await metadataStoreServiceClient.CreateOperatorAsync(operator3);
        await metadataStoreServiceClient.CreateAliasAsync(AliasOperatorTaylor, operator3);

        // Create Test Stations
        Console.WriteLine("Creating test stations...");
        var station1 = new TestStation { Name = "TestStation_A1" };
        await metadataStoreServiceClient.CreateTestStationAsync(station1);
        await metadataStoreServiceClient.CreateAliasAsync(AliasStationA1, station1);

        var station2 = new TestStation { Name = "TestStation_B2" };
        await metadataStoreServiceClient.CreateTestStationAsync(station2);
        await metadataStoreServiceClient.CreateAliasAsync(AliasStationB2, station2);

        var station3 = new TestStation { Name = "TestStation_C3" };
        await metadataStoreServiceClient.CreateTestStationAsync(station3);
        await metadataStoreServiceClient.CreateAliasAsync(AliasStationC3, station3);

        // Create Hardware Items
        Console.WriteLine("Creating hardware items...");
        var dmm = new HardwareItem
        {
            Manufacturer = "NI",
            Model = "PXIe-4081",
            SerialNumber = "DMM001"
        };
        await metadataStoreServiceClient.CreateHardwareItemAsync(dmm);
        await metadataStoreServiceClient.CreateAliasAsync(AliasDmm, dmm);

        var scope = new HardwareItem
        {
            Manufacturer = "NI",
            Model = "PXIe-5171",
            SerialNumber = "SCOPE001"
        };
        await metadataStoreServiceClient.CreateHardwareItemAsync(scope);
        await metadataStoreServiceClient.CreateAliasAsync(AliasScope, scope);

        // Create UUTs
        Console.WriteLine("Creating UUTs...");
        var powerSupplyUnit = new Uut
        {
            ModelName = "PowerSupply v2.1",
            Family = "Power"
        };
        await metadataStoreServiceClient.CreateUutAsync(powerSupplyUnit);
        await metadataStoreServiceClient.CreateAliasAsync(AliasUutPowerSupply, powerSupplyUnit);

        var amplifierUnit = new Uut
        {
            ModelName = "Audio Amplifier v1.3",
            Family = "Audio"
        };
        await metadataStoreServiceClient.CreateUutAsync(amplifierUnit);
        await metadataStoreServiceClient.CreateAliasAsync(AliasUutAudioAmplifier, amplifierUnit);

        // Create Software Items
        Console.WriteLine("Creating software items...");
        var pythonSoftware = new SoftwareItem
        {
            Product = "Python",
            Version = "3.11.5"
        };
        await metadataStoreServiceClient.CreateSoftwareItemAsync(pythonSoftware);
        await metadataStoreServiceClient.CreateAliasAsync(AliasSoftwarePython, pythonSoftware);

        var pythonTestSoftware = new SoftwareItem
        {
            Product = "pytest",
            Version = "7.4.0"
        };
        await metadataStoreServiceClient.CreateSoftwareItemAsync(pythonTestSoftware);
        await metadataStoreServiceClient.CreateAliasAsync(AliasSoftwarePytest, pythonTestSoftware);

        var daqSoftware = new SoftwareItem
        {
            Product = "NI-DAQmx",
            Version = "23.3.0"
        };
        await metadataStoreServiceClient.CreateSoftwareItemAsync(daqSoftware);
        await metadataStoreServiceClient.CreateAliasAsync(AliasSoftwareNIDaq, daqSoftware);

        // Create UUT Instances
        Console.WriteLine("\nCreating UUT instances...");
        var psInstance1 = new UutInstance
        {
            UutId = AliasUutPowerSupply,
            SerialNumber = "PS-2024-001"
        };
        await metadataStoreServiceClient.CreateUutInstanceAsync(psInstance1);
        await metadataStoreServiceClient.CreateAliasAsync(AliasUutInstancePs1, psInstance1);

        var psInstance2 = new UutInstance
        {
            UutId = AliasUutPowerSupply,
            SerialNumber = "PS-2024-002"
        };
        await metadataStoreServiceClient.CreateUutInstanceAsync(psInstance2);
        await metadataStoreServiceClient.CreateAliasAsync(AliasUutInstancePs2, psInstance2);

        var ampInstance1 = new UutInstance
        {
            UutId = AliasUutAudioAmplifier,
            SerialNumber = "AMP-2024-001"
        };
        await metadataStoreServiceClient.CreateUutInstanceAsync(ampInstance1);
        await metadataStoreServiceClient.CreateAliasAsync(AliasUutInstanceAmp1, ampInstance1);

        // Create Test Results with Measurements
        Console.WriteLine("Creating sample test results with measurements...");

        // Scenario 1: Power Supply Test
        Console.WriteLine("Creating Power Supply test...");
        await CreatePowerSupplyTestAsync(dataStoreServiceClient, random);

        // Scenario 2: Audio Amplifier Test
        Console.WriteLine("Creating Audio Amplifier test...");
        await CreateAudioAmplifierTestAsync(dataStoreServiceClient, random);

        // Scenario 3: Second Power Supply Test
        Console.WriteLine("Creating second Power Supply test...");
        await CreateSecondPowerSupplyTestAsync(dataStoreServiceClient, random);

        Console.WriteLine("Sample data creation complete!");
        Console.WriteLine("Ready to run query examples!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error publishing sample data: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
    }
}

static async Task CreatePowerSupplyTestAsync(DataStoreServiceClient dataStoreServiceClient, Random random)
{
    // Create power supply test result
    var powerTestResult = new TestResult
    {
        UutInstanceId = AliasUutInstancePs1,
        OperatorId = AliasOperatorAlex,
        TestStationId = AliasStationA1,
        Name = "Power Supply Test v2.1"
    };
    powerTestResult.SoftwareItemIds.Add(AliasSoftwarePython);
    powerTestResult.HardwareItemIds.Add(AliasDmm);
    powerTestResult.HardwareItemIds.Add(AliasScope);
    var psTestResultId = await dataStoreServiceClient.CreateTestResultAsync(powerTestResult);

    // Define power supply test steps
    var powerSteps = new[]
    {
        ("Initialize", new[] { "Configure DMM", "Set Load Conditions", "Warm-up Period" }),
        ("Input Voltage Test", new[] { "Measure Input Voltage", "Measure Input Current", "Calculate Input Power" }),
        ("Output Voltage Test", new[] { "Measure 5V Rail", "Measure 12V Rail", "Measure -12V Rail" }),
        ("Load Regulation", new[] { "No Load Test", "25% Load Test", "50% Load Test", "75% Load Test", "Full Load Test" }),
        ("Ripple Measurement", new[] { "5V Ripple", "12V Ripple", "-12V Ripple" })
    };

    int totalMeasurements = 0;
    int powerConditions = 0;

    foreach (var (stepName, measurementNames) in powerSteps)
    {
        var step = new Step
        {
            Name = stepName,
            TestResultId = psTestResultId
        };
        var stepId = await dataStoreServiceClient.CreateStepAsync(step);

        foreach (var measurementName in measurementNames)
        {
            var (value, unit) = GeneratePowerSupplyMeasurement(measurementName, random);
            var outcome = GenerateRandomOutcome(random);

            await dataStoreServiceClient.PublishMeasurementAsync(
                measurementName,
                new Scalar { DoubleValue = value, Units = unit },
                PrecisionDateTime.UtcNow,
                stepId,
                outcome: outcome);

            Console.WriteLine($"Created measurement '{measurementName}' - {(outcome == Outcome.Passed ? "Passed" : "Failed")}");
            totalMeasurements++;
        }

        // Add test conditions
        var stepConditions = new[]
        {
            ("Ambient Temperature", random.NextDouble() * 5 + 21, "deg C"),
            ("Barometric Pressure", random.NextDouble() * 20 + 995, "hPa"),
            ("Line Frequency", random.NextDouble() * 0.2 + 59.9, "Hz")
        };

        foreach (var (conditionName, value, unit) in stepConditions)
        {
            await dataStoreServiceClient.PublishConditionAsync(
                conditionName,
                "Environment",
                new Scalar { DoubleValue = value, Units = unit },
                stepId);
            powerConditions++;
        }
    }

    Console.WriteLine($"Created {totalMeasurements} measurements for Power Supply test");
    Console.WriteLine($"Created {powerConditions} conditions for Power Supply test");
}

static async Task CreateAudioAmplifierTestAsync(DataStoreServiceClient dataStoreServiceClient, Random random)
{
    var amplifierTestResult = new TestResult
    {
        UutInstanceId = AliasUutInstanceAmp1,
        OperatorId = AliasOperatorJordan,
        TestStationId = AliasStationB2,
        Name = "Audio Amplifier Test v1.3"
    };
    amplifierTestResult.SoftwareItemIds.Add(AliasSoftwarePytest);
    amplifierTestResult.HardwareItemIds.Add(AliasDmm);
    amplifierTestResult.HardwareItemIds.Add(AliasScope);
    var ampTestResultId = await dataStoreServiceClient.CreateTestResultAsync(amplifierTestResult);

    var amplifierSteps = new[]
    {
        ("Power-On Test", new[] { "Supply Voltage", "Standby Current", "Enable Signal" }),
        ("DC Offset", new[] { "Left Channel DC Offset", "Right Channel DC Offset" }),
        ("Frequency Response", new[] { "20Hz Response", "1kHz Response", "10kHz Response", "20kHz Response" }),
        ("THD+N Test", new[] { "1kHz THD+N Left", "1kHz THD+N Right", "10kHz THD+N Left", "10kHz THD+N Right" }),
        ("Power Output", new[] { "Max Power Left", "Max Power Right", "Power at 1% THD" }),
        ("Signal-to-Noise", new[] { "SNR Left Channel", "SNR Right Channel" })
    };

    int amplifierMeasurements = 0;
    int amplifierConditions = 0;

    foreach (var (stepName, measurementNames) in amplifierSteps)
    {
        var step = new Step
        {
            Name = stepName,
            TestResultId = ampTestResultId
        };
        var stepId = await dataStoreServiceClient.CreateStepAsync(step);

        foreach (var measurementName in measurementNames)
        {
            var (value, unit) = GenerateAmplifierMeasurement(measurementName, random);
            var outcome = GenerateRandomOutcome(random);

            await dataStoreServiceClient.PublishMeasurementAsync(
                measurementName,
                new Scalar { DoubleValue = value, Units = unit },
                PrecisionDateTime.UtcNow,
                stepId,
                outcome: outcome);
            amplifierMeasurements++;
        }

        // Add test conditions
        var conditions = new[]
        {
            ("Ambient Temperature", random.NextDouble() * 3 + 22, "deg C"),
            ("Relative Humidity", random.NextDouble() * 10 + 45, "%"),
            ("Test Load", random.NextDouble() * 0.4 + 7.8, "ohms")
        };

        foreach (var (conditionName, value, unit) in conditions)
        {
            await dataStoreServiceClient.PublishConditionAsync(
                conditionName,
                "Environment",
                new Scalar { DoubleValue = value, Units = unit },
                stepId);
            amplifierConditions++;
        }
    }

    Console.WriteLine($"Created {amplifierMeasurements} measurements for Audio Amplifier test");
    Console.WriteLine($"Created {amplifierConditions} conditions for Audio Amplifier test");
}

static async Task CreateSecondPowerSupplyTestAsync(DataStoreServiceClient dataStoreServiceClient, Random random)
{
    var powerTest2 = new TestResult
    {
        UutInstanceId = AliasUutInstancePs2,
        OperatorId = AliasOperatorTaylor,
        TestStationId = AliasStationC3,
        Name = "Power Supply Test v2.1"
    };
    powerTest2.SoftwareItemIds.Add(AliasSoftwarePython);
    powerTest2.HardwareItemIds.Add(AliasDmm);
    powerTest2.HardwareItemIds.Add(AliasScope);
    var psTest2ResultId = await dataStoreServiceClient.CreateTestResultAsync(powerTest2);

    var step = new Step
    {
        Name = "Quick Verification",
        TestResultId = psTest2ResultId,
    };
    var stepId = await dataStoreServiceClient.CreateStepAsync(step);

    var quickMeasurements = new[] { "Output 5V", "Output 12V", "Load Current" };
    foreach (var measurementName in quickMeasurements)
    {
        var (value, unit) = GeneratePowerSupplyMeasurement(measurementName, random);
        var outcome = GenerateRandomOutcome(random);

        await dataStoreServiceClient.PublishMeasurementAsync(
            measurementName,
            new Scalar { DoubleValue = value, Units = unit },
            PrecisionDateTime.UtcNow,
            stepId,
            outcome: outcome);
    }

    // Add test condition
    await dataStoreServiceClient.PublishConditionAsync(
        "Room Temperature",
        "Environment",
        new Scalar { DoubleValue = random.NextDouble() * 4 + 20, Units = "deg C" },
        stepId);

    Console.WriteLine("Created 3 measurements for second Power Supply test");
    Console.WriteLine("Created 1 condition for second Power Supply test");
}

static Outcome GenerateRandomOutcome(Random random)
{
    return random.NextDouble() > 0.4 ? Outcome.Passed : Outcome.Failed;
}

static (double Value, string Unit) GeneratePowerSupplyMeasurement(string measurementName, Random random)
{
    if (measurementName.Contains("Voltage") || measurementName.Contains("Input Voltage"))
    {
        if (measurementName.Contains("Input"))
        {
            return (random.NextDouble() * 10 + 115, "V");
        }
        else if (measurementName.Contains("5V"))
        {
            return (random.NextDouble() * 0.1 + 4.95, "V");
        }
        else if (measurementName.Contains("12V") && !measurementName.Contains("-12V"))
        {
            return (random.NextDouble() * 0.4 + 11.8, "V");
        }
        else if (measurementName.Contains("-12V"))
        {
            return (random.NextDouble() * 0.4 - 12.2, "V");
        }
        else
        {
            return (random.NextDouble() * 15, "V");
        }
    }
    else if (measurementName.Contains("Current"))
    {
        if (measurementName.Contains("Input"))
        {
            return (random.NextDouble() * 0.4 + 0.8, "A");
        }
        else
        {
            return (random.NextDouble() * 1.9 + 0.1, "A");
        }
    }
    else if (measurementName.Contains("Power"))
    {
        return (random.NextDouble() * 100 + 50, "W");
    }
    else if (measurementName.Contains("Ripple"))
    {
        return (random.NextDouble() * 40 + 10, "mV");
    }
    else
    {
        return (random.NextDouble() * 100, string.Empty);
    }
}

static (double Value, string Unit) GenerateAmplifierMeasurement(string measurementName, Random random)
{
    if (measurementName.Contains("Voltage") || measurementName.Contains("Supply"))
    {
        return (random.NextDouble() + 14.5, "V");
    }
    else if (measurementName.Contains("Current"))
    {
        return (random.NextDouble() * 0.04 + 0.01, "A");
    }
    else if (measurementName.Contains("DC Offset"))
    {
        return (random.NextDouble() * 10 - 5, "mV");
    }
    else if (measurementName.Contains("Response"))
    {
        return (random.NextDouble() * 2 - 1, "dB");
    }
    else if (measurementName.Contains("THD"))
    {
        return (random.NextDouble() * 0.009 + 0.001, "%");
    }
    else if (measurementName.Contains("Power"))
    {
        if (measurementName.Contains("Max"))
        {
            return (random.NextDouble() * 10 + 45, "W");
        }
        else
        {
            return (random.NextDouble() * 10 + 40, "W");
        }
    }
    else if (measurementName.Contains("SNR"))
    {
        return (random.NextDouble() * 10 + 95, "dB");
    }
    else
    {
        return (random.NextDouble() * 10, string.Empty);
    }
}

static async Task QueryMeasurementsAsync(DataStoreServiceClient dataStoreServiceClient, MetadataStoreServiceClient metadataStoreServiceClient, Random random)
{
    Console.WriteLine("\n=== Query Measurements ===\n");

    try
    {
        // Query failed measurements
        Console.WriteLine("1. Failed Measurements:");
        var allMeasurements = await dataStoreServiceClient.QueryMeasurementsAsync(AllItemsQuery);
        var failedMeasurements = allMeasurements.Where(m => m.Outcome == Outcome.Failed).ToList();

        foreach (var measurement in failedMeasurements.Take(15))
        {
            var measurementName = measurement.Name ?? "Unnamed Measurement";
            var dateTime = measurement.StartDateTime?.ToString() ?? "Unknown Time";
            Console.WriteLine($"{measurementName} - Failed at {dateTime}");
        }

        if (failedMeasurements.Count > 15)
        {
            Console.WriteLine($"  ... and {failedMeasurements.Count - 15} more measurements");
        }

        if (failedMeasurements.Count == 0)
        {
            Console.WriteLine("No failed measurements found!");
        }

        // Query voltage measurements
        Console.WriteLine("\n=== Voltage Measurements ===");
        var voltageMeasurements = await dataStoreServiceClient.QueryMeasurementsAsync("$filter=contains(Name,'Voltage')");
        foreach (var measurement in voltageMeasurements)
        {
            Console.WriteLine($"{PrintMeasurementWithOutcome(measurement)}");
        }

        // Query current measurements
        Console.WriteLine("\n=== Current Measurements ===");
        var currentMeasurements = await dataStoreServiceClient.QueryMeasurementsAsync("$filter=contains(Name,'Current')");
        foreach (var measurement in currentMeasurements)
        {
            Console.WriteLine($"{PrintMeasurementWithOutcome(measurement)}");
        }

        // Query failed measurements by operator
        Console.WriteLine("\n=== Failed Measurements by Operator Alex Smith ===");
        foreach (var measurement in failedMeasurements)
        {
            if (!string.IsNullOrEmpty(measurement.TestResultId))
            {
                var testResult = await dataStoreServiceClient.GetTestResultAsync(measurement.TestResultId);
                if (testResult is not null && !string.IsNullOrEmpty(testResult.OperatorId))
                {
                    var operatorEntity = await metadataStoreServiceClient.GetOperatorAsync(testResult.OperatorId);
                    if (operatorEntity is not null && operatorEntity.Name == "Alex Smith")
                    {
                        Console.WriteLine($"{PrintMeasurementWithOutcome(measurement)} - Operator: {operatorEntity.Name}");
                    }
                }
            }
        }

        // Query all steps
        Console.WriteLine("\n=== Steps ===");
        var allSteps = await dataStoreServiceClient.QueryStepsAsync(AllItemsQuery);
        Console.WriteLine($"Found {allSteps.Count} steps total");
        Console.WriteLine("\nStep summary:");

        foreach (var step in allSteps.Take(10))
        {
            var stepName = step.Name ?? "Unnamed Step";
            Console.WriteLine($"{stepName}");
        }

        if (allSteps.Count > 10)
        {
            Console.WriteLine($"  ... and {allSteps.Count - 10} more steps");
        }

        // Query steps containing 'Voltage'
        Console.WriteLine("\n=== Steps containing 'Voltage' ===");
        var voltageSteps = await dataStoreServiceClient.QueryStepsAsync("$filter=contains(Name,'Voltage')");
        foreach (var step in voltageSteps)
        {
            var stepName = step.Name ?? "Unnamed Step";
            Console.WriteLine($"{stepName}");
        }

        // Query conditions
        Console.WriteLine("\n=== Conditions (Temperature and Pressure) ===");
        var tempAndPressureConditions = await dataStoreServiceClient.QueryConditionsAsync(
            "$filter=contains(Name,'Temperature') or contains(Name,'Pressure')");

        var uniqueConditionNames = tempAndPressureConditions
            .Select(c => c.Name ?? "Unnamed Condition")
            .Distinct()
            .OrderBy(n => n)
            .ToList();

        if (tempAndPressureConditions.Any())
        {
            foreach (var conditionName in uniqueConditionNames.Take(10))
            {
                Console.WriteLine($"{conditionName}");
            }

            if (tempAndPressureConditions.Count > 10)
            {
                Console.WriteLine($"  ... and {tempAndPressureConditions.Count - 10} more conditions");
            }
        }
        else
        {
            Console.WriteLine("No published conditions found in the sample data");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nError querying measurements: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
    }
}

static string PrintMeasurementWithOutcome(PublishedMeasurement measurement)
{
    var name = measurement.Name ?? "Unnamed Measurement";
    return $"{name} - {measurement.Outcome}";
}

static async Task QueryMetadataAsync(MetadataStoreServiceClient metadataStoreServiceClient)
{
    Console.WriteLine("\n=== Query Metadata ===\n");

    try
    {
        // Query operators
        Console.WriteLine("=== Operators ===");
        Console.WriteLine("\nFiltered operators (by name containing 'Smith'):");
        var operatorsNamedSmith = await metadataStoreServiceClient.QueryOperatorsAsync("$filter=contains(Name,'Smith')");
        foreach (var @operator in operatorsNamedSmith)
        {
            Console.WriteLine($"  {@operator.Name} ({@operator.Role})");
        }

        Console.WriteLine("\nFiltered operators (by role containing 'Test Engineer'):");
        var testEngineerOperators = await metadataStoreServiceClient.QueryOperatorsAsync("$filter=contains(Role,'Test Engineer')");
        foreach (var @operator in testEngineerOperators)
        {
            Console.WriteLine($"  {@operator.Name} ({@operator.Role})");
        }

        // Query test stations
        Console.WriteLine("\n=== Test Stations ===");
        Console.WriteLine("\nFiltered test stations (by name containing 'A1'):");
        var a1TestStations = await metadataStoreServiceClient.QueryTestStationsAsync("$filter=contains(Name,'A1')");
        foreach (var station in a1TestStations)
        {
            Console.WriteLine($"  {station.Name}");
        }

        Console.WriteLine("\nFiltered test stations (by name exactly 'TestStation_B2'):");
        var exactB2TestStations = await metadataStoreServiceClient.QueryTestStationsAsync("$filter=Name eq 'TestStation_B2'");
        foreach (var station in exactB2TestStations)
        {
            Console.WriteLine($"  {station.Name}");
        }

        Console.WriteLine("\nFiltered test stations (by name starting with 'Test'):");
        var startsTestStations = await metadataStoreServiceClient.QueryTestStationsAsync("$filter=startswith(Name,'Test')");
        foreach (var station in startsTestStations)
        {
            Console.WriteLine($"  {station.Name}");
        }

        // Query hardware items
        Console.WriteLine("\n=== Hardware Items ===");
        Console.WriteLine("\nFiltered hardware items (by manufacturer 'NI'):");
        var filteredHardware = await metadataStoreServiceClient.QueryHardwareItemsAsync("$filter=Manufacturer eq 'NI'");
        foreach (var item in filteredHardware)
        {
            Console.WriteLine($"  {item.Manufacturer} {item.Model} (S/N: {item.SerialNumber})");
        }

        Console.WriteLine("\nFiltered hardware items (by model containing '4081'):");
        var modelFiltered = await metadataStoreServiceClient.QueryHardwareItemsAsync("$filter=contains(Model,'4081')");
        foreach (var item in modelFiltered)
        {
            Console.WriteLine($"  {item.Manufacturer} {item.Model} (S/N: {item.SerialNumber})");
        }

        Console.WriteLine("\nFiltered hardware items (by serial number containing 'SCOPE'):");
        var serialFiltered = await metadataStoreServiceClient.QueryHardwareItemsAsync("$filter=contains(SerialNumber,'SCOPE')");
        foreach (var item in serialFiltered)
        {
            Console.WriteLine($"  {item.Manufacturer} {item.Model} (S/N: {item.SerialNumber})");
        }

        // Query UUTs
        Console.WriteLine("\n=== UUTs (Units Under Test) ===");
        Console.WriteLine("\nFiltered UUTs (by name containing 'Power'):");
        var filteredUuts = await metadataStoreServiceClient.QueryUutsAsync("$filter=contains(ModelName,'Power')");
        foreach (var uut in filteredUuts)
        {
            Console.WriteLine($"  {uut.ModelName}");
        }

        Console.WriteLine("\nFiltered UUTs (by name ending with 'v1.3'):");
        var v13Uuts = await metadataStoreServiceClient.QueryUutsAsync("$filter=endswith(ModelName,'v1.3')");
        foreach (var uut in v13Uuts)
        {
            Console.WriteLine($"  {uut.ModelName}");
        }

        // Query UUT instances
        Console.WriteLine("\n=== UUT Instances ===");
        Console.WriteLine("\nFiltered UUT instances (by serial containing '2024'):");
        var filteredInstances = await metadataStoreServiceClient.QueryUutInstancesAsync("$filter=contains(SerialNumber,'2024')");
        foreach (var instance in filteredInstances)
        {
            Console.WriteLine($"  Serial: {instance.SerialNumber}");
        }

        Console.WriteLine("\nFiltered UUT instances (by serial starting with 'PS'):");
        var psInstances = await metadataStoreServiceClient.QueryUutInstancesAsync("$filter=startswith(SerialNumber,'PS')");
        foreach (var instance in psInstances)
        {
            Console.WriteLine($"  Serial: {instance.SerialNumber}");
        }

        Console.WriteLine("\nCombined filter - UUT instances (serial contains 'AMP' and '2024'):");
        var instancesCombined = await metadataStoreServiceClient.QueryUutInstancesAsync(
            "$filter=contains(SerialNumber,'AMP') and contains(SerialNumber,'2024')");
        foreach (var instance in instancesCombined)
        {
            Console.WriteLine($"  Serial: {instance.SerialNumber}");
        }

        // Query software items
        Console.WriteLine("\n=== Software Items ===");
        Console.WriteLine("\nFiltered software items (by product 'Python'):");
        var filteredSoftware = await metadataStoreServiceClient.QuerySoftwareItemsAsync("$filter=Product eq 'Python'");
        foreach (var item in filteredSoftware)
        {
            Console.WriteLine($"  {item.Product} {item.Version}");
        }

        Console.WriteLine("\nFiltered software items (by version starting with '3.'):");
        var versionFiltered = await metadataStoreServiceClient.QuerySoftwareItemsAsync("$filter=startswith(Version,'3.')");
        foreach (var item in versionFiltered)
        {
            Console.WriteLine($"  {item.Product} {item.Version}");
        }

        // Query aliases
        Console.WriteLine("\n=== Aliases ===");
        Console.WriteLine("\nFiltered aliases (by name containing 'Operator'):");
        var filteredAliases = await metadataStoreServiceClient.QueryAliasesAsync("$filter=contains(Name,'Operator')");
        foreach (var alias in filteredAliases)
        {
            var targetTypeName = GetTargetTypeName(alias.TargetType);
            Console.WriteLine($"  {alias.Name} -> {targetTypeName}");
        }

        Console.WriteLine("\nFiltered aliases (by name starting with 'UUT'):");
        var uutAliases = await metadataStoreServiceClient.QueryAliasesAsync("$filter=startswith(Name,'UUT')");
        foreach (var alias in uutAliases)
        {
            var targetTypeName = GetTargetTypeName(alias.TargetType);
            Console.WriteLine($"  {alias.Name} -> {targetTypeName}");
        }

        Console.WriteLine("\nFiltered aliases (by target type 'TestStation'):");
        var testStationAliases = await metadataStoreServiceClient.QueryAliasesAsync(
            "$filter=TargetType eq DataStore.AliasTargetType'TestStation'");
        foreach (var alias in testStationAliases)
        {
            var targetTypeName = GetTargetTypeName(alias.TargetType);
            Console.WriteLine($"  {alias.Name} -> {targetTypeName}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nError querying metadata: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
    }
}

static string GetTargetTypeName(AliasTargetType targetType)
{
    return targetType switch
    {
        AliasTargetType.Unspecified => "Unspecified",
        AliasTargetType.UutInstance => "UUT Instance",
        AliasTargetType.Uut => "UUT",
        AliasTargetType.HardwareItem => "Hardware Item",
        AliasTargetType.SoftwareItem => "Software Item",
        AliasTargetType.Operator => "Operator",
        AliasTargetType.TestDescription => "Test Description",
        AliasTargetType.Test => "Test",
        AliasTargetType.TestStation => "Test Station",
        AliasTargetType.TestAdapter => "Test Adapter",
        _ => $"Unknown ({(int)targetType})"
    };
}
