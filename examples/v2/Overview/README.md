# V2 Overview

This example creates and verifies the Stage 1 V2 scalar resource graph:

```text
TestResult
  -> Step
     -> Condition
     -> scalar Measurement
```

## Prerequisites

- .NET 10 SDK.
- Access to the internal NuGet feed configured by the repository. Authenticate
  with your approved internal-feed mechanism; do not put credentials in this
  project or its configuration. The V2 metadata dependency is pinned to the
  same package version as `NI.DataStore.V2` and restored from that feed.
- The `NI.DataStore.V2` package and a separately started MDS service that
  supports its V2 API. Package-graph compatibility is resolved by the pinned
  V2 packages, but the compatible MDS runtime release is separately required
  and not yet confirmed.

## Build

```powershell
dotnet build "examples\v2\Overview\Overview.csproj"
```

## Run

Start a compatible V2 MDS service separately, then run:

```powershell
dotnet run --project "examples\v2\Overview\Overview.csproj" -- --endpoint http://localhost:58371
```

`--endpoint` is optional and defaults to `http://localhost:58371`. The
example does not start, configure, or authenticate an MDS service.

On a compatible service, output includes the returned resource IDs and:

```text
Created TestResult ID: <id>
Created Step ID: <id>
Created Measurement ID: <id>
Verified scalar measurement value: 42
Verified condition propagation: operating-mode=nominal
```

## Stage 1 limitations

This example covers scalar values only. It references no inline metadata and
does not demonstrate metadata creation; use pre-existing metadata references
when a service policy requires them. It does not cover batch operations,
updates, deletes, non-scalar values, extension-schema behavior, or
workspace/RBAC operations.
