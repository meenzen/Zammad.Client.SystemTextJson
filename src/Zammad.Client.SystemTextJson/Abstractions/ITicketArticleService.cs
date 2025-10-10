using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Zammad.Client.Resources;

namespace Zammad.Client.Abstractions;

public interface ITicketArticleService
{
    Task<List<TicketArticle>> GetTicketArticleListAsync();
    Task<List<TicketArticle>> GetTicketArticleListAsync(int page, int count);
    Task<List<TicketArticle>> GetTicketArticleListForTicketAsync(int ticketId);
    Task<TicketArticle> GetTicketArticleAsync(int id);
    Task<TicketArticle> CreateTicketArticleAsync(TicketArticle article);
    Task<Stream> GetTicketArticleAttachmentAsync(int ticketId, int articleId, int id);
}
