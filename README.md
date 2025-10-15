[![GitHub](https://img.shields.io/github/license/meenzen/Zammad.Client.SystemTextJson.svg)](https://github.com/meenzen/Zammad.Client.SystemTextJson/blob/master/LICENSE)
[![codecov](https://codecov.io/gh/meenzen/Zammad.Client.SystemTextJson/graph/badge.svg?token=tqT5gmR32w)](https://codecov.io/gh/meenzen/Zammad.Client.SystemTextJson)
[![NuGet](https://img.shields.io/nuget/v/Zammad.Client.SystemTextJson.svg)](https://www.nuget.org/packages/Zammad.Client.SystemTextJson)
[![NuGet](https://img.shields.io/nuget/dt/Zammad.Client.SystemTextJson.svg)](https://www.nuget.org/packages/Zammad.Client.SystemTextJson)

# Zammad.Client.SystemTextJson

A hard fork of [Zammad.Client](https://github.com/S3bt3r/Zammad.Client) with support for `System.Text.Json` instead of
`Newtonsoft.Json`.

This library provides a .NET client for interacting with the [Zammad](https://zammad.org/) helpdesk system API.

## Installation

```bash
dotnet add package Zammad.Client.SystemTextJson
```

## Usage

Basic example:

```csharp
var httpClient = new HttpClient();
var client = new ZammadClient(
    httpClient,
    Options.Create(new ZammadOptions
    {
        BaseUrl = new Uri("https://zammad.example.com/"),
        Token = "your_token_here",
    })
);

var user = await client.GetUserMeAsync();
Console.WriteLine($"Signed in as {user.FirstName} {user.LastName} ({user.Email})");
```

### Dependency Injection

Install the extensions package:

```bash
dotnet add package Zammad.Client.SystemTextJson.Extensions
```

Configure the client:

```csharp
builder.Services.AddZammadClient(options =>
{
    options.BaseUrl = new Uri("https://zammad.example.com/");
    options.Token = "your_token_here";
});
```

Alternatively, use a configuration section:

```csharp
builder.Services.AddZammadClient(builder.Configuration.GetSection("Zammad"));
```

Then inject the client:

```csharp
public class MyService(IZammadClient client)
{
    public async Task DoSomething()
     {
         var user = await client.GetUserMeAsync();
         Console.WriteLine($"Signed in as {user.FirstName} {user.LastName} ({user.Email})");
     }
}
```

### HttpClient Customization

The `AddZammadClient` method returns an `IHttpClientBuilder`, allowing further customization of the underlying
`HttpClient`. For example, to add a resilience handler:

```bash
dotnet add package Microsoft.Extensions.Http.Resilience
```

```csharp
builder.Services.AddZammadClient(builder.Configuration.GetSection("Zammad"))
    .AddStandardResilienceHandler();
```

This configuration will automatically handle transipent errors, making your application more robust.

## Contributing

Pull requests are welcome. Please use [Conventional Commits](https://www.conventionalcommits.org/) to keep
commit messages consistent.

Please consider adding tests for any new features or bug fixes.

## Acknowledgements

- [Zammad API Documentation](https://docs.zammad.org/en/latest/api/intro.html)
- The original [Zammad.Client](https://github.com/S3bt3r/Zammad.Client) library by [@S3bt3r](https://github.com/S3bt3r)

## License

Distributed under the [Apache License 2.0](https://choosealicense.com/licenses/apache-2.0/). See `LICENSE` for more
information.
