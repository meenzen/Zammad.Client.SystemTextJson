using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

/// <summary>
/// Service level agreements (<c>/api/v1/slas</c>). Requires the <c>admin.sla</c> permission.
/// </summary>
public interface ISlaService
{
    Task<List<Sla>> ListSlasAsync(Pagination? pagination = null);

    Task<Sla?> GetSlaAsync(SlaId id);

    /// <summary>
    /// Creates an SLA.
    /// </summary>
    /// <remarks>
    /// <see cref="Sla.Name"/> (unique, case-insensitive) and <see cref="Sla.CalendarId"/> are required. An SLA can
    /// have either a <see cref="Sla.ResponseTime"/> or an <see cref="Sla.UpdateTime"/>, not both. An SLA without a
    /// <see cref="Sla.Condition"/> applies to every ticket that no other SLA matches. Every change that affects
    /// escalation makes Zammad recalculate the escalation times of all open tickets in the background.
    /// </remarks>
    Task<Sla> CreateSlaAsync(Sla sla);

    Task<Sla> UpdateSlaAsync(SlaId id, Sla sla);

    Task DeleteSlaAsync(SlaId id);
}

public sealed partial class ZammadClient : ISlaService
{
    private const string SlasEndpoint = "/api/v1/slas";

    public async Task<List<Sla>> ListSlasAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<Sla>>(SlasEndpoint, builder.ToString()) ?? [];
    }

    public async Task<Sla?> GetSlaAsync(SlaId id) => await GetAsync<Sla>($"{SlasEndpoint}/{id}");

    public async Task<Sla> CreateSlaAsync(Sla sla) =>
        await PostAsync<Sla>(SlasEndpoint, sla) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Sla> UpdateSlaAsync(SlaId id, Sla sla) =>
        await PutAsync<Sla>($"{SlasEndpoint}/{id}", sla) ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteSlaAsync(SlaId id) => await DeleteAsync<bool>($"{SlasEndpoint}/{id}");
}
