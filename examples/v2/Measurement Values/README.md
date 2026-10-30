# V2 Measurement Values

This example creates and verifies isolated V2 measurement values:

```text
TestResult (per-run suffix)
  -> Step (per-run suffix)
     -> scalar Measurement (double)
     -> non-scalar Measurement (Vector.DoubleArray)
```

## Prerequisites

- .NET 10 SDK.
- Access to the internal NuGet feed configured by the repository. Authenticate
  with your approved internal-feed mechanism; do not put credentials in this
  project or its configuration.
- A compatible V2 MDS service started separately. This example connects to the
  service but does not start, configure, or authenticate it.

## Build

```powershell
dotnet build "examples\v2\Measurement Values\Measurement Values.csproj"
```

## Run

Start a compatible V2 MDS service separately, then run:

```powershell
dotnet run --project "examples\v2\Measurement Values\Measurement Values.csproj" -- --endpoint http://localhost:58371
```

`--endpoint` is optional and defaults to `http://localhost:58371`.

The example creates a uniquely named TestResult and Step for each run, then
round-trips a scalar `double` and a non-scalar `Vector.DoubleArray` containing
three `double` values. It verifies both the returned value-union arms and their
payloads, and throws detailed `InvalidOperationException` errors if either
value does not round-trip.

## Limitations

This example does not demonstrate data-moniker values, windowed reads, or
streaming/storage behavior.
