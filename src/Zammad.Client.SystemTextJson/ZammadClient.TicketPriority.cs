using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface ITicketPriorityService
{
    Task<List<TicketPriority>> ListTicketPrioritiesAsync();

    [Obsolete($"Use {nameof(Pagination)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    Task<List<TicketPriority>> ListTicketPrioritiesAsync(int page, int count);

    Task<List<TicketPriority>> ListTicketPrioritiesAsync(Pagination? pagination);
    Task<TicketPriority?> GetTicketPriorityAsync(PriorityId id);
    Task<TicketPriority> CreateTicketPriorityAsync(TicketPriority priority);
    Task<TicketPriority> UpdateTicketPriorityAsync(PriorityId id, TicketPriority priority);
    Task<bool> DeleteTicketPriorityAsync(PriorityId id);
}

public sealed partial class ZammadClient : ITicketPriorityService
{
    private const string TicketPrioritiesEndpoint = "/api/v1/ticket_priorities";

    public async Task<List<TicketPriority>> ListTicketPrioritiesAsync() =>
        await GetAsync<List<TicketPriority>>(TicketPrioritiesEndpoint) ?? [];

    [Obsolete($"Use {nameof(Pagination)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    public async Task<List<TicketPriority>> ListTicketPrioritiesAsync(int page, int count) =>
        await ListTicketPrioritiesAsync(new Pagination { Page = page, PerPage = count });

    public async Task<List<TicketPriority>> ListTicketPrioritiesAsync(Pagination? pagination)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<TicketPriority>>(TicketPrioritiesEndpoint, builder.ToString()) ?? [];
    }

    public async Task<TicketPriority?> GetTicketPriorityAsync(PriorityId id) =>
        await GetAsync<TicketPriority>($"{TicketPrioritiesEndpoint}/{id}");

    public async Task<TicketPriority> CreateTicketPriorityAsync(TicketPriority priority) =>
        await PostAsync<TicketPriority>(TicketPrioritiesEndpoint, priority)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<TicketPriority> UpdateTicketPriorityAsync(PriorityId id, TicketPriority priority) =>
        await PutAsync<TicketPriority>($"{TicketPrioritiesEndpoint}/{id}", priority)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteTicketPriorityAsync(PriorityId id) =>
        await DeleteAsync<bool>($"{TicketPrioritiesEndpoint}/{id}");
}
