# DataStoreContext for C#

A C# implementation of the DataStoreContext class that provides a context manager for running a data store in an isolated environment.

## Overview

The `DataStoreContext` class implements the `IDisposable` interface, allowing it
to be used with C#'s `using` statement pattern. It manages environment variables
for the NI Measurement Data Store, ensuring isolated test environments and
automatic cleanup.


## Features

- **IDisposable Pattern**: Full support for C# `using` statements
- **Environment Isolation**: Saves and restores environment variables
- **Automatic Cleanup**: Environment variables are restored when disposed
- **Unique Cluster ID**: Generates a unique cluster ID based on the base directory path
- **Configurable Paths**: Optional custom base directory for data store files

## Environment Variables Managed

The class manages the following environment variables:

- `NIDiscovery_ClusterId` - Discovery service cluster identifier
- `NIDATASTORE_DATASTORESETTINGS__SQLITEDATABASEPATH` - SQLite database path
- `NIDATASTORE_DATASTORESETTINGS__DATAFILESDIRECTORY` - Data files directory
- `NIDATASTORE_DATASTORESETTINGS__INGESTDIRECTORY` - Ingest directory
- `NIDATASTORE_DATASTORESETTINGS__FAILEDINGESTDIRECTORY` - Failed ingest directory
- `NIDATASTORE_DATASTORESETTINGS__TDMSFILECACHEEXPIRATIONSECONDS` - TDMS cache expiration

## Usage

### Option 1: Using Statement (Recommended - C# 8.0+)

```csharp
using NationalInstruments.DataStore.Utilities;

// Modern C# using declaration
using var context = new DataStoreContext();
// Context is automatically initialized
// Your data store operations here
// Context is automatically disposed and cleaned up at end of scope
```

### Option 2: Using Block

```csharp
using NationalInstruments.DataStore.Utilities;

using (var context = new DataStoreContext())
{
    // Your data store operations here
}
// Context is automatically disposed here
```

### Option 3: Manual Cleanup

```csharp
using NationalInstruments.DataStore.Utilities;

var context = new DataStoreContext();
try
{
    // Your data store operations here
}
finally
{
    context.Dispose();
}
```

### With Custom Base Directory

```csharp
using NationalInstruments.DataStore.Utilities;

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

## Examples

See the examples in the parent directory for full usage demonstrations.
