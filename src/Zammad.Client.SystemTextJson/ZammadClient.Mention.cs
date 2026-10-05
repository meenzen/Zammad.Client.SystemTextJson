using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

public interface IMentionService
{
    /// <summary>
    /// Lists the users subscribed to a ticket.
    /// </summary>
    /// <remarks>
    /// Needs read permission as an agent. Zammad responds with 403, not 404, if the ticket doesn't exist.
    /// </remarks>
    Task<List<Mention>> ListTicketMentionsAsync(TicketId ticketId);

    /// <summary>
    /// Subscribes the current user to a ticket. Does nothing if the user is already subscribed.
    /// </summary>
    /// <remarks>
    /// Zammad always subscribes the current user. To subscribe someone else, use a client that acts on behalf of that
    /// user (<see cref="ZammadOptions.OnBehalfOf"/>).
    /// </remarks>
    Task CreateTicketMentionAsync(TicketId ticketId);

    /// <summary>
    /// Unsubscribes the current user.
    /// </summary>
    /// <remarks>
    /// Users can only delete their own mentions. Zammad responds with 403 if the mention belongs to another user or
    /// doesn't exist.
    /// </remarks>
    Task DeleteMentionAsync(MentionId id);
}

public sealed partial class ZammadClient : IMentionService
{
    private const string MentionsEndpoint = "/api/v1/mentions";
    private const string TicketMentionableType = "Ticket";

    public async Task<List<Mention>> ListTicketMentionsAsync(TicketId ticketId)
    {
        var builder = new QueryBuilder();
        builder.Add("mentionable_type", TicketMentionableType);
        builder.Add("mentionable_id", ticketId.ToString());
        var list = await GetAsync<MentionList>(MentionsEndpoint, builder.ToString());
        return list?.Mentions ?? [];
    }

    public async Task CreateTicketMentionAsync(TicketId ticketId) =>
        await PostAsync<bool>(
            MentionsEndpoint,
            new MentionRequest { MentionableType = TicketMentionableType, MentionableId = ticketId.ToTargetObjectId() }
        );

    public async Task DeleteMentionAsync(MentionId id) => await DeleteAsync<bool>($"{MentionsEndpoint}/{id}");
}
