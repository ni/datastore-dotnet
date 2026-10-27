# V2 .NET Examples

The V2 suite is prerelease/internal. It uses `NI.DataStore.V2`
`9.23.0-ci.2610222014` and targets .NET 10. V1 examples remain unchanged.

## Delivery matrix

| Example | Status in this phase | Required delivery capability |
|---|---|---|
| Overview | Available | Stage 1 scalar lifecycle |
| Query | Available | Stage 1 scalar query |
| Batched Publishing | Available | Stage 2 batch resource creation |
| Measurement Values | Available | Stage 2 fields and limits |
| Update and Delete | Available | Stage 2 measurement mutation APIs |
| Extension Attributes | Available (hybrid) | V1 metadata schema registration with V2 extension values |
| Local System | Available (hybrid) | System Configuration discovery and V1 metadata IDs on a V2 TestResult |

## Feed authentication

V2 packages are restored from the internal Artifactory feed configured in the
repository `nuget.config`. Before restoring, authenticate the local NuGet
source named `niartifacts-nuget-packages` using your approved Artifactory
credentials. Do not add credentials or tokens to this repository.

## Build and run

```powershell
dotnet build "examples\v2\V2.Examples.slnx"
dotnet run --project "examples\v2\Batched Publishing\Batched Publishing.csproj"
dotnet run --project "examples\v2\Extension Attributes\Extension Attributes.csproj"
dotnet run --project "examples\v2\Local System\Local System.csproj"
dotnet run --project "examples\v2\Overview\Overview.csproj"
dotnet run --project "examples\v2\Query\Query.csproj"
dotnet run --project "examples\v2\Update and Delete\Update and Delete.csproj"
```

Each executable requires an externally started compatible V2 MDS service. The
build workflow verifies restore, build, and formatting only; it does not run
service-backed examples.

**Hybrid examples** use the existing V1 metadata API where V2-native metadata
operations are not available. They do not claim V2-native metadata
find-or-create or inline metadata support.
