using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface ITicketAccountingService
{
    Task<List<TicketAccounting>> ListTicketAccountingsAsync(TicketId ticketId);
    Task<TicketAccounting?> GetTicketAccountingAsync(TicketId ticketId, TimeAccountingId id);
    Task<TicketAccounting> CreateTicketAccountingAsync(TicketId ticketId, TicketAccounting accounting);
    Task<TicketAccounting> UpdateTicketAccountingAsync(
        TicketId ticketId,
        TimeAccountingId id,
        TicketAccounting accounting
    );
    Task<bool> DeleteTicketAccountingAsync(TicketId ticketId, TimeAccountingId id);
}

public sealed partial class ZammadClient : ITicketAccountingService
{
    public async Task<List<TicketAccounting>> ListTicketAccountingsAsync(TicketId ticketId) =>
        await GetAsync<List<TicketAccounting>>($"{TicketsEndpoint}/{ticketId}/time_accountings") ?? [];

    public async Task<TicketAccounting?> GetTicketAccountingAsync(TicketId ticketId, TimeAccountingId id) =>
        await GetAsync<TicketAccounting>($"{TicketsEndpoint}/{ticketId}/time_accountings/{id}");

    public async Task<TicketAccounting> CreateTicketAccountingAsync(TicketId ticketId, TicketAccounting accounting) =>
        await PostAsync<TicketAccounting>($"{TicketsEndpoint}/{ticketId}/time_accountings", accounting)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<TicketAccounting> UpdateTicketAccountingAsync(
        TicketId ticketId,
        TimeAccountingId id,
        TicketAccounting accounting
    ) =>
        await PutAsync<TicketAccounting>($"{TicketsEndpoint}/{ticketId}/time_accountings/{id}", accounting)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> DeleteTicketAccountingAsync(TicketId ticketId, TimeAccountingId id) =>
        await DeleteAsync<bool>($"{TicketsEndpoint}/{ticketId}/time_accountings/{id}");
}
