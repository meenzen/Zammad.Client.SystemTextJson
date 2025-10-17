using System;

namespace Zammad.Client.IntegrationTests.Infrastructure;

public static class TestSetup
{
    public static string RandomString() => Guid.NewGuid().ToString("N").Substring(0, 8);

    public static TimeSpan IndexerDelay => TimeSpan.FromSeconds(5);
}
