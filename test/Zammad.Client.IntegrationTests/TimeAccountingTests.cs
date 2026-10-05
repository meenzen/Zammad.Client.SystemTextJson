using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

/// <remarks>
/// Other tests create time accounting entries in parallel, so the reports are only checked for this class's entry.
/// </remarks>
[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class TimeAccountingTests(ZammadStackFixture zammadStack)
{
    private static readonly string Id = TestSetup.RandomString();
    private static readonly string TypeName = "Test Type " + Id;
    private static TimeAccountingTypeId CreatedTypeId { get; set; } = TimeAccountingTypeId.Empty;
    private static TicketId TestTicketId { get; set; } = TicketId.Empty;
    private static OrganizationId TestOrganizationId { get; set; } = OrganizationId.Empty;
    private static UserId TestCustomerId { get; set; } = UserId.Empty;
    private static TimeAccountingId CreatedAccountingId { get; set; } = TimeAccountingId.Empty;
    private static DateTimeOffset CreatedAt { get; set; }

    [Test]
    public async Task CreateTimeAccountingType()
    {
        var client = await zammadStack.GetClientAsync();

        var type = await client.CreateTimeAccountingTypeAsync(
            new TimeAccountingType { Name = TypeName, Note = "Created by integration test" }
        );

        await Assert.That(type.Id).IsNotEqualTo(TimeAccountingTypeId.Empty);
        await Assert.That(type.Name).IsEqualTo(TypeName);
        await Assert.That(type.Active).IsTrue();

        CreatedTypeId = type.Id;
    }

    [Test]
    [DependsOn(nameof(CreateTimeAccountingType))]
    public async Task ListTimeAccountingTypes()
    {
        var client = await zammadStack.GetClientAsync();

        var types = await client.ListTimeAccountingTypesAsync();

        await Assert.That(types).Contains(t => t.Id == CreatedTypeId && t.Name == TypeName);
    }

    [Test]
    [DependsOn(nameof(CreateTimeAccountingType))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task CreateTicket()
    {
        var client = await zammadStack.GetClientAsync();

        var organization = await client.CreateOrganizationAsync(
            new Organization { Name = "Time Accounting Organization " + Id }
        );
        // Zammad replaces the ticket's organization with the customer's if the customer isn't a member
        var customer = await client.CreateUserAsync(
            new User
            {
                FirstName = "Time Accounting",
                LastName = "Customer " + Id,
                Email = $"time-accounting-{Id}@example.org",
                OrganizationId = organization.Id,
            }
        );
        var ticket = await client.CreateTicketAsync(
            new Ticket
            {
                Title = "Time Accounting Test Ticket " + Id,
                GroupId = new GroupId(1),
                CustomerId = customer.Id,
                OrganizationId = organization.Id,
            },
            new TicketArticle
            {
                Subject = "Initial Article " + Id,
                Body = "Initial article body " + Id,
                Type = "note",
            }
        );

        await Assert.That(ticket.OrganizationId).IsEqualTo(organization.Id);

        TestOrganizationId = organization.Id;
        TestCustomerId = customer.Id;
        TestTicketId = ticket.Id;
    }

    [Test]
    [DependsOn(nameof(CreateTicket))]
    public async Task CreateTimeAccounting()
    {
        var client = await zammadStack.GetClientAsync();

        var accounting = await client.CreateTimeAccountingAsync(
            new TicketAccounting
            {
                TicketId = TestTicketId,
                TimeUnit = 12.5m,
                TypeId = CreatedTypeId,
            }
        );

        await Assert.That(accounting.Id).IsNotEqualTo(TimeAccountingId.Empty);
        await Assert.That(accounting.TicketId).IsEqualTo(TestTicketId);
        await Assert.That(accounting.TimeUnit).IsEqualTo(12.5m);
        await Assert.That(accounting.TypeId).IsEqualTo(CreatedTypeId);
        await Assert.That(accounting.CreatedAt).IsNotNull();

        CreatedAccountingId = accounting.Id;
        CreatedAt = accounting.CreatedAt!.Value;
    }

    [Test]
    [DependsOn(nameof(CreateTimeAccounting))]
    public async Task ListTimeAccountings()
    {
        var client = await zammadStack.GetClientAsync();

        // ordered by ID, so the newest entries are on the last page
        var page = new Pagination { Page = 1, PerPage = 100 };
        var accountings = await client.ListTimeAccountingsAsync(page);
        var found = accountings.Any(a => a.Id == CreatedAccountingId);
        while (!found && accountings.Count == page.PerPage)
        {
            page = page.Next();
            accountings = await client.ListTimeAccountingsAsync(page);
            found = accountings.Any(a => a.Id == CreatedAccountingId);
        }

        await Assert.That(found).IsTrue();
    }

    [Test]
    [DependsOn(nameof(CreateTimeAccounting))]
    public async Task GetTimeAccounting()
    {
        var client = await zammadStack.GetClientAsync();

        var accounting = await client.GetTimeAccountingAsync(CreatedAccountingId);

        await Assert.That(accounting).IsNotNull();
        await Assert.That(accounting!.TicketId).IsEqualTo(TestTicketId);
        await Assert.That(accounting.TimeUnit).IsEqualTo(12.5m);
    }

    [Test]
    [DependsOn(nameof(CreateTimeAccounting))]
    public async Task GetTicket_SumsUpTimeUnits()
    {
        var client = await zammadStack.GetClientAsync();

        var ticket = await client.GetTicketAsync(TestTicketId);

        await Assert.That(ticket!.TimeUnit).IsEqualTo(12.5m);
    }

    [Test]
    [DependsOn(nameof(CreateTimeAccounting))]
    public async Task GetTimeAccountingByActivity()
    {
        var client = await zammadStack.GetClientAsync();

        var rows = await client.GetTimeAccountingByActivityAsync(CreatedAt.Year, CreatedAt.Month);

        var row = rows.SingleOrDefault(r => r.Ticket?.Id == TestTicketId);
        await Assert.That(row).IsNotNull();
        await Assert.That(row!.TimeUnit).IsEqualTo(12.5m);
        await Assert.That(row.Customer).IsEqualTo("Time Accounting Customer " + Id);
        await Assert.That(row.Organization).IsEqualTo("Time Accounting Organization " + Id);
        await Assert.That(row.Agent).IsNotNullOrWhiteSpace();
        await Assert.That(row.CreatedAt).IsNotNull();
        // the type name is only included if the time_accounting_types setting is enabled
        await Assert.That(row.Type).IsNull();
    }

    [Test]
    [DependsOn(nameof(CreateTimeAccounting))]
    public async Task GetTimeAccountingByActivity_Limit()
    {
        var client = await zammadStack.GetClientAsync();

        var rows = await client.GetTimeAccountingByActivityAsync(CreatedAt.Year, CreatedAt.Month, limit: 1);

        await Assert.That(rows).HasSingleItem();
    }

    [Test]
    [DependsOn(nameof(CreateTimeAccounting))]
    public async Task GetTimeAccountingByTicket()
    {
        var client = await zammadStack.GetClientAsync();

        var rows = await client.GetTimeAccountingByTicketAsync(CreatedAt.Year, CreatedAt.Month);

        var row = rows.SingleOrDefault(r => r.Ticket?.Id == TestTicketId);
        await Assert.That(row).IsNotNull();
        await Assert.That(row!.TimeUnit).IsEqualTo(12.5m);
        await Assert.That(row.Ticket!.Title).IsEqualTo("Time Accounting Test Ticket " + Id);
    }

    [Test]
    [DependsOn(nameof(CreateTimeAccounting))]
    public async Task GetTimeAccountingByCustomer()
    {
        var client = await zammadStack.GetClientAsync();

        var rows = await client.GetTimeAccountingByCustomerAsync(CreatedAt.Year, CreatedAt.Month);

        var row = rows.SingleOrDefault(r => r.Customer?.Id == TestCustomerId);
        await Assert.That(row).IsNotNull();
        await Assert.That(row!.Organization?.Id).IsEqualTo(TestOrganizationId);
        await Assert.That(row.TimeUnit).IsEqualTo(12.5m);
    }

    [Test]
    [DependsOn(nameof(CreateTimeAccounting))]
    public async Task GetTimeAccountingByOrganization()
    {
        var client = await zammadStack.GetClientAsync();

        var rows = await client.GetTimeAccountingByOrganizationAsync(CreatedAt.Year, CreatedAt.Month);

        var row = rows.SingleOrDefault(r => r.Organization?.Id == TestOrganizationId);
        await Assert.That(row).IsNotNull();
        await Assert.That(row!.TimeUnit).IsEqualTo(12.5m);
    }

    [Test]
    [DependsOn(nameof(CreateTimeAccounting))]
    [Arguments(TimeAccountingReport.ByActivity)]
    [Arguments(TimeAccountingReport.ByTicket)]
    [Arguments(TimeAccountingReport.ByCustomer)]
    [Arguments(TimeAccountingReport.ByOrganization)]
    public async Task DownloadTimeAccountingReport(TimeAccountingReport report)
    {
        var client = await zammadStack.GetClientAsync();

        await using var stream = await client.DownloadTimeAccountingReportAsync(
            report,
            CreatedAt.Year,
            CreatedAt.Month,
            "Europe/Berlin"
        );
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        var bytes = memory.ToArray();

        // .xlsx files are zip archives
        await Assert.That(bytes.Length).IsGreaterThan(4);
        await Assert.That(bytes.Take(2).ToArray()).IsEquivalentTo("PK"u8.ToArray());
    }

    [Test]
    [DependsOn(nameof(GetTimeAccounting))]
    [DependsOn(nameof(ListTimeAccountings))]
    [DependsOn(nameof(GetTicket_SumsUpTimeUnits))]
    [DependsOn(nameof(GetTimeAccountingByActivity))]
    [DependsOn(nameof(GetTimeAccountingByActivity_Limit))]
    [DependsOn(nameof(GetTimeAccountingByTicket))]
    [DependsOn(nameof(GetTimeAccountingByCustomer))]
    [DependsOn(nameof(GetTimeAccountingByOrganization))]
    [DependsOn(nameof(DownloadTimeAccountingReport), [typeof(TimeAccountingReport)])]
    public async Task UpdateTimeAccounting()
    {
        var client = await zammadStack.GetClientAsync();

        var updated = await client.UpdateTimeAccountingAsync(
            CreatedAccountingId,
            new TicketAccounting { TimeUnit = 7.25m }
        );

        await Assert.That(updated.Id).IsEqualTo(CreatedAccountingId);
        await Assert.That(updated.TimeUnit).IsEqualTo(7.25m);
        await Assert.That(updated.TypeId).IsEqualTo(CreatedTypeId);
    }

    [Test]
    [DependsOn(nameof(UpdateTimeAccounting))]
    public async Task DeleteTimeAccounting()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteTimeAccountingAsync(CreatedAccountingId);

        await Assert.That(await client.GetTimeAccountingAsync(CreatedAccountingId)).IsNull();
        var ticket = await client.GetTicketAsync(TestTicketId);
        await Assert.That(ticket!.TimeUnit ?? 0m).IsEqualTo(0m);
    }

    [Test]
    [DependsOn(nameof(ListTimeAccountingTypes))]
    [DependsOn(nameof(DeleteTimeAccounting))]
    public async Task UpdateTimeAccountingType()
    {
        var client = await zammadStack.GetClientAsync();

        var updated = await client.UpdateTimeAccountingTypeAsync(
            CreatedTypeId,
            new TimeAccountingType
            {
                Name = TypeName,
                Note = "Updated by integration test",
                Active = false,
            }
        );

        await Assert.That(updated.Id).IsEqualTo(CreatedTypeId);
        await Assert.That(updated.Note).IsEqualTo("Updated by integration test");
        await Assert.That(updated.Active).IsFalse();
    }
}
