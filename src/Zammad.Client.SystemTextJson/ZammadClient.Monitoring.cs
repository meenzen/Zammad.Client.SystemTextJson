using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

public interface IMonitoringService
{
    Task<HealthCheckResult> HealthCheckAsync();

    /// <summary>
    /// Returns the number of records per table, when the last ones were created, and the storage size.
    /// </summary>
    Task<MonitoringStatus> GetMonitoringStatusAsync();

    /// <summary>
    /// Counts the tickets created within <see cref="AmountCheckQuery.Period"/> and compares the count with the
    /// thresholds.
    /// </summary>
    Task<AmountCheckResult> AmountCheckAsync(AmountCheckQuery query);

    /// <summary>
    /// Restarts the failed scheduler jobs.
    /// </summary>
    Task RestartFailedJobsAsync();

    /// <summary>
    /// Returns the version of Zammad, including the build for packaged installations, e.g.
    /// <c>7.2.0-9a69c6b8.docker</c>. Requires the <c>admin</c> permission.
    /// </summary>
    Task<string> GetVersionAsync();
}

public sealed partial class ZammadClient : IMonitoringService
{
    private const string MonitoringEndpoint = "/api/v1/monitoring";

    public async Task<HealthCheckResult> HealthCheckAsync() =>
        await GetAsync<HealthCheckResult>($"{MonitoringEndpoint}/health_check")
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<MonitoringStatus> GetMonitoringStatusAsync() =>
        await GetAsync<MonitoringStatus>($"{MonitoringEndpoint}/status") ?? throw LogicException.UnexpectedNullResult;

    public async Task<AmountCheckResult> AmountCheckAsync(AmountCheckQuery query)
    {
        var builder = new QueryBuilder();
        builder.Add("periode", AmountCheckQuery.FormatPeriod(query.Period));
        if (query.MaxWarning is { } maxWarning)
        {
            builder.Add("max_warning", maxWarning);
        }

        if (query.MaxCritical is { } maxCritical)
        {
            builder.Add("max_critical", maxCritical);
        }

        if (query.MinWarning is { } minWarning)
        {
            builder.Add("min_warning", minWarning);
        }

        if (query.MinCritical is { } minCritical)
        {
            builder.Add("min_critical", minCritical);
        }

        return await GetAsync<AmountCheckResult>($"{MonitoringEndpoint}/amount_check", builder.ToString())
            ?? throw LogicException.UnexpectedNullResult;
    }

    public async Task RestartFailedJobsAsync() => await PostAsync<object>($"{MonitoringEndpoint}/restart_failed_jobs");

    public async Task<string> GetVersionAsync() =>
        (await GetAsync<VersionResponse>("/api/v1/version"))?.Version ?? throw LogicException.UnexpectedNullResult;
}
