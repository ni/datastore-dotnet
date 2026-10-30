using Google.Protobuf.WellKnownTypes;
using NationalInstruments.DataStore.V2.Utilities;
using NationalInstruments.Measurements.Data.V2;
using NationalInstruments.Protobuf.Types;

const string firstTestResultNamePrefix = "V2 Query Test Result One";
const string secondTestResultNamePrefix = "V2 Query Test Result Two";
const string firstStepNamePrefix = "V2 Query Step One";
const string secondStepNamePrefix = "V2 Query Step Two";
const string firstMeasurementNamePrefix = "V2 Query Measurement One";
const string secondMeasurementNamePrefix = "V2 Query Measurement Two";
const double firstMeasurementValue = 101.0;
const double secondMeasurementValue = 202.0;

var runSuffix = Guid.NewGuid().ToString("N");
var firstTestResultName = WithRunSuffix(firstTestResultNamePrefix, runSuffix);
var secondTestResultName = WithRunSuffix(secondTestResultNamePrefix, runSuffix);
var firstStepName = WithRunSuffix(firstStepNamePrefix, runSuffix);
var secondStepName = WithRunSuffix(secondStepNamePrefix, runSuffix);
var firstMeasurementName = WithRunSuffix(firstMeasurementNamePrefix, runSuffix);
var secondMeasurementName = WithRunSuffix(secondMeasurementNamePrefix, runSuffix);

using var dataStoreContext = new DataStoreContext(ParseEndpoint(args));
var dataStoreServiceClient = dataStoreContext.CreateDataStoreClient();
Console.WriteLine($"Run suffix: {runSuffix}");

var firstTestResult = await CreateTestResultAsync(firstTestResultName);
var secondTestResult = await CreateTestResultAsync(secondTestResultName);
var firstStep = await CreateStepAsync(firstTestResult.Id, firstStepName);
var secondStep = await CreateStepAsync(secondTestResult.Id, secondStepName);
var firstMeasurement = await CreateMeasurementAsync(firstStep.Id, firstMeasurementName, firstMeasurementValue);
var secondMeasurement = await CreateMeasurementAsync(secondStep.Id, secondMeasurementName, secondMeasurementValue);

var testResultFilter = FilterByName(firstTestResultName);
var testResultQuery = await dataStoreServiceClient.QueryTestResultsAsync(
    new QueryTestResultsRequest
    {
        OdataQuery = testResultFilter,
    }).ResponseAsync;
EnsureOnlyResource(testResultQuery.TestResults.Select(testResult => testResult.Id), firstTestResult.Id, testResultFilter);
Console.WriteLine($"Verified filter: {testResultFilter}");
Console.WriteLine($"Records read: {testResultQuery.TestResults.Count}");

var stepFilter = FilterByName(firstStepName);
var stepQuery = await dataStoreServiceClient.QueryStepsAsync(
    new QueryStepsRequest
    {
        OdataQuery = stepFilter,
    }).ResponseAsync;
EnsureOnlyResource(stepQuery.Steps.Select(step => step.Id), firstStep.Id, stepFilter);
Console.WriteLine($"Verified filter: {stepFilter}");
Console.WriteLine($"Records read: {stepQuery.Steps.Count}");

var measurementFilter = FilterByName(firstMeasurementName);
var measurementQuery = await dataStoreServiceClient.QueryMeasurementsAsync(
    new QueryMeasurementsRequest
    {
        OdataQuery = measurementFilter,
    }).ResponseAsync;
EnsureOnlyResource(measurementQuery.Measurements.Select(measurement => measurement.Id), firstMeasurement.Id, measurementFilter);
Console.WriteLine($"Verified filter: {measurementFilter}");
Console.WriteLine($"Records read: {measurementQuery.Measurements.Count}");

var readMask = new FieldMask
{
    Paths = { "id" },
};
var readMaskQuery = await dataStoreServiceClient.QueryTestResultsAsync(
    new QueryTestResultsRequest
    {
        OdataQuery = testResultFilter,
        ReadMask = readMask,
    }).ResponseAsync;
EnsureOnlyResource(readMaskQuery.TestResults.Select(testResult => testResult.Id), firstTestResult.Id, testResultFilter);
if (!string.IsNullOrEmpty(readMaskQuery.TestResults[0].Name))
{
    throw new InvalidOperationException(
        $"Resource '{firstTestResult.Id}' returned the unrequested Name field.");
}

Console.WriteLine($"Verified read-mask fields: {string.Join(", ", readMask.Paths)}");
Console.WriteLine($"Records read: {readMaskQuery.TestResults.Count}");

var paginationFilter =
    $"$filter=Name eq '{firstTestResultName}' or Name eq '{secondTestResultName}'&$top=1";
var firstPage = await dataStoreServiceClient.QueryTestResultsAsync(
    new QueryTestResultsRequest
    {
        OdataQuery = paginationFilter,
    }).ResponseAsync;
if (firstPage.TestResults.Count != 1 || string.IsNullOrEmpty(firstPage.NextLink))
{
    throw new InvalidOperationException(
        $"Filter '{paginationFilter}' did not return a continuation for an additional record.");
}

var secondPage = await dataStoreServiceClient.QueryTestResultsAsync(
    new QueryTestResultsRequest
    {
        OdataQuery = firstPage.NextLink,
    }).ResponseAsync;
if (secondPage.TestResults.Count != 1 || !string.IsNullOrEmpty(secondPage.NextLink))
{
    throw new InvalidOperationException(
        $"Filter '{paginationFilter}' did not terminate after the final record.");
}

var firstPageId = firstPage.TestResults[0].Id;
var secondPageId = secondPage.TestResults[0].Id;
if (firstPageId == secondPageId)
{
    throw new InvalidOperationException(
        $"Resource '{secondPageId}' was duplicated across query pages.");
}

HashSet<string> pagedTestResultIds = [firstPageId, secondPageId];
if (!pagedTestResultIds.SetEquals([firstTestResult.Id, secondTestResult.Id]))
{
    throw new InvalidOperationException(
        $"Filter '{paginationFilter}' did not return the expected resources across query pages.");
}

Console.WriteLine($"Verified filter: {paginationFilter}");
Console.WriteLine($"Records read: {firstPage.TestResults.Count + secondPage.TestResults.Count}");

async Task<TestResult> CreateTestResultAsync(string name)
{
    var testResult = await dataStoreServiceClient.CreateTestResultAsync(
        new CreateTestResultRequest
        {
            TestResult = new TestResult
            {
                Name = name,
            },
        });
    EnsureResource(testResult, $"TestResult '{name}'");
    return testResult;
}

async Task<Step> CreateStepAsync(string testResultId, string name)
{
    var step = await dataStoreServiceClient.CreateStepAsync(
        new CreateStepRequest
        {
            Step = new Step
            {
                TestResultId = testResultId,
                Name = name,
            },
        });
    EnsureResource(step, $"Step '{name}'");
    return step;
}

async Task<Measurement> CreateMeasurementAsync(string stepId, string name, double value)
{
    var measurement = await dataStoreServiceClient.CreateMeasurementAsync(
        new CreateMeasurementRequest
        {
            Measurement = new Measurement
            {
                StepId = stepId,
                Name = name,
                Value = MeasurementValues.From(value),
            },
        });
    EnsureResource(measurement, $"Measurement '{name}'");
    return measurement;
}

static string FilterByName(string name) => $"$filter=Name eq '{name}'";

static string WithRunSuffix(string namePrefix, string runSuffix) => $"{namePrefix} {runSuffix}";

static void EnsureOnlyResource(
    IEnumerable<string> resourceIds,
    string expectedId,
    string filter)
{
    var resourceIdList = resourceIds.ToList();
    if (resourceIdList.Count != 1 || resourceIdList[0] != expectedId)
    {
        throw new InvalidOperationException(
            $"Filter '{filter}' did not return only resource '{expectedId}'.");
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
            throw new ArgumentException($"Usage: Query [{endpointOption} <absolute-http-or-https-uri>]");
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
