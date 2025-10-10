using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Resources;

namespace Zammad.Client.Abstractions;

#nullable enable
public interface ITicketPriorityService
{
    Task<List<TicketPriority>> GetTicketPriorityListAsync();
    Task<List<TicketPriority>> GetTicketPriorityListAsync(int page, int count);
    Task<TicketPriority?> GetTicketPriorityAsync(int id);
    Task<TicketPriority> CreateTicketPriorityAsync(TicketPriority priority);
    Task<TicketPriority> UpdateTicketPriorityAsync(int id, TicketPriority priority);
    Task<bool> DeleteTicketPriorityAsync(int id);
}
