using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface ITicketStateService
{
    Task<List<TicketState>> ListTicketStatesAsync();
    Task<List<TicketState>> ListTicketStatesAsync(int page, int count);
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

    public async Task<List<TicketState>> ListTicketStatesAsync(int page, int count)
    {
        var builder = new QueryBuilder();
        builder.Add("page", page);
        builder.Add("per_page", count);
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
