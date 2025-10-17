using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface ITicketStateService
{
    Task<List<TicketState>> ListTicketStatesAsync();

    [Obsolete($"Use {nameof(Pagination)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    Task<List<TicketState>> ListTicketStatesAsync(int page, int count);

    Task<List<TicketState>> ListTicketStatesAsync(Pagination? pagination);
    Task<TicketState?> GetTicketStateAsync(StateId id);
    Task<TicketState> CreateTicketStateAsync(TicketState state);
    Task<TicketState> UpdateTicketStateAsync(StateId id, TicketState state);
    Task<bool> DeleteTicketStateAsync(StateId id);
}

public sealed partial class ZammadClient : ITicketStateService
{
    private const string TicketStatesEndpoint = "/api/v1/ticket_states";

    public async Task<List<TicketState>> ListTicketStatesAsync() =>
        await GetAsync<List<TicketState>>(TicketStatesEndpoint) ?? [];

    [Obsolete($"Use {nameof(Pagination)} overload instead.")]
    [SuppressMessage("Info Code Smell", "S1133:Deprecated code should be removed")]
    public async Task<List<TicketState>> ListTicketStatesAsync(int page, int count) =>
        await ListTicketStatesAsync(new Pagination { Page = page, PerPage = count });

    public async Task<List<TicketState>> ListTicketStatesAsync(Pagination? pagination)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<TicketState>>(TicketStatesEndpoint, builder.ToString()) ?? [];
    }

    public async Task<TicketState?> GetTicketStateAsync(StateId id) =>
        await GetAsync<TicketState>($"{TicketStatesEndpoint}/{id}");

    public async Task<TicketState> CreateTicketStateAsync(TicketState state) =>
        await PostAsync<TicketState>(TicketStatesEndpoint, state) ?? throw LogicException.UnexpectedNullResult;

    public async Task<TicketState> UpdateTicketStateAsync(StateId id, TicketState state) =>
        await PutAsync<TicketState>($"{TicketStatesEndpoint}/{id}", state) ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteTicketStateAsync(StateId id) =>
        await DeleteAsync<bool>($"{TicketStatesEndpoint}/{id}");
}
