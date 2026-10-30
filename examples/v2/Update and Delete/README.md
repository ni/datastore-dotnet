# V2 Update and Delete

This standalone Stage 2 lifecycle example creates the minimum V2 graph:

```text
TestResult (per-run suffix)
  -> Step (per-run suffix)
     -> Measurement for singular update/delete
     -> Measurement for batch delete
```

## Prerequisites

- .NET 10 SDK.
- Access to the internal NuGet feed configured by the repository. Authenticate
  with your approved internal-feed mechanism; do not put credentials in this
  project or its configuration.
- An externally started, compatible V2 MDS service that supports measurement
  update and delete APIs. This example connects to the service; it does not
  start, configure, or authenticate it.

## Build

```powershell
dotnet build "examples\v2\Update and Delete\Update and Delete.csproj"
```

## Run

Start a compatible V2 MDS service separately, then run:

```powershell
dotnet run --project "examples\v2\Update and Delete\Update and Delete.csproj" -- --endpoint http://localhost:58371
```

`--endpoint` is optional and defaults to `http://localhost:58371`.

## Workflow and assertions

Each execution appends one 32-character hexadecimal suffix to all resource
names, isolating the run from persistent service data.

1. Creates one TestResult, one Step, and two scalar Measurements.
2. Uses `UpdateMeasurement` with a `FieldMask` containing `name` to change one
   Measurement's mutable `Name`, then reads it back with `GetMeasurement` and
   asserts the new name.
3. Attempts to update the documented immutable `Value` field using a
   `FieldMask` containing `value`. It requires an `RpcException` with a
   non-empty service status detail; accepting the mutation or returning an
   empty failure detail stops the example.
4. Deletes the first Measurement with `DeleteMeasurement`, then requires
   `GetMeasurement` to return `NotFound`.
5. Deletes the second Measurement with `BatchDeleteMeasurements`, then
   requires the same failed read.

The project intentionally does not delete the parent Step or TestResult.
It does not demonstrate relationship mutation, parent/child deletion rules,
or force-delete behavior because those semantics are outside this focused
example's confirmed scope.
