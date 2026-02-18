# Datastore .NET Examples

This repo contains the source code for examples that show how to use the .NET API of NI Measurement Data Services.
These examples do not go through any build process, you can simply copy them to a system that has the necessary
software installed.

## Running the Examples

1. Clone or download this repository.
2. Copy the contents of `Examples` to a system that has NI Measurement Data Services and the .NET framework installed.
3. Compile and run the console applications defined in the .csproj files in the `Examples` directory.
4. Be aware that running these examples will publish data to the default datastore location on your system. Data that is
published by a certain example may show up in queries from other examples or from your own Measurement Data Services application.

## Installation

As a prerequisite to using the DataStore .NET API, you must install Measurement Data Services
Software 2026 Q1 or later on your system. You can download and install this software using
[NI Package Manager](https://www.ni.com/en/support/downloads/software-products/download.package-manager.html).