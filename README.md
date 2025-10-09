[![codecov](https://codecov.io/gh/meenzen/Zammad.Client.SystemTextJson/graph/badge.svg?token=tqT5gmR32w)](https://codecov.io/gh/meenzen/Zammad.Client.SystemTextJson)
[![NuGet](https://img.shields.io/nuget/v/Zammad.Client.SystemTextJson.svg)](https://www.nuget.org/packages/Zammad.Client.SystemTextJson)
[![NuGet](https://img.shields.io/nuget/dt/Zammad.Client.SystemTextJson.svg)](https://www.nuget.org/packages/Zammad.Client.SystemTextJson)

# Zammad.Client.SystemTextJson

A hard fork of [Zammad.Client](https://github.com/S3bt3r/Zammad.Client) with support for `System.Text.Json` instead of `Newtonsoft.Json`.

This library provides a .NET client for interacting with the [Zammad](https://zammad.org/) helpdesk system API.

## Installation

```bash
dotnet add package Zammad.Client.SystemTextJson
```

## Usage

Coming soon.

## Development

### Running Tests

Integration tests require a running Zammad instance. Start one locally with Docker Compose:

```bash
docker compose up -d
```

**Initial Setup** (first time only):

1. Open [http://localhost:8080/](http://localhost:8080/)
2. Complete the setup wizard
3. Create admin account with:
    - Email: `admin@example.org`
    - Password: `TestPassword1234`

Run the test suite:

```bash
dotnet test
```
