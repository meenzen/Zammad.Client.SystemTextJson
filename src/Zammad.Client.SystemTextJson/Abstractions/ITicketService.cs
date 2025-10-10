using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Resources;

namespace Zammad.Client.Abstractions;

public interface ITicketService
{
    Task<List<Ticket>> GetTicketListAsync();
    Task<List<Ticket>> GetTicketListAsync(int page, int count);
    Task<List<Ticket>> SearchTicketAsync(string query, int limit);
    Task<List<Ticket>> SearchTicketAsync(string query, int limit, string sortBy, string orderBy);
    Task<Ticket> GetTicketAsync(int id);
    Task<Ticket> CreateTicketAsync(Ticket ticket, TicketArticle article);
    Task<Ticket> UpdateTicketAsync(int id, Ticket ticket);
    Task<bool> DeleteTicketAsync(int id);
}
