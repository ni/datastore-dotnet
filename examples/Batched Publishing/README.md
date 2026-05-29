# Batched Publishing Example

This example demonstrates how to use the `DataStoreServiceClient` to batch publish N iterations of data within a given `Step`. Rather than calling `publishConditionAsync` or `publishMeasurementAsync` N times to publish data for each of these N iterations, this data can instead be published by a single call to `PublishConditionBatchAsync` or `PublishMeasurementBatchAsync`, respectively. Batch publishing can help improve overall publishing performance.

**Note:** These batching APIs handle batch publishing N iterations of data for a single condition or measurement with the specified name. They do *not* support publishing data across multiple (distinctly named) conditions or multiple (distinctly named) measurements at once.

## Running this example

1. Open a command prompt and navigate to `<datastore-dotnet>/examples`.
2. Build the examples by running `dotnet build`
3. Execute the overview example by running `dotnet run --project "Batched Publishing/Batched Publishing.csproj"`