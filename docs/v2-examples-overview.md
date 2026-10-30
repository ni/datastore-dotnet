# V2 Client Examples Overview

## Purpose

This document defines the V2 client example set for every supported client
language. It is intentionally language-neutral so that the .NET and Python
repositories teach the same consumer workflows, even though their project
layouts and client-construction code differ.

The examples should help a consumer answer:

1. How do I connect a V2 client to Measurement Data Services (MDS)?
2. How do I create, query, and maintain V2 test data?
3. How do I use advanced V2 capabilities when they become available?
4. How do I associate local-system and custom metadata with V2 test data?

V2 examples must use the actual published V2 client package and supported MDS
service behavior. They must not treat a proto declaration as proof that an API
is available in a particular package or service release.

## Current Runtime Availability

The .NET V2 projects compile against the prerelease package, but no supported
MDS runtime release or required V2 API and storage configuration has been
confirmed. The examples are therefore not runnable yet. Keep their status as
not runnable until those runtime requirements are published and verified.

## Design Principles

- Keep V1 examples working while they are supported.
- Give V2 its own package configuration, example suite, local data directory,
  and build path.
- Use the same short, capability-based names in every language.
- Keep each example independently runnable and focused on one consumer
  workflow.
- Demonstrate only delivered V2 behavior; defer incomplete verticals instead
  of providing non-working placeholders.
- Verify each example's key result with a read, query, or explicit error
  assertion.
- Never store package-feed credentials in source control.

## V2 Resource Model

The foundational V2 data flow is:

```text
TestResult
  -> Step
     -> Condition
     -> Measurement
```

The first scalar examples should teach these V2 rules:

- A Step represents one execution.
- A Measurement represents one value.
- Conditions are owned by the Step and are exposed by related Measurements.
- V2 does not use V1 `ParametricIndex` behavior.

## Final Example Set

### 1. Overview

**Consumer question:** How do I create and read a simple V2 result?

**Demonstrates:**

1. Initialize the local example environment.
2. Construct the V2 client using the language's supported public API.
3. Reference required metadata by ID or alias.
4. Create a TestResult.
5. Create a Step.
6. Add a Step-owned Condition.
7. Create a scalar Measurement.
8. Read back the created resources and measurement value.
9. Verify the Measurement exposes the expected Condition.

**Delivery gate:** Stage 1 scalar tracer-bullet capability.

### 2. Query

**Consumer question:** How do I efficiently find V2 data?

**Demonstrates:**

1. Create a small deterministic scalar data set.
2. Query TestResults, Steps, and Measurements using supported OData filters.
3. Request only required fields with the gRPC read mask.
4. Follow pagination links or tokens.
5. Query through supported resource and condition relationships.

**Delivery gate:** Stage 1 for gRPC query behavior. Add HTTP/OData-specific
response shaping only after the approved HTTP contract is delivered.

### 3. Batched Publishing

**Consumer question:** How do I create related V2 data efficiently without
partial writes?

**Demonstrates:**

1. Batch creation of TestResults, Steps, and Measurements.
2. Nested creation when supported.
3. Inline metadata find-or-create when supported.
4. Propagation of Step Conditions to nested Measurements.
5. Atomic rollback after a deliberately invalid request.

**Delivery gate:** Stage 2 batch and inline-create capability.

### 4. Measurement Values

**Consumer question:** How do I define scalar and non-scalar measurement
values correctly?

**Demonstrates initially:**

1. Scalar Measurement creation.
2. Units and comparison type.
3. Upper and lower limits.
4. Timestamp and duration behavior.
5. Immutable-field validation after creation.

**Adds after non-scalar delivery:**

1. Supported waveform, XY, digital, or other non-scalar value types.
2. Reading values through `ReadMeasurementValues`.
3. Supported value limits.
4. Value-store cleanup behavior when a Measurement is deleted.

**Delivery gate:** Scalar fields and limits require the Stage 2
new-resource-fields capability. Non-scalar content requires the finalized
non-scalar storage and lifecycle contract, including DEC-008.

Do not create a separate Non-Scalar Values example unless non-scalar setup,
data-moniker behavior, limits, streaming, and cleanup make Measurement Values
too large to understand.

### 5. Update and Delete

**Consumer question:** How do I safely modify and remove V2 data?

**Demonstrates:**

1. Field-mask updates to mutable fields.
2. Adding and removing Step Conditions.
3. Adding and removing supported metadata relationships.
4. The distinction between lifecycle Status and terminal Outcome.
5. Singular and atomic batch update/delete behavior.
6. Force and non-force deletion behavior.
7. Expected validation and precondition failures.

**Delivery gate:** Stage 2 update/delete capability.

### 6. Extension Attributes

**Consumer question:** How do I attach validated custom data to V2 resources?

**Demonstrates:**

1. Schema-governed extension registration and usage.
2. Writing and reading valid custom values.
3. Rejection of undeclared closed-schema keys without partial writes.
4. Local and hosted workspace authorization behavior.
5. Cross-workspace metadata-reference behavior where available.

**Delivery gate:** Stage 3 extension-schema, datastore RBAC, and metadata RBAC
capabilities.

### 7. Local System

**Consumer question:** How do I attach local machine context to V2 test data?

**Demonstrates:**

1. Discover the current user, local computer, NI hardware, and installed
   software with the language's System Configuration integration.
2. Create or resolve the corresponding metadata with the supported metadata
   API.
3. Create a V2 TestResult for the local-system snapshot.
4. Associate the discovered metadata with the TestResult.
5. Read the TestResult back and verify the metadata associations.

**Delivery gate:** Stage 2 metadata relationships and inline metadata
find-or-create capability. Defer this example when only Stage 1 references to
pre-existing metadata by ID or alias are available.

## Shared Example Context

Every language needs a shared context helper equivalent to the .NET
`DataStoreContext`.

The helper must:

1. Save MDS-related environment or configuration values before changing them.
2. Configure a deterministic, example-specific local MDS environment.
3. Use a V2-specific data directory, separate from V1 example data.
4. Configure a distinct discovery/cluster identity.
5. Restore the original environment or configuration when the example exits.

The precise MDS settings must be confirmed for the V2 service release. Do not
assume that V1 database, ingest, data-file, TDMS-cache, or discovery settings
continue unchanged in V2.

## Package and Credential Requirements

Before V2 is publicly published:

1. Add the approved internal NuGet/Artifactory source to the language's package
   configuration.
2. Map only V2 packages and confirmed V2-only companion packages to that
   source.
3. Pin or lock the approved prerelease package version.
4. Authenticate developers through their user-level credential provider or
   package configuration.
5. Authenticate CI with protected secrets.
6. Never commit passwords, tokens, API keys, or credentials.

For the Python client, apply these rules using its package index configuration,
lockfile or dependency constraints, and CI secret mechanism rather than
copying .NET NuGet configuration.

## Validation Requirements

Each example must:

1. Build or install successfully from the selected V2 package.
2. Run independently against a supported MDS installation.
3. Use isolated V2 example data.
4. Print a concise, meaningful success result.
5. Exit unsuccessfully on service, schema, API, or read-back failure.
6. Verify one key V2 contract rule, not merely that the process completed.

Examples that need MDS should be runtime-tested on a controlled environment
with the required service release installed. CI may initially validate package
restore, build, and formatting only if it cannot provision MDS. Add automated
runtime tests when a supported MDS test environment becomes available.

## Delivery Order

1. Verify the selected V2 client package, public API surface, transport/client
   construction, target runtimes, and supported MDS release.
2. Add V2 package configuration and the language-specific shared context
   helper.
3. Implement Overview.
4. Implement Query.
5. Implement Stage 2 examples as their corresponding package and service
   verticals are released: Batched Publishing, Measurement Values, Update and
   Delete, and Local System.
6. Add non-scalar content to Measurement Values only after its value-store
   contract is delivered.
7. Implement Extension Attributes after the Stage 3 schema and workspace/RBAC
   capabilities are released.

## Explicitly Out of Scope

Do not add examples for these fourth-stage capabilities under this plan:

- DataFrame API.
- TestMonitor compatibility.
- HTTP mutation APIs.
