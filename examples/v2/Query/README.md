# V2 Query

This example creates two Stage 1 scalar graphs and verifies the V2 gRPC query
contract for filtering, read masks, and pagination.

## Prerequisites

- .NET 10 SDK.
- Access to the internal NuGet feed configured by the repository. Authenticate
  with your approved internal-feed mechanism; do not put credentials in this
  project or its configuration.
- The `NI.DataStore.V2` package and a separately started MDS service that
  supports its V2 API. The example does not start, configure, or authenticate
  the service.

## Build

```powershell
dotnet build "examples\v2\Query\Query.csproj"
```

## Run

Start a compatible V2 MDS service separately, then run:

```powershell
dotnet run --project "examples\v2\Query\Query.csproj" -- --endpoint http://localhost:58371
```

`--endpoint` is optional and defaults to `http://localhost:58371`.

## Seeded resources

Each execution generates one 32-character hexadecimal suffix and appends it to
every seeded resource name. This keeps exact-name filters isolated from prior
runs against persistent MDS storage while retaining deterministic scalar
values. The example creates:

| TestResult | Step | Scalar Measurement | Value |
| --- | --- | --- | ---: |
| `V2 Query Test Result One <suffix>` | `V2 Query Step One <suffix>` | `V2 Query Measurement One <suffix>` | 101 |
| `V2 Query Test Result Two <suffix>` | `V2 Query Step Two <suffix>` | `V2 Query Measurement Two <suffix>` | 202 |

`<suffix>` is the same safe, per-run value for all six names. Repeated runs
therefore create independent graphs and continue to assert exactly one result
for every exact-name filter.

It queries each resource type using an OData `$filter` expression, requests
only `id` through a gRPC read mask, and uses `$top=1` to request a small page.
The response's V2 `NextLink` is supplied verbatim to retrieve the next page.

The gRPC request supports these filter, read-mask, and pagination behaviors.
It deliberately does not send `$select`, `$count`, `$expand`, or `$apply`.
Those HTTP/OData response-shaping features are separate from this Stage 1
gRPC example and remain future work.

The executable throws `InvalidOperationException` if a filter returns the
wrong resource, a read mask includes `Name`, pagination omits or retains a
continuation incorrectly, or pages duplicate a resource ID. On success,
output includes:

```text
Run suffix: <32-character-hexadecimal-value>
Verified filter: $filter=Name eq 'V2 Query Test Result One <suffix>'
Records read: 1
Verified read-mask fields: id
Verified filter: $filter=Name eq 'V2 Query Test Result One <suffix>' or Name eq 'V2 Query Test Result Two <suffix>'&$top=1
Records read: 2
```
