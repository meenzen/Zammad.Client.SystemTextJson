using System;
using System.Net.Http;
using Microsoft.Extensions.Options;
using Zammad.Client.Abstractions;

namespace Zammad.Client.IntegrationTests;

public static class TestHelper
{
    public static readonly IZammadClient Client = GetClient();

    private static ZammadClient GetClient()
    {
        ZammadClient client = new ZammadClient(
            new HttpClient(),
            Options.Create(
                new ZammadOptions
                {
                    BaseUrl = new Uri("http://127.0.0.1:8080"),
                    Username = "admin@example.org",
                    Password = "TestPassword1234",
                }
            )
        );
        return client;
    }
}
