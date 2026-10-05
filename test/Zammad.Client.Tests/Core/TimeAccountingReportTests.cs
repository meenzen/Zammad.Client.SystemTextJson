using Microsoft.Extensions.Options;
using Zammad.Client.Resources;

namespace Zammad.Client.Tests.Core;

public sealed class TimeAccountingReportTests
{
    private static ZammadClient CreateClient() =>
        new(
            new HttpClient(),
            Options.Create(new ZammadOptions { BaseUrl = new Uri("http://localhost:1"), Token = "unused" })
        );

    [Test]
    [Arguments(0)]
    [Arguments(13)]
    public async Task Report_ThrowsForInvalidMonth(int month)
    {
        var client = CreateClient();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            client.GetTimeAccountingByActivityAsync(2026, month)
        );
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            client.DownloadTimeAccountingReportAsync(TimeAccountingReport.ByTicket, 2026, month)
        );
    }
}
