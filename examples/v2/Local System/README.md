# V2 Local System

This focused hybrid example discovers the current computer, user, NI hardware,
and installed software through the supported System Configuration API. It
creates the corresponding metadata through the existing **V1 metadata API**,
then creates an isolated **V2** TestResult that references those metadata IDs.
It reads the TestResult back and verifies every submitted metadata relationship
ID.

## Prerequisites

- .NET 10 SDK.
- Access to the internal NuGet feed configured by the repository. Authenticate
  using your approved internal-feed mechanism; do not put credentials in this
  project or its configuration.
- A compatible external V2 MDS service, started separately, that exposes both
  the V2 datastore API and the V1 metadata API at the selected endpoint.
- NI System Configuration installed on the machine that runs the example.

## Build

```powershell
dotnet build "examples\v2\Local System\Local System.csproj"
```

## Run

Start the compatible external V2 MDS service, then run:

```powershell
dotnet run --project "examples\v2\Local System\Local System.csproj" -- --endpoint http://localhost:58371
```

`--endpoint` is optional and defaults to `http://localhost:58371`. A
per-run suffix isolates the V2 TestResult name. The example does not start,
configure, or authenticate the service.

## Metadata bridge limitation

The discovery-to-relationship flow is: System Configuration discovery → V1
metadata creation → confirmed V1 metadata IDs on a V2 TestResult → V2
read-back verification. This is an explicit **V1 metadata bridge**. It does
not demonstrate V2-native metadata find-or-create, V2-native metadata
management, or inline metadata support.
