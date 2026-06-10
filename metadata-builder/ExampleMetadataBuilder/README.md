# ExampleMetadataBuilder

This example demonstrates how to implement the `IMetadataBuilder` interface to attach metadata to test results, steps, measurements, and conditions published from TestStand.

`IMetadataBuilder` is the plugin interface provided by the `NI.Measurements.MetadataBuilder` package. TestStand loads a class that implements this interface (identified by its assembly name) and calls its methods at key points during sequence execution, giving the implementation an opportunity to enrich published data with additional metadata.

The `ExampleMetadataBuilder` class implements the following methods:

* `BuildMetadataForTestResultAsync`
* `BuildMetadataForStepAsync`
* `BuildMetadataForPublishMeasurementAsync`
* `BuildMetadataForPublishConditionAsync`

For this example, the primary behavior is implemented in `BuildMetadataForTestResultAsync`. In that method, we attach a single hardware item and a single software item to the test result. In the other interface methods, we demonstrate adding simple notes metadata.

The class uses `GrpcClientStubFactory` to create a `MetadataStoreServiceClient`, communicating with the Metadata Store over gRPC using the NI Discovery service to locate the endpoint.

## Building and Using This Example

1. Open a command prompt and navigate to `<datastore-dotnet>/metadata-builder`.
2. Build the project by running `dotnet publish -c Release -o publish`.
3. Copy the `<datastore-dotnet>/metadata-builder/publish` folder to the desired location on your machine.
4. Open the Configure >> Result Processing menu.
5. Ensure the MDS Data Store plugin is enabled and navigate to its configuration menu.
6. Check the `Include custom metadata` checkbox
7. Select `NI.Measurements.Data.V1.ExampleMetadataBuilder.dll` from the publish directory as the Custom metadata assembly.
8. Ensure `ExampleMetadataBuilder` is selected for Custom metadata class.

For more information on steps 5-8, see see the [Logging Metadata for Analysis](https://www.ni.com/docs/en-US/bundle/teststand/page/logging-system-metadata-for-analysis.html) topic in the Teststand documentation.
