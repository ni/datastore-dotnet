using NI.DataStore.Utilities;
using NationalInstruments.MeasurementLink.Discovery.V1;
using NationalInstruments.Measurements.Data.V1;
using NationalInstruments.Measurements.Metadata.V1;
using static NationalInstruments.Measurements.Data.V1.DataStoreService;
using static NationalInstruments.Measurements.Metadata.V1.MetadataStoreService;

// Initialize DataStoreContext to set up isolated environment
using var context = new DataStoreContext();

var clientStubFactory = new GrpcClientStubFactory(discoveryClient: context.CreateDiscoveryClient());
var dataStoreServiceClient = clientStubFactory.CreateClient<DataStoreServiceClient>();
var metadataStoreServiceClient = clientStubFactory.CreateClient<MetadataStoreServiceClient>();

// Register multiple hardware schemas.
var cableSchemaId = await metadataStoreServiceClient.RegisterSchemaFromFileAsync("cable_schema.toml");
var socketSchemaId = await metadataStoreServiceClient.RegisterSchemaFromFileAsync("socket_schema.toml");
var scopeSchemaId = await metadataStoreServiceClient.RegisterSchemaFromFileAsync("scope_schema.toml");
var testResultSchemaId = await metadataStoreServiceClient.RegisterSchemaFromFileAsync("test_result_schema.toml");

// To be completed later once we've reviewed one example.
