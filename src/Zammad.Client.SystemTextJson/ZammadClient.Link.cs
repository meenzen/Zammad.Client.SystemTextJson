using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

public interface ILinkService
{
    /// <summary>
    /// Lists the links of a ticket, with the linked tickets in <see cref="LinkList.Assets"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="Link.LinkType"/> is what the linked ticket is to <paramref name="ticketId"/>, so the same link is
    /// <see cref="LinkType.Parent"/> on one side and <see cref="LinkType.Child"/> on the other. Links to tickets the
    /// current user can't read are left out.
    /// </remarks>
    /// <exception cref="ZammadException">
    /// Since Zammad 7.2.1, thrown with <see cref="System.Net.HttpStatusCode.Forbidden"/> if the ticket doesn't exist
    /// or the current user can't read it. Older versions return an empty list instead.
    /// </exception>
    Task<LinkList> ListTicketLinksAsync(TicketId ticketId);

    /// <summary>
    /// Links another ticket to a ticket.
    /// </summary>
    /// <param name="ticketId">The ticket to add the link to. Needs change permission.</param>
    /// <param name="type">
    /// What the linked ticket is to <paramref name="ticketId"/>. <see cref="LinkType.Parent"/> makes the linked ticket
    /// the parent of <paramref name="ticketId"/>.
    /// </param>
    /// <param name="linkedTicketNumber">
    /// The <see cref="Ticket.Number"/> (not the ID) of the ticket to link. Needs read permission.
    /// </param>
    /// <remarks>
    /// Zammad rejects adding the same link twice ("Link already exists") and linking a ticket to itself ("An object
    /// cannot be linked to itself.") with 422. The duplicate check only compares links added from the same side, so
    /// adding the reverse link from the other ticket creates a second link that both tickets list.
    /// </remarks>
    Task AddTicketLinkAsync(TicketId ticketId, LinkType type, string linkedTicketNumber);

    /// <summary>
    /// Removes a link between two tickets.
    /// </summary>
    /// <param name="ticketId">The ticket to remove the link from. Needs change permission.</param>
    /// <param name="type">
    /// What the linked ticket is to <paramref name="ticketId"/>, as returned by <see cref="ListTicketLinksAsync"/>.
    /// </param>
    /// <param name="linkedTicketId">The ID of the linked ticket.</param>
    /// <remarks>
    /// The link is removed no matter which ticket it was added to. Does nothing if there is no such link.
    /// </remarks>
    Task RemoveTicketLinkAsync(TicketId ticketId, LinkType type, TicketId linkedTicketId);
}

public sealed partial class ZammadClient : ILinkService
{
    private const string LinksEndpoint = "/api/v1/links";
    private const string TicketLinkObject = "Ticket";

    public async Task<LinkList> ListTicketLinksAsync(TicketId ticketId)
    {
        var builder = new QueryBuilder();
        builder.Add("link_object", TicketLinkObject);
        builder.Add("link_object_value", ticketId.ToString());
        return await GetAsync<LinkList>(LinksEndpoint, builder.ToString()) ?? throw LogicException.UnexpectedNullResult;
    }

    public async Task AddTicketLinkAsync(TicketId ticketId, LinkType type, string linkedTicketNumber) =>
        await PostAsync<object>(
            $"{LinksEndpoint}/add",
            new LinkAddRequest
            {
                LinkType = type,
                TargetObject = TicketLinkObject,
                TargetValue = ticketId.ToTargetObjectId(),
                SourceObject = TicketLinkObject,
                SourceNumber = linkedTicketNumber,
            }
        );

    public async Task RemoveTicketLinkAsync(TicketId ticketId, LinkType type, TicketId linkedTicketId) =>
        await DeleteAsync<object>(
            $"{LinksEndpoint}/remove",
            new LinkRemoveRequest
            {
                LinkType = type,
                TargetObject = TicketLinkObject,
                TargetValue = ticketId.ToTargetObjectId(),
                SourceObject = TicketLinkObject,
                SourceValue = linkedTicketId.ToTargetObjectId(),
            }
        );
}
