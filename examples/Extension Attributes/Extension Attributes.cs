using NationalInstruments.MeasurementLink.Discovery.V1;
using NationalInstruments.Measurements.Data.V1;
using NationalInstruments.Measurements.Metadata.V1;
using static NationalInstruments.Measurements.Data.V1.DataStoreService;
using static NationalInstruments.Measurements.Metadata.V1.MetadataStoreService;

var clientStubFactory = new GrpcClientStubFactory();
var dataStoreServiceClient = clientStubFactory.CreateClient<DataStoreServiceClient>();
var metadataStoreServiceClient = clientStubFactory.CreateClient<MetadataStoreServiceClient>();

// Register multiple hardware schemas.
var cableSchemaId = await metadataStoreServiceClient.RegisterSchemaFromFileAsync("cable_schema.toml");
var socketSchemaId = await metadataStoreServiceClient.RegisterSchemaFromFileAsync("cable_schema.toml");
var scopeSchemaId = await metadataStoreServiceClient.RegisterSchemaFromFileAsync("cable_schema.toml");
var testResultSchemaId = await metadataStoreServiceClient.RegisterSchemaFromFileAsync("cable_schema.toml");

// To be completed later once we've reviewed one example.
