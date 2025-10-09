
using System.Text.Json.Serialization;
using Zammad.Client.Core.Internal;

namespace Zammad.Client.Resources.Internal
{
    
    public class TicketWithArticle : Ticket
    {
        [JsonPropertyName("article")]
        public TicketArticle Article { get; set; }

        public static TicketWithArticle Combine(Ticket ticket, TicketArticle article)
        {
            var combined = new TicketWithArticle();
            TypeUtility.CopyProperties(ticket, combined);
            combined.Article = article;
            return combined;
        }
    }
}
