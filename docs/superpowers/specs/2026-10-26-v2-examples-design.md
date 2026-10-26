# V2 .NET Examples Design

## Purpose

Add a V2 example suite to this repository that teaches consumers how to use
`NI.DataStore.V2` without changing, breaking, or conflating the existing V1
examples.

The V2 examples follow the V2 delivery stages and service contract. They are
not V1 samples with namespaces renamed. Each example documents the V2 resource
model and only uses capabilities available in the consumed V2 package and
service.

## Scope

This design covers the consumer examples for the first three V2 delivery
stages:

- Stage 1: scalar tracer-bullet capabilities.
- Stage 2: batch and inline creation, measurement fields and limits,
  update/delete, and non-scalar values when their delivery dependencies are
  complete.
- Stage 3: extension schemas and workspace/RBAC behavior.

The following fourth-stage capabilities are explicitly out of scope:

- DataFrame API.
- TestMonitor compatibility.
- HTTP mutation examples.

## Source Material

This design is based on:

- `C:\Users\sbalusam\source\MDS\datastore-service\docs\design\mds\api-v2\supplemental\delivery-staging.md`
- `C:\Users\sbalusam\source\MDS\datastore-service\src\datastore\proto\data_store.proto`
- `C:\Users\sbalusam\source\MDS\datastore-service\src\datastore\proto\data_store_service.proto`

The delivery-staging document defines what is ready to demonstrate at each
stage. The proto files define the resource model and service operations:
`TestResult`, `Step`, `Condition`, `Measurement`, `MeasurementValue`, create,
query, batch, update, relationship, delete, and value-read operations.

## Repository Layout

Keep the existing V1 suite unchanged. Add an isolated V2 suite under
`examples\v2`.

```text
examples\
  Examples.slnx                         # Existing V1 solution; unchanged
  Directory.Build.props                 # Existing V1 build settings; unchanged
  Directory.Packages.props              # Existing NI.DataStore.V1 pin; unchanged
  Alias\
  Batched Publishing\
  DataStoreContext\
  Extension Attributes\
  Local System\
  Overview\
  Query\

  v2\
    V2.Examples.slnx                    # V2-only solution
    Directory.Build.props               # V2 build settings
    Directory.Packages.props            # NI.DataStore.V2 version pin
    README.md                            # V2 suite guide and delivery-stage matrix
    DataStoreContext\
      DataStoreContext.csproj
      DataStoreContext.cs
      README.md
    Overview\
      Overview.csproj
      Overview.cs
      README.md
    Query\
      Query.csproj
      Query.cs
      README.md
    Batched Publishing\
      Batched Publishing.csproj
      Batched Publishing.cs
      README.md
    Measurement Values\
      Measurement Values.csproj
      Measurement Values.cs
      README.md
    Update and Delete\
      Update and Delete.csproj
      Update and Delete.cs
      README.md
    Extension Attributes\
      Extension Attributes.csproj
      Extension Attributes.cs
      README.md
    Local System\
      Local System.csproj
      Local System.cs
      README.md
```

The names intentionally follow the current V1 naming style: concise,
consumer-facing capability names such as `Overview`, `Query`, and `Batched
Publishing`.

The V2 helper writes only beneath `examples\v2\temp_data`. It must not share
the V1 `examples\temp_data` location because the V2 API mode and datastore are
intentionally isolated from V1.

## Package and Feed Configuration

`NI.DataStore.V2` is not yet public. The repository must therefore restore it
from the internal Artifactory/NuGet feed without weakening the current
nuget.org-only protections for other packages.

1. Keep the existing `nuget.org` source and mappings.
2. Add a named internal feed source to `nuget.config`.
3. Map only `NI.DataStore.V2` and confirmed V2-only companion packages to that
   source.
4. Put the V2 package version in
   `examples\v2\Directory.Packages.props`.
5. Reference `NI.DataStore.V2` without a per-project version.
6. Never commit Artifactory tokens, passwords, API keys, or credentials.

Developer authentication uses the approved user-level NuGet configuration or
credential provider. CI authentication uses a protected secret only in the V2
workflow.

The exact V2 package version, feed URL, target framework, generated C#
namespace, and client construction API must be validated from the actual
prerelease package before compiling examples.

## Example Design

### Overview

**Purpose:** Teach the first V2 scalar lifecycle end-to-end.

**Capabilities:**

1. Initialize the V2 local DataStore context.
2. Construct the V2 data-store client using the supported public client API.
3. Reference required existing metadata by ID or alias.
4. Create a `TestResult`.
5. Create one `Step` for an execution.
6. Add a `Condition` owned by the step.
7. Create one scalar `Measurement` under the step.
8. Read back the created resources and measurement value.
9. Verify that the measurement exposes the step-owned condition.

**Consumer outcome:** Understand the V2 resource graph:

```text
TestResult
  -> Step
     -> Condition
     -> scalar Measurement
```

The example explains the V2 rules that differ from V1: one step per
execution, one measurement per value, step-owned conditions, and no
`ParametricIndex`.

**Stage gate:** Stage 1 only. It must not use batch APIs, update/delete,
inline metadata creation, non-scalar values, extension-schema behavior, or any
other later-stage feature.

### Query

**Purpose:** Teach consumers how to retrieve scalar V2 data efficiently.

**Capabilities:**

1. Seed a small deterministic scalar hierarchy.
2. Query test results, steps, and measurements with supported OData filters.
3. Request only required fields with gRPC `read_mask`.
4. Follow `next_link` pagination where returned.
5. Demonstrate relationship and condition-based queries.

**Consumer outcome:** Understand that gRPC queries support filtering and
read masks, while response shaping such as `$select`, `$count`, `$expand`, and
`$apply` belongs to the HTTP/OData API rather than gRPC.

**Stage gate:** The gRPC content belongs to Stage 1. HTTP/OData examples are
added only after their approved supplemental contract is delivered.

### Batched Publishing

**Purpose:** Teach efficient and atomic V2 creation of related data.

**Capabilities:**

1. Batch-create TestResults, Steps, and Measurements.
2. Use nested resource creation where it is supported.
3. Use inline metadata find-or-create where it is supported.
4. Demonstrate propagation of Step conditions to nested Measurements.
5. Submit an intentionally invalid request and verify that the entire batch is
   rolled back.

**Consumer outcome:** Learn the V2 answer to high-volume publishing:
atomic graph creation, ordered results, and no partial writes on validation
failure.

**Stage gate:** Stage 2 batch and inline-create vertical. This project is not
implemented until that capability is available in both the V2 package and
service.

### Measurement Values

**Purpose:** Teach how to define and retrieve V2 Measurement values correctly.

**Capabilities:**

1. Create scalar measurements with units, comparison type, upper limit, and
   lower limit.
2. Show timestamp and duration presence/derivation behavior.
3. Read the stored measurement fields back.
4. Attempt to change immutable fields and verify the documented validation
   failure.
5. Add supported non-scalar values, including their read and cleanup behavior,
   after the non-scalar value-store contract is delivered.

**Consumer outcome:** Understand that limits are independent values, fields
such as value and limits are immutable after creation, and non-scalar storage
requires its own delivered lifecycle contract. This is intentionally one
example because scalar and non-scalar values are both Measurement concerns.

**Stage gate:** Scalar fields and limits require the Stage 2 new-resource-fields
vertical. Non-scalar content remains deferred until the DEC-008 storage
decision and its service/package support are complete. No placeholder code is
created before then. A standalone `Non-Scalar Values` project is considered
only if value-store setup, data-moniker behavior, limits, streaming, and
cleanup make this example too large to understand as part of Measurement Values.

### Update and Delete

**Purpose:** Teach safe lifecycle management for V2 data.

**Capabilities:**

1. Update mutable fields with `FieldMask`.
2. Add and remove Step conditions.
3. Add and remove supported metadata relationships.
4. Show the difference between lifecycle `Status` and terminal `Outcome`.
5. Perform singular and atomic batch updates/deletes.
6. Demonstrate force and non-force deletion behavior.
7. Explain non-retry-safe delete/remove behavior and expected precondition
   failures.

**Consumer outcome:** Know how to mutate only intended fields, preserve
relationship invariants, and avoid partial lifecycle changes.

**Stage gate:** Stage 2 update/delete vertical.

### Extension Attributes

**Purpose:** Teach V2 custom data validation and workspace boundaries.

**Capabilities:**

1. Register and use a schema-governed extension.
2. Write and read valid extension values on the supported V2 resources.
3. Verify that undeclared closed-schema keys are rejected without partial
   writes.
4. Demonstrate configured local and hosted workspace authorization behavior.
5. Demonstrate cross-workspace metadata-reference behavior where available.

**Consumer outcome:** Understand how schema-governed extension data is
validated and how workspace boundaries affect consumer access.

**Stage gate:** Stage 3 extension-schema, datastore RBAC, and metadata RBAC
verticals. Do not implement this project until the selected `NI.DataStore.V2`
package and service expose the required contract.

### Local System

**Purpose:** Capture the local machine's operator, test station, NI hardware,
and installed software as metadata associated with a V2 TestResult.

**Capabilities:**

1. Discover the current user, local computer, NI hardware, and installed
   software through `NationalInstruments.SystemConfiguration.x64`.
2. Create or resolve the corresponding metadata with the supported metadata
   API.
3. Create a V2 TestResult for the local-system snapshot.
4. Associate the discovered metadata with the V2 TestResult.
5. Read the TestResult back and verify the captured metadata associations.

**Consumer outcome:** Understand how a practical local-system discovery
workflow produces V2 test data with traceable machine, hardware, software,
and operator context.

**Stage gate:** Stage 2 metadata-relationship and inline metadata
find-or-create behavior. The project is deferred when the selected package and
service support only Stage 1 references to pre-existing metadata by ID or
alias.

## Documentation Standards

Every V2 example README must include:

1. The feature it demonstrates and its delivery-stage dependency.
2. Required MDS version, operating-system requirements, and package version.
3. Internal-feed authentication prerequisites without exposing credentials.
4. Exact build and run commands.
5. Expected output and verification behavior.
6. Important V2 contract rules and known capability exclusions.
7. The local data directory used by the example.

The V2 suite README must include a stage matrix that marks each example as:

- available;
- pending its package/service capability;
- deferred by a named design decision; or
- intentionally out of scope.

The root README must link independently to the V1 and V2 suites and label V2
as internal/prerelease until it is published.

## CI and Validation

### V1

The existing V1 workflow and `examples\Examples.slnx` remain unchanged. V1
continues to restore from nuget.org, build, and format without access to the
internal feed.

### V2

Add a separate credentialed V2 workflow or protected V2 job that:

1. Authenticates to Artifactory with a CI secret.
2. Restores `examples\v2\V2.Examples.slnx`.
3. Builds the V2 solution.
4. Verifies formatting for the V2 solution.
5. Does not log credentials.

Do not run MDS-dependent executable examples in hosted CI until a supported
Windows runner or test environment provisions the required MDS version.

Runtime validation occurs on a controlled Windows machine with MDS installed:

```powershell
dotnet build examples\v2\V2.Examples.slnx
dotnet run --project "examples\v2\Overview\Overview.csproj"
```

Each example must verify its success condition, print a meaningful result, and
fail with a non-zero exit code on a service, contract, schema, or read-back
failure.

## Implementation Sequence

1. Inspect the prerelease `NI.DataStore.V2` package and confirm its public C#
   types, client construction pattern, supported target frameworks, helpers,
   and staged capability availability.
2. Confirm the internal feed URL, approved package version, developer
   authentication method, and CI secret mechanism.
3. Add the isolated V2 solution, central package configuration, V2 data-store
   context, suite README, narrow feed mapping, and protected V2 build/format
   workflow.
4. Implement and manually validate `Overview` against the real V2 service.
5. Implement `Query` after the scalar query contract is available.
6. Implement `Batched Publishing`, `Measurement Values`, and `Update and
   Delete` independently as their Stage 2 package/service verticals become
   available.
7. Add non-scalar content to `Measurement Values` only after DEC-008 and the
   non-scalar lifecycle contract are complete.
8. Implement `Extension Attributes` after its Stage 3 extension-schema and
   workspace/RBAC verticals are delivered.
9. Implement `Local System` after its Stage 2 metadata-relationship contract
   is delivered.
10. Do not add fourth-stage examples under this design.

## Acceptance Criteria

- Existing V1 examples, package pins, solution, and public CI behavior remain
  unchanged.
- V2 examples restore only from the narrowly mapped V2 internal feed.
- No credentials are tracked in the repository.
- Each implemented example compiles against the actual selected
  `NI.DataStore.V2` package.
- Each implemented example runs independently against a supported local MDS
  installation and verifies its stated consumer outcome.
- An example is added only when its required delivery-stage capability is
  available in both the package and service.
- V2 documentation clearly distinguishes available, deferred, and out-of-scope
  capabilities.
