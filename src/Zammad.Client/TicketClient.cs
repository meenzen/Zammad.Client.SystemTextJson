using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;
using Zammad.Client.Services;

namespace Zammad.Client
{
    public class TicketClient
        : ZammadClient,
            ITicketService,
            ITicketArticleService,
            ITicketPriorityService,
            ITicketStateService
    {
        public TicketClient(ZammadAccount account)
            : base(account) { }

        #region ITicketService

        public Task<IList<Ticket>> GetTicketListAsync() => GetAsync<IList<Ticket>>("/api/v1/tickets");

        public Task<IList<Ticket>> GetTicketListAsync(int page, int count) =>
            GetAsync<IList<Ticket>>("/api/v1/tickets", $"page={page}&per_page={count}");

        public Task<IList<Ticket>> SearchTicketAsync(string query, int limit) =>
            GetAsync<IList<Ticket>>("/api/v1/tickets/search", $"query={query}&limit={limit}&expand=true");

        public Task<IList<Ticket>> SearchTicketAsync(string query, int limit, string sortBy, string orderBy) =>
            GetAsync<IList<Ticket>>(
                "/api/v1/tickets/search",
                $"query={query}&limit={limit}&expand=true&sort_by={sortBy}&order_by={orderBy}"
            );

        public Task<Ticket> GetTicketAsync(int id) => GetAsync<Ticket>($"/api/v1/tickets/{id}");

        public Task<Ticket> CreateTicketAsync(Ticket ticket, TicketArticle article) =>
            PostAsync<Ticket>("/api/v1/tickets", TicketWithArticle.Combine(ticket, article));

        public Task<Ticket> UpdateTicketAsync(int id, Ticket ticket) =>
            PutAsync<Ticket>($"/api/v1/tickets/{id}", ticket);

        public Task<bool> DeleteTicketAsync(int id) => DeleteAsync<bool>($"/api/v1/tickets/{id}");

        #endregion

        #region ITicketArticleService

        public Task<IList<TicketArticle>> GetTicketArticleListAsync() =>
            GetAsync<IList<TicketArticle>>("/api/v1/ticket_articles");

        public Task<IList<TicketArticle>> GetTicketArticleListAsync(int page, int count) =>
            GetAsync<IList<TicketArticle>>("/api/v1/ticket_articles", $"page={page}&per_page={count}");

        public Task<IList<TicketArticle>> GetTicketArticleListForTicketAsync(int ticketId) =>
            GetAsync<IList<TicketArticle>>($"/api/v1/ticket_articles/by_ticket/{ticketId}");

        public Task<TicketArticle> GetTicketArticleAsync(int id) =>
            GetAsync<TicketArticle>($"/api/v1/ticket_articles/{id}");

        public Task<TicketArticle> CreateTicketArticleAsync(TicketArticle article) =>
            PostAsync<TicketArticle>("/api/v1/ticket_articles", article);

        public Task<Stream> GetTicketArticleAttachmentAsync(int ticketId, int articleId, int id) =>
            GetAsync<Stream>($"/api/v1/ticket_attachment/{ticketId}/{articleId}/{id}");

        #endregion

        #region ITicketPriorityService

        public Task<IList<TicketPriority>> GetTicketPriorityListAsync() =>
            GetAsync<IList<TicketPriority>>("/api/v1/ticket_priorities");

        public Task<IList<TicketPriority>> GetTicketPriorityListAsync(int page, int count) =>
            GetAsync<IList<TicketPriority>>("/api/v1/ticket_priorities", $"page={page}&per_page={count}");

        public Task<TicketPriority> GetTicketPriorityAsync(int id) =>
            GetAsync<TicketPriority>($"/api/v1/ticket_priorities/{id}");

        public Task<TicketPriority> CreateTicketPriorityAsync(TicketPriority priority) =>
            PostAsync<TicketPriority>("/api/v1/ticket_priorities", priority);

        public Task<TicketPriority> UpdateTicketPriorityAsync(int id, TicketPriority priority) =>
            PutAsync<TicketPriority>($"/api/v1/ticket_priorities/{id}", priority);

        public Task<bool> DeleteTicketPriorityAsync(int id) => DeleteAsync<bool>($"/api/v1/ticket_priorities/{id}");

        #endregion

        #region ITicketStateService

        public Task<IList<TicketState>> GetTicketStateListAsync() =>
            GetAsync<IList<TicketState>>("/api/v1/ticket_states");

        public Task<IList<TicketState>> GetTicketStateListAsync(int page, int count) =>
            GetAsync<IList<TicketState>>("/api/v1/ticket_states", $"page={page}&per_page={count}");

        public Task<TicketState> GetTicketStateAsync(int id) => GetAsync<TicketState>($"/api/v1/ticket_states/{id}");

        public Task<TicketState> CreateTicketStateAsync(TicketState state) =>
            PostAsync<TicketState>("/api/v1/ticket_states", state);

        public Task<TicketState> UpdateTicketStateAsync(int id, TicketState state) =>
            PutAsync<TicketState>($"/api/v1/ticket_states/{id}", state);

        public Task<bool> DeleteTicketStateAsync(int id) => DeleteAsync<bool>($"/api/v1/ticket_states/{id}");

        #endregion
    }
}
