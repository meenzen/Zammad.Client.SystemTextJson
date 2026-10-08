[![GitHub](https://img.shields.io/github/license/meenzen/Zammad.Client.SystemTextJson.svg)](https://github.com/meenzen/Zammad.Client.SystemTextJson/blob/master/LICENSE)
[![codecov](https://codecov.io/gh/meenzen/Zammad.Client.SystemTextJson/graph/badge.svg?token=tqT5gmR32w)](https://codecov.io/gh/meenzen/Zammad.Client.SystemTextJson)
[![NuGet](https://img.shields.io/nuget/v/Zammad.Client.SystemTextJson.svg)](https://www.nuget.org/packages/Zammad.Client.SystemTextJson)
[![NuGet](https://img.shields.io/nuget/dt/Zammad.Client.SystemTextJson.svg)](https://www.nuget.org/packages/Zammad.Client.SystemTextJson)

# Zammad.Client.SystemTextJson

A hard fork of [Zammad.Client](https://github.com/S3bt3r/Zammad.Client) with support for `System.Text.Json` instead of
`Newtonsoft.Json`.

This library provides a .NET client for interacting with the [Zammad](https://zammad.org/) helpdesk system API.

## Compatibility

The integration tests on `master` run against **Zammad 7.2.2** (`ghcr.io/zammad/zammad:7.2.2`). Each release is
tested against one Zammad version only:

| Library version    | Tested against Zammad |
| ------------------ | --------------------- |
| `master` (> 4.0.0) | 7.2.2                 |
| 4.0.0              | 7.2.0                 |
| 3.0.0              | 7.0.1                 |
| 2.0.0 – 2.4.0      | 6.5.2                 |

Other versions usually work as long as the REST API hasn't changed, but they aren't tested.

Zammad changes that affect this client:

- **7.2.1**: `ListTicketLinksAsync` throws a `ZammadException` with `403 Forbidden` for a ticket that doesn't exist or
  that the user can't read. Before, it returned an empty list.
- **7.2.1**: Overviews reject an `order.direction` or `group_direction` other than `ASC` or `DESC` with
  `422 Unprocessable Entity`.
- **7.2.1**: Agents without admin permissions can no longer update users that have any `admin.*` permission.
- **7.2.1**: `GetOnlineNotificationAsync` throws a `ZammadException` with `403 Forbidden` for a notification that
  doesn't exist. Before, it returned `null`. Notifications can also only be read or updated while the user can still
  access the related ticket.
- **7.2.1**: Articles created by customers drop `preferences`.

See Zammad's [breaking changes](https://github.com/zammad/zammad/blob/stable/BREAKING_CHANGES.md) for everything else.
The 7.2.1 breaking changes (`email_verify`, `getting_started`, `signshow` and the WebSocket login) concern endpoints
this client doesn't use.

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

This configuration will automatically handle transient errors, making your application more robust.

### Custom Fields

Custom attributes configured in the Zammad object manager end up in the `ExtensionData` dictionary of `Ticket`, `User`,
`Organization` and `Group`. The extension methods in `Zammad.Client.Resources.CustomFieldExtensions` convert them from
and to regular .NET types:

```csharp
using Zammad.Client.Resources;

// Set custom fields when creating or updating a resource
var ticket = await client.CreateTicketAsync(
    new Ticket { Title = "Printer is on fire", GroupId = new GroupId(1), CustomerId = customerId }
        .WithCustomField("product", "Printer 3000")
        .WithCustomField("serial_number", 12345)
        .WithCustomField("affected_sites", new[] { "berlin", "hamburg" }),
    article
);

// Read them back
string? product = ticket.GetCustomField<string>("product");
int serialNumber = ticket.GetCustomField<int>("serial_number");
List<string>? sites = ticket.GetCustomField<List<string>>("affected_sites");

// Change or clear a single field
var update = new Ticket().WithCustomField("product", "Printer 4000");
update.ClearCustomField("serial_number");
await client.UpdateTicketAsync(ticket.Id, update);
```

If you have many custom fields, you can map all of them to a class with `GetCustomFields<T>()` and
`SetCustomFields(fields)`. Use `[JsonPropertyName]` to match the attribute names.

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
