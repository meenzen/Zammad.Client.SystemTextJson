using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zammad.Client.Abstractions;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

#nullable enable

public sealed partial class ZammadClient : ITicketStateService
{
    public async Task<List<TicketState>> GetTicketStateListAsync() =>
        await GetAsync<List<TicketState>>("/api/v1/ticket_states") ?? [];

    public async Task<List<TicketState>> GetTicketStateListAsync(int page, int count) =>
        await GetAsync<List<TicketState>>("/api/v1/ticket_states", $"page={page}&per_page={count}") ?? [];

    public async Task<TicketState?> GetTicketStateAsync(int id) =>
        await GetAsync<TicketState>($"/api/v1/ticket_states/{id}");

    public async Task<TicketState> CreateTicketStateAsync(TicketState state) =>
        await PostAsync<TicketState>("/api/v1/ticket_states", state) ?? throw LogicException.UnexpectedNullResult;

    public async Task<TicketState> UpdateTicketStateAsync(int id, TicketState state) =>
        await PutAsync<TicketState>($"/api/v1/ticket_states/{id}", state) ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteTicketStateAsync(int id) => await DeleteAsync<bool>($"/api/v1/ticket_states/{id}");
}
