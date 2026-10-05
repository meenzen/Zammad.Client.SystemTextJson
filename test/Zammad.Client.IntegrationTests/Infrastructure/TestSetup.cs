namespace Zammad.Client.IntegrationTests.Infrastructure;

public static class TestSetup
{
    /// <summary>
    /// Returns 8 random lowercase letters.
    /// </summary>
    /// <remarks>
    /// No digits: Zammad stores any run of 6+ digits in a user's string attributes as a CTI caller ID
    /// (<c>Cti::CallerId</c>), and that row makes deleting the user fail with 422 "object has references".
    /// </remarks>
    public static string RandomString() =>
        string.Concat(
            Guid.NewGuid().ToString("N").Substring(0, 8).Select(c => char.IsDigit(c) ? (char)('g' + (c - '0')) : c)
        );

    public static TimeSpan IndexerDelay => TimeSpan.FromSeconds(5);
    public const int BackoffMs = 1000;
    public const int RetryCount = 3;
}
