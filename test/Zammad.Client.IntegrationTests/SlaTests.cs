using System.Net;
using System.Text.Json;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class SlaTests(ZammadStackFixture zammadStack)
{
    private static readonly string RandomName = TestSetup.RandomString();
    private static readonly string SlaName = "Test SLA " + RandomName;
    private static CalendarId CalendarId { get; set; } = CalendarId.Empty;
    private static SlaId CreatedSlaId { get; set; } = SlaId.Empty;

    // An SLA without a condition applies to all tickets. This one matches no ticket of other tests.
    private static JsonElement Condition =>
        JsonSerializer.Deserialize<JsonElement>(
            $$$"""{"ticket.title":{"operator":"contains","value":"{{{RandomName}}}"}}"""
        );

    [Test]
    public async Task CreateCalendar()
    {
        var client = await zammadStack.GetClientAsync();

        var calendar = await client.CreateCalendarAsync(
            new Calendar
            {
                Name = "SLA Calendar " + RandomName,
                Timezone = "Europe/Berlin",
                BusinessHours = CalendarTests.BusinessHours(),
            }
        );

        CalendarId = calendar.Id;
    }

    [Test]
    public async Task CreateSla_ThrowsWithoutCalendar()
    {
        var client = await zammadStack.GetClientAsync();

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.CreateSlaAsync(new Sla { Name = "Invalid SLA " + RandomName, Condition = Condition })
        );

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(exception.Error).Contains("calendar_id");
    }

    [Test]
    [DependsOn(nameof(CreateCalendar))]
    public async Task CreateSla_ThrowsWithResponseAndUpdateTime()
    {
        var client = await zammadStack.GetClientAsync();

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.CreateSlaAsync(
                new Sla
                {
                    Name = "Invalid SLA " + RandomName,
                    CalendarId = CalendarId,
                    Condition = Condition,
                    ResponseTime = 60,
                    UpdateTime = 120,
                }
            )
        );

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(exception.Error).IsEqualTo("Cannot have both response time and update time.");
    }

    [Test]
    [DependsOn(nameof(CreateCalendar))]
    public async Task CreateSla()
    {
        var client = await zammadStack.GetClientAsync();

        var sla = await client.CreateSlaAsync(
            new Sla
            {
                Name = SlaName,
                CalendarId = CalendarId,
                Condition = Condition,
                FirstResponseTime = 60,
                UpdateTime = 120,
                SolutionTime = 480,
            }
        );

        await Assert.That(sla.Id).IsNotEqualTo(SlaId.Empty);
        await Assert.That(sla.Name).IsEqualTo(SlaName);
        await Assert.That(sla.CalendarId).IsEqualTo(CalendarId);
        await Assert.That(sla.FirstResponseTime).IsEqualTo(60);
        await Assert.That(sla.UpdateTime).IsEqualTo(120);
        await Assert.That(sla.ResponseTime).IsNull();
        await Assert.That(sla.SolutionTime).IsEqualTo(480);

        CreatedSlaId = sla.Id;
    }

    [Test]
    [DependsOn(nameof(CreateSla))]
    public async Task ListSlas()
    {
        var client = await zammadStack.GetClientAsync();

        var slas = await client.ListSlasAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(slas).Contains(s => s.Id == CreatedSlaId);
    }

    [Test]
    [DependsOn(nameof(ListSlas))]
    public async Task GetSla()
    {
        var client = await zammadStack.GetClientAsync();

        var sla = await client.GetSlaAsync(CreatedSlaId);

        await Assert.That(sla).IsNotNull();
        await Assert.That(sla!.Name).IsEqualTo(SlaName);
        await Assert
            .That(sla.Condition!.Value.GetProperty("ticket.title").GetProperty("value").GetString())
            .IsEqualTo(RandomName);
    }

    [Test]
    [DependsOn(nameof(GetSla))]
    public async Task UpdateSla()
    {
        var client = await zammadStack.GetClientAsync();

        var sla = await client.GetSlaAsync(CreatedSlaId);
        await Assert.That(sla).IsNotNull();
        sla!.SolutionTime = 960;

        var updated = await client.UpdateSlaAsync(CreatedSlaId, sla);

        await Assert.That(updated.SolutionTime).IsEqualTo(960);
    }

    [Test]
    [DependsOn(nameof(UpdateSla))]
    public async Task DeleteCalendar_ThrowsWhileUsedBySla()
    {
        var client = await zammadStack.GetClientAsync();

        var exception = await Assert.ThrowsAsync<ZammadException>(() => client.DeleteCalendarAsync(CalendarId));

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(exception.Error).IsEqualTo("Can't delete, object has references.");
    }

    [Test]
    [DependsOn(nameof(DeleteCalendar_ThrowsWhileUsedBySla))]
    public async Task DeleteSla()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteSlaAsync(CreatedSlaId);

        await Assert.That(await client.GetSlaAsync(CreatedSlaId)).IsNull();
    }

    [Test]
    [DependsOn(nameof(DeleteSla))]
    [DependsOn(nameof(CreateSla_ThrowsWithResponseAndUpdateTime))]
    public async Task DeleteCalendar()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteCalendarAsync(CalendarId);

        await Assert.That(await client.GetCalendarAsync(CalendarId)).IsNull();
    }
}
