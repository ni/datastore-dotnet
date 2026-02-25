using NationalInstruments.DataStore.Utilities;
using NationalInstruments.MeasurementLink.Discovery.V1;
using NationalInstruments.Measurements.Data.V1;
using NationalInstruments.Measurements.Metadata.V1;
using static NationalInstruments.Measurements.Data.V1.DataStoreService;
using static NationalInstruments.Measurements.Metadata.V1.MetadataStoreService;

// Initialize DataStoreContext to set up isolated environment
using var context = new DataStoreContext();

// This using statement will ensure that the client stub factory is properly disposed.
using var clientStubFactory = new GrpcClientStubFactory();
var dataStoreServiceClient = clientStubFactory.CreateClient<DataStoreServiceClient>();
var metadataStoreServiceClient = clientStubFactory.CreateClient<MetadataStoreServiceClient>();

// Create operators to alias
var operatorOne = new Operator { Name = "Jane Doe" };
await metadataStoreServiceClient.CreateOperatorAsync(operatorOne);
var operatorTwo = new Operator { Name = "John Smith" };
await metadataStoreServiceClient.CreateOperatorAsync(operatorTwo);

// Create aliases for the operators
await metadataStoreServiceClient.CreateAliasAsync("primary_operator", operatorOne);
await metadataStoreServiceClient.CreateAliasAsync("secondary_operator", operatorTwo);
Console.WriteLine("Aliases created successfully.");

// Create a test result using the secondary_operator alias.
var testResult = new TestResult
{
    Name = "Test Result with Operator Alias",
    OperatorId = "secondary_operator",
};
var testResultId = await dataStoreServiceClient.CreateTestResultAsync(testResult);
Console.WriteLine("The test result was created successfully using an alias to reference an operator.");

// Retrieve the test result and verify which operator was associated with it.
var retrievedTestResult = await dataStoreServiceClient.GetTestResultAsync(testResultId);
var retrievedOperator = await metadataStoreServiceClient.GetOperatorAsync(retrievedTestResult.OperatorId);
Console.WriteLine($"The name of the operator for the test result: {retrievedOperator.Name}.");

// Get the primary_operator alias and print the alias information.
Console.WriteLine("Alias information for primary_operator:");
var aliasInfo = await metadataStoreServiceClient.GetAliasAsync("primary_operator");
printAlias(aliasInfo);
printSeparator();

// Query all aliases and print their information.
Console.WriteLine("Query all aliases:");
var aliases = await metadataStoreServiceClient.QueryAliasesAsync(string.Empty);
foreach (var alias in aliases)
{
    printAlias(alias);
}
printSeparator();

// Query aliases with a filter and print their information.
Console.WriteLine("Query aliases with filter (name eq 'primary_operator'):");
aliases = await metadataStoreServiceClient.QueryAliasesAsync("$filter=name eq 'primary_operator'");
foreach (var alias in aliases)
{
    printAlias(alias);
}
printSeparator();

// Delete an alias. This does not delete the underlying metadata.
var deleted = await metadataStoreServiceClient.DeleteAliasAsync("primary_operator");
Console.WriteLine($"Success of deleting the primary_operator alias: {deleted}.");

// Query as again to verify deletion.
aliases = await metadataStoreServiceClient.QueryAliasesAsync(string.Empty);
Console.WriteLine("Query all aliases after deletion:");
foreach (var alias in aliases)
{
    printAlias(alias);
}
printSeparator();

// Try to delete the alias again. This should return false as the alias no longer exists.
deleted = await metadataStoreServiceClient.DeleteAliasAsync("primary_operator");
Console.WriteLine($"Success of deleting the primary_operator alias again: {deleted}.");

static void printAlias(Alias alias)
{
    Console.WriteLine($"Alias Name: {alias.Name}\tTarget Type: {alias.TargetType}.");
}

static void printSeparator()
{
    Console.WriteLine("-------\n");
}
