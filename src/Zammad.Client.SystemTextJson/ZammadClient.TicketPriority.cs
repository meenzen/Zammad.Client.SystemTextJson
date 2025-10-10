using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zammad.Client.Abstractions;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

#nullable enable
public sealed partial class ZammadClient : ITicketPriorityService
{
    public async Task<List<TicketPriority>> GetTicketPriorityListAsync() =>
        await GetAsync<List<TicketPriority>>("/api/v1/ticket_priorities") ?? [];

    public async Task<List<TicketPriority>> GetTicketPriorityListAsync(int page, int count) =>
        await GetAsync<List<TicketPriority>>("/api/v1/ticket_priorities", $"page={page}&per_page={count}") ?? [];

    public async Task<TicketPriority?> GetTicketPriorityAsync(int id) =>
        await GetAsync<TicketPriority>($"/api/v1/ticket_priorities/{id}");

    public async Task<TicketPriority> CreateTicketPriorityAsync(TicketPriority priority) =>
        await PostAsync<TicketPriority>("/api/v1/ticket_priorities", priority)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<TicketPriority> UpdateTicketPriorityAsync(int id, TicketPriority priority) =>
        await PutAsync<TicketPriority>($"/api/v1/ticket_priorities/{id}", priority)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteTicketPriorityAsync(int id) =>
        await DeleteAsync<bool>($"/api/v1/ticket_priorities/{id}");
}
