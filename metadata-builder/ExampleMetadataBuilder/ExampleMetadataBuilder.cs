using NationalInstruments.MeasurementLink.Discovery.V1;
using NationalInstruments.Measurements.Metadata.V1;
using NationalInstruments.Measurements.MetadataBuilder;
using NationalInstruments.TestStand.Interop.API;

namespace NationalInstruments.ExampleMetadataBuilders
{
    public sealed class ExampleMetadataBuilder : IMetadataBuilder
    {
        private readonly GrpcClientStubFactory _grpcClientStubFactory = new();
        private readonly MetadataStoreService.MetadataStoreServiceClient _metadataStoreClient;

        public ExampleMetadataBuilder()
        {
            _metadataStoreClient = _grpcClientStubFactory.CreateClient<MetadataStoreService.MetadataStoreServiceClient>();
        }

        public async Task BuildMetadataForTestResultAsync(TestResultBuilder builder, PropertyObject sequenceResult, string uutSerialNumber)
        {
            var softwareItem = new SoftwareItem()
            {
                Product = "Example Metadata Builder Product",
                Version = "1.0",
            };
            var softwareItemId = await _metadataStoreClient.CreateSoftwareItemAsync(softwareItem);

            var hardwareItem = new HardwareItem()
            {
                SerialNumber = "ABC",
                PartNumber = "DEF",
                Model = "XYZ",
                Manufacturer = "Third Party",
            };
            var hardwareItemId = await _metadataStoreClient.CreateHardwareItemAsync(hardwareItem);

            builder
                .WithSoftwareItemIds([softwareItemId])
                .WithHardwareItemIds([hardwareItemId]);
        }

        public Task BuildMetadataForStepAsync(StepBuilder builder, PropertyObject stepResult)
        {
            builder
                .WithNotes("StepNotes");
            return Task.CompletedTask;
        }

        public Task BuildMetadataForPublishMeasurementAsync(PublishMeasurementRequestBuilder builder, PropertyObject measurement)
        {
            builder
                .WithNotes("PublishMeasurementNotes");
            return Task.CompletedTask;
        }

        public Task BuildMetadataForPublishConditionAsync(PublishConditionRequestBuilder builder, PropertyObject condition)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _grpcClientStubFactory.Dispose();
        }
    }
}