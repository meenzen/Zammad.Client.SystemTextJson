using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface ITicketStateService
{
    Task<List<TicketState>> GetTicketStateListAsync();
    Task<List<TicketState>> GetTicketStateListAsync(int page, int count);
    Task<TicketState?> GetTicketStateAsync(StateId id);
    Task<TicketState> CreateTicketStateAsync(TicketState state);
    Task<TicketState> UpdateTicketStateAsync(StateId id, TicketState state);
    Task<bool> DeleteTicketStateAsync(StateId id);
}

public sealed partial class ZammadClient : ITicketStateService
{
    public async Task<List<TicketState>> GetTicketStateListAsync() =>
        await GetAsync<List<TicketState>>("/api/v1/ticket_states") ?? [];

    public async Task<List<TicketState>> GetTicketStateListAsync(int page, int count) =>
        await GetAsync<List<TicketState>>("/api/v1/ticket_states", $"page={page}&per_page={count}") ?? [];

    public async Task<TicketState?> GetTicketStateAsync(StateId id) =>
        await GetAsync<TicketState>($"/api/v1/ticket_states/{id}");

    public async Task<TicketState> CreateTicketStateAsync(TicketState state) =>
        await PostAsync<TicketState>("/api/v1/ticket_states", state) ?? throw LogicException.UnexpectedNullResult;

    public async Task<TicketState> UpdateTicketStateAsync(StateId id, TicketState state) =>
        await PutAsync<TicketState>($"/api/v1/ticket_states/{id}", state) ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteTicketStateAsync(StateId id) =>
        await DeleteAsync<bool>($"/api/v1/ticket_states/{id}");
}
