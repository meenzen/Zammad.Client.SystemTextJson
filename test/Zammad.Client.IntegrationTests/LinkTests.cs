using System.Net;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class LinkTests(ZammadStackFixture zammadStack)
{
    private static readonly string Id = TestSetup.RandomString();
    private static Ticket? ParentTicket { get; set; }
    private static Ticket? ChildTicket { get; set; }

    private static async Task<Ticket> CreateTicketAsync(IZammadClient client, string name) =>
        await client.CreateTicketAsync(
            new Ticket
            {
                Title = $"LinkTests {name} {Id}",
                GroupId = new GroupId(1),
                CustomerId = new UserId(1),
            },
            new TicketArticle
            {
                Subject = "LinkTests",
                Body = "LinkTests",
                Type = "note",
            }
        );

    [Test]
    public async Task CreateTickets()
    {
        var client = await zammadStack.GetClientAsync();

        ParentTicket = await CreateTicketAsync(client, "Parent");
        ChildTicket = await CreateTicketAsync(client, "Child");

        await Assert.That((await client.ListTicketLinksAsync(ChildTicket.Id)).Links).IsEmpty();
    }

    [Test]
    [DependsOn(nameof(CreateTickets))]
    public async Task AddTicketLink()
    {
        var client = await zammadStack.GetClientAsync();

        // Make the parent ticket the parent of the child ticket
        await client.AddTicketLinkAsync(ChildTicket!.Id, LinkType.Parent, ParentTicket!.Number!);

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.AddTicketLinkAsync(ChildTicket.Id, LinkType.Parent, ParentTicket.Number!)
        );
        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(exception.Error).IsEqualTo("Link already exists");
    }

    [Test]
    [DependsOn(nameof(CreateTickets))]
    public async Task AddTicketLink_Invalid()
    {
        var client = await zammadStack.GetClientAsync();

        var self = await Assert.ThrowsAsync<ZammadException>(() =>
            client.AddTicketLinkAsync(ParentTicket!.Id, LinkType.Normal, ParentTicket.Number!)
        );
        await Assert.That(self!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(self.Error).IsEqualTo("An object cannot be linked to itself.");

        var missing = await Assert.ThrowsAsync<ZammadException>(() =>
            client.AddTicketLinkAsync(ParentTicket!.Id, LinkType.Normal, "does-not-exist")
        );
        await Assert.That(missing!.Code).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    [DependsOn(nameof(AddTicketLink))]
    public async Task ListTicketLinks()
    {
        var client = await zammadStack.GetClientAsync();

        // Each side sees what the other ticket is to it
        var childLinks = await client.ListTicketLinksAsync(ChildTicket!.Id);
        await Assert.That(childLinks.Links).HasSingleItem();
        var link = childLinks.Links[0];
        await Assert.That(link.LinkType).IsEqualTo(LinkType.Parent);
        await Assert.That(link.LinkObject).IsEqualTo("Ticket");
        await Assert.That(link.TicketId).IsEqualTo(ParentTicket!.Id);
        await Assert.That(childLinks.Assets.GetTicket(ParentTicket.Id)?.Title).IsEqualTo(ParentTicket.Title);
        // The ticket assets come with their users
        await Assert.That(childLinks.Assets.GetUser(new UserId(1))).IsNotNull();

        var parentLinks = await client.ListTicketLinksAsync(ParentTicket.Id);
        await Assert.That(parentLinks.Links).HasSingleItem();
        await Assert.That(parentLinks.Links[0].LinkType).IsEqualTo(LinkType.Child);
        await Assert.That(parentLinks.Links[0].TicketId).IsEqualTo(ChildTicket.Id);
    }

    [Test]
    [DependsOn(nameof(ListTicketLinks))]
    public async Task AddTicketLink_ReverseIsNotADuplicate()
    {
        var client = await zammadStack.GetClientAsync();

        // The same relation added from the other side passes the duplicate check and creates a second link
        await client.AddTicketLinkAsync(ParentTicket!.Id, LinkType.Child, ChildTicket!.Number!);

        var childLinks = await client.ListTicketLinksAsync(ChildTicket.Id);
        await Assert.That(childLinks.Links.Count).IsEqualTo(2);
        await Assert.That(childLinks.Links).All(l => l.LinkType == LinkType.Parent && l.TicketId == ParentTicket.Id);

        // Removing works from either side, and removes both
        await client.RemoveTicketLinkAsync(ParentTicket.Id, LinkType.Child, ChildTicket.Id);
        await Assert.That((await client.ListTicketLinksAsync(ChildTicket.Id)).Links).IsEmpty();

        await client.AddTicketLinkAsync(ChildTicket.Id, LinkType.Parent, ParentTicket.Number!);
    }

    [Test]
    [DependsOn(nameof(AddTicketLink_ReverseIsNotADuplicate))]
    public async Task RemoveTicketLink()
    {
        var client = await zammadStack.GetClientAsync();

        // The wrong type doesn't match
        await client.RemoveTicketLinkAsync(ChildTicket!.Id, LinkType.Child, ParentTicket!.Id);
        await Assert.That((await client.ListTicketLinksAsync(ChildTicket.Id)).Links).HasSingleItem();

        await client.RemoveTicketLinkAsync(ChildTicket.Id, LinkType.Parent, ParentTicket.Id);

        await Assert.That((await client.ListTicketLinksAsync(ChildTicket.Id)).Links).IsEmpty();
        await Assert.That((await client.ListTicketLinksAsync(ParentTicket.Id)).Links).IsEmpty();
    }

    [Test]
    [DependsOn(nameof(RemoveTicketLink))]
    [DependsOn(nameof(AddTicketLink_Invalid))]
    public async Task DeleteTickets()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteTicketAsync(ParentTicket!.Id);
        await client.DeleteTicketAsync(ChildTicket!.Id);
    }
}
