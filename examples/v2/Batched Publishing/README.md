# V2 Batched Publishing

This example demonstrates **V2 batch resource creation**. It creates a
uniquely named TestResult and Step, then uses `BatchCreateMeasurements` to
create three Measurements in one request. It verifies the returned count,
IDs, and input ordering, then reads every Measurement back by ID.

The example also submits a batch containing one valid Measurement and one
invalid Measurement with an empty `StepId`. It requires an informative V2
validation failure and queries for the otherwise-valid Measurement to prove
the failed batch did not create it partially.

## What this does not demonstrate

This is not the V1 parametric-publishing workflow. It does **not** demonstrate
`ParametricIndex`, iteration grouping, or condition batch publishing. V2 batch
creation atomically creates V2 resource records; the prerequisite TestResult
and Step are created first.

## Prerequisites

- .NET 10 SDK.
- Access to the internal NuGet feed configured by the repository. Authenticate
  using your approved internal-feed mechanism; do not put credentials in this
  project or its configuration.
- A compatible V2 MDS service started separately. This example connects to the
  service but does not start, configure, or authenticate it.

## Build

```powershell
dotnet build "examples\v2\Batched Publishing\Batched Publishing.csproj"
```

## Run

Start a compatible V2 MDS service separately, then run:

```powershell
dotnet run --project "examples\v2\Batched Publishing\Batched Publishing.csproj" -- --endpoint http://localhost:58371
```

`--endpoint` is optional and defaults to `http://localhost:58371`.

The program throws `InvalidOperationException` if the batch response does not
preserve the expected count, unique IDs, resource order, names, or parent
Step; if read-back differs; if an invalid batch succeeds or partially creates
the valid request; or if the V2 validation error omits useful status detail.
