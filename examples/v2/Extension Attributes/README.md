# V2 Extension Attributes

This focused hybrid example registers a TestResult extension schema through the
existing **V1 metadata** schema-registration API, then creates and reads a
**V2** TestResult that references the returned `SchemaId`. It verifies that a
declared string extension value round-trips through the V2 datastore resource.

## Prerequisites

- .NET 10 SDK.
- Access to the internal NuGet feed configured by the repository. Authenticate
  using your approved internal-feed mechanism; do not put credentials in this
  project or its configuration.
- A compatible MDS service, started separately, that exposes the V1 metadata
  API and the V2 datastore API at the selected endpoint.

## Build

```powershell
dotnet build "examples\v2\Extension Attributes\Extension Attributes.csproj"
```

## Run

Start a compatible service, then run:

```powershell
dotnet run --project "examples\v2\Extension Attributes\Extension Attributes.csproj" -- --endpoint http://localhost:58371
```

`--endpoint` is optional and defaults to `http://localhost:58371`. The
example does not start, configure, or authenticate the service.

## Scope and limitation

This bridges **V1 metadata schema management** with **V2 datastore resources**;
it does **not** demonstrate V2-native schema registration, workspace behavior,
RBAC, or cross-workspace authorization.

The example verifies a declared extension value only. It intentionally does
not submit an undeclared closed-schema key because the selected client package
does not expose a concrete, service-independent closed-schema validation
contract that can be asserted safely. Do not treat the absence of that request
as evidence that an endpoint accepts undeclared keys.
