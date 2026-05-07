using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface IMonitoringService
{
    Task<HealthCheckResult> HealthCheckAsync();
}

public sealed partial class ZammadClient : IUserService
{
    private const string MonitoringEndpoint = "/api/v1/monitoring";

    public async Task<HealthCheckResult> HealthCheckAsync() =>
        await GetAsync<HealthCheckResult>($"{MonitoringEndpoint}/health_check")
        ?? throw LogicException.UnexpectedNullResult;
}
