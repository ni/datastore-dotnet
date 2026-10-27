using Grpc.Net.Client;
using NationalInstruments.Measurements.Data.V2;
using NationalInstruments.Measurements.Metadata.V1;

namespace NationalInstruments.DataStore.V2.Utilities;

public sealed class DataStoreContext : IDisposable
{
    private readonly GrpcChannel _channel;

    public DataStoreContext(Uri endpoint)
    {
        ArgumentNullException.ThrowIfNull(endpoint);

        if (!endpoint.IsAbsoluteUri ||
            (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException(
                "The endpoint must be an absolute HTTP or HTTPS URI.",
                nameof(endpoint));
        }

        Endpoint = endpoint;
        _channel = GrpcChannel.ForAddress(endpoint);
    }

    public Uri Endpoint { get; }

    public DataStoreService.DataStoreServiceClient CreateDataStoreClient() =>
        new(_channel);

    public MetadataStoreService.MetadataStoreServiceClient CreateMetadataStoreClient() =>
        new(_channel);

    public void Dispose() => _channel.Dispose();
}
