# V2 .NET Examples

The V2 suite is prerelease/internal. It uses `NI.DataStore.V2`
`9.23.0-ci.2610222014` and targets .NET 10. V1 examples remain unchanged.

> **Not runnable yet:** These projects compile against the prerelease package,
> but no supported MDS runtime release or required V2 service/storage
> configuration has been confirmed. Do not run these examples until those
> requirements are published.

## Delivery matrix

| Example | Status in this phase | Required delivery capability |
|---|---|---|
| Overview | Not runnable yet | Stage 1 scalar lifecycle |
| Query | Not runnable yet | Stage 1 scalar query |
| Batched Publishing | Not runnable yet | Stage 2 batch resource creation |
| Measurement Values | Not runnable yet | Stage 2 fields and limits |
| Update and Delete | Not runnable yet | Stage 2 measurement mutation APIs |
| Extension Attributes | Not runnable yet (hybrid) | V1 metadata schema registration with V2 extension values |
| Local System | Not runnable yet (hybrid) | System Configuration discovery and V1 metadata IDs on a V2 TestResult |

## Feed authentication

V2 packages are restored from the internal Artifactory feed configured in
`examples\v2\nuget.config`. Before restoring, authenticate the local NuGet
source named `niartifacts-nuget-packages` using your approved Artifactory
credentials. Do not add credentials or tokens to this repository.

## Build

```powershell
dotnet restore "examples\v2\V2.Examples.slnx" --configfile "examples\v2\nuget.config"
dotnet build "examples\v2\V2.Examples.slnx" --no-restore
```

## Run when a supported MDS runtime is available

```powershell
dotnet run --project "examples\v2\Batched Publishing\Batched Publishing.csproj"
dotnet run --project "examples\v2\Extension Attributes\Extension Attributes.csproj"
dotnet run --project "examples\v2\Local System\Local System.csproj"
dotnet run --project "examples\v2\Measurement Values\Measurement Values.csproj"
dotnet run --project "examples\v2\Overview\Overview.csproj"
dotnet run --project "examples\v2\Query\Query.csproj"
dotnet run --project "examples\v2\Update and Delete\Update and Delete.csproj"
```

Each executable requires an externally started compatible V2 MDS service with
the published V2 storage and API configuration. The build workflow verifies
restore, build, and formatting only; it does not run service-backed examples.

**Hybrid examples** use the existing V1 metadata API where V2-native metadata
operations are not available. They do not claim V2-native metadata
find-or-create or inline metadata support.
