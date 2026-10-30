# V2 DataStoreContext

`DataStoreContext` creates V2 data-store clients for an explicitly configured
external MDS gRPC endpoint. The MDS service must be started separately; this
helper does not start or configure it.

For local development, use `http://localhost:58371`.

The service owner must configure MDS to expose the V2 API and configure its V2
storage before starting the service. These service-side API-mode and storage
settings are outside the responsibility of this helper and V2 examples.

The context accepts only absolute HTTP or HTTPS endpoints and throws
`ArgumentException` for invalid endpoints. `CreateDataStoreClient` returns a
`DataStoreService.DataStoreServiceClient` that uses the context's gRPC channel.
Dispose the context to dispose that channel.

All V2 executable examples must use this helper to create their data-store
clients.
