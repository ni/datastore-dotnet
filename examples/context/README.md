# DataStoreContext for C#

A C# implementation of the DataStoreContext class that provides a context manager for running a data store in an isolated environment. This is a direct port of the Python `DataStoreContext` from the utilities package.

## Overview

The `DataStoreContext` class implements the `IDisposable` interface, allowing it to be used with C#'s `using` statement pattern. It manages environment variables for the NI Measurement Data Store, ensuring isolated test environments and automatic cleanup.

## Features

- **IDisposable Pattern**: Full support for C# `using` statements
- **Environment Isolation**: Saves and restores environment variables
- **Automatic Cleanup**: Environment variables are restored when disposed
- **Unique Cluster ID**: Generates a unique cluster ID based on the base directory path
- **Configurable Paths**: Optional custom base directory for data store files

## Environment Variables Managed

The class manages the following environment variables:

- `NIDISCOVERY_CLUSTERID` - Discovery service cluster identifier
- `NIDATASTORE_DATASTORESETTINGS__SQLITEDATABASEPATH` - SQLite database path
- `NIDATASTORE_DATASTORESETTINGS__DATAFILESDIRECTORY` - Data files directory
- `NIDATASTORE_DATASTORESETTINGS__INGESTDIRECTORY` - Ingest directory
- `NIDATASTORE_DATASTORESETTINGS__FAILEDINGESTDIRECTORY` - Failed ingest directory
- `NIDATASTORE_DATASTORESETTINGS__TDMSFILECACHEEXPIRATIONSECONDS` - TDMS cache expiration

## Usage

### Option 1: Using Statement (Recommended - C# 8.0+)

```csharp
using NI.DataStore.Utilities;

// Modern C# using declaration
using var context = new DataStoreContext();
// Context is automatically initialized
// Your data store operations here
// Context is automatically disposed and cleaned up at end of scope
```

### Option 2: Using Block

```csharp
using NI.DataStore.Utilities;

using (var context = new DataStoreContext())
{
    // Your data store operations here
}
// Context is automatically disposed here
```

### Option 3: Manual Initialization and Cleanup

```csharp
using NI.DataStore.Utilities;

var context = new DataStoreContext();
context.Initialize();
try
{
    // Your data store operations here
}
finally
{
    context.Close();
}
```

### With Custom Base Directory

```csharp
using NI.DataStore.Utilities;

using var context = new DataStoreContext(@"C:\MyCustomDataPath");
// All data store files will be created under C:\MyCustomDataPath
```

## Directory Structure Created

When initialized, the context sets up the following directory structure (relative to the base directory):

```
temp_data/
├── MetadataStore.db           # SQLite database
├── DataFiles/                 # Data files directory
├── Ingest/                    # Ingest directory
└── FailedIngest/              # Failed ingest directory
```

## Integration with Data Store Client

```csharp
using NI.DataStore.Utilities;
using NationalInstruments.MeasurementLink.Discovery.V1;
using NationalInstruments.Measurements.Data.V1;
using NationalInstruments.Measurements.Metadata.V1;
using static NationalInstruments.Measurements.Data.V1.DataStoreService;
using static NationalInstruments.Measurements.Metadata.V1.MetadataStoreService;

// Initialize context with using statement
using var context = new DataStoreContext();

// Create clients
var clientStubFactory = new GrpcClientStubFactory();
var dataStoreServiceClient = clientStubFactory.CreateClient<DataStoreServiceClient>();
var metadataStoreServiceClient = clientStubFactory.CreateClient<MetadataStoreServiceClient>();

// Use the clients
// ...
```

## Key Differences from Python Version

1. **Constructor initialization**: The C# version calls `Initialize()` in the constructor, making it ready to use immediately when created with `using`.
2. **IDisposable pattern**: Instead of `__enter__` and `__exit__`, uses `IDisposable` and `Dispose()`.
3. **Explicit initialization option**: Still supports manual `Initialize()` and `Close()` calls for advanced scenarios.

## Examples

See the examples in the parent directory for full usage demonstrations:
- `Alias/Alias.cs` - Using DataStoreContext with alias operations
- `Extension Attributes/Extension Attributes.cs` - Using DataStoreContext with extension attributes
