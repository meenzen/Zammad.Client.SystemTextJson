using System.Net;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class CalendarTests(ZammadStackFixture zammadStack)
{
    private static readonly string RandomName = TestSetup.RandomString();
    private static readonly string CalendarName = "Test Calendar " + RandomName;
    private static CalendarId CreatedCalendarId { get; set; } = CalendarId.Empty;

    internal static Dictionary<string, CalendarBusinessDay> BusinessHours() =>
        new()
        {
            ["mon"] = new CalendarBusinessDay
            {
                Active = true,
                Timeframes =
                [
                    ["09:00", "17:00"],
                ],
            },
            ["tue"] = new CalendarBusinessDay
            {
                Active = true,
                Timeframes =
                [
                    ["09:00", "12:00"],
                    ["13:00", "17:00"],
                ],
            },
            ["sat"] = new CalendarBusinessDay
            {
                Active = false,
                Timeframes =
                [
                    ["09:00", "17:00"],
                ],
            },
        };

    [Test]
    public async Task CreateCalendar()
    {
        var client = await zammadStack.GetClientAsync();

        // No ical_url: the stack has no internet access
        var calendar = await client.CreateCalendarAsync(
            new Calendar
            {
                Name = CalendarName,
                Timezone = "Europe/Berlin",
                BusinessHours = BusinessHours(),
                PublicHolidays = new Dictionary<string, CalendarPublicHoliday>
                {
                    ["2030-12-25"] = new CalendarPublicHoliday { Active = true, Summary = "Christmas" },
                },
            }
        );

        await Assert.That(calendar.Id).IsNotEqualTo(CalendarId.Empty);
        await Assert.That(calendar.Name).IsEqualTo(CalendarName);
        await Assert.That(calendar.Timezone).IsEqualTo("Europe/Berlin");
        await Assert.That(calendar.BusinessHours!["tue"].Timeframes!.Count).IsEqualTo(2);
        await Assert.That(calendar.PublicHolidays!["2030-12-25"].Summary).IsEqualTo("Christmas");
        await Assert.That(calendar.IcalUrl).IsNull();

        CreatedCalendarId = calendar.Id;
    }

    [Test]
    public async Task CreateCalendar_ThrowsWithoutBusinessHours()
    {
        var client = await zammadStack.GetClientAsync();

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.CreateCalendarAsync(
                new Calendar { Name = "Invalid Calendar " + RandomName, Timezone = "Europe/Berlin" }
            )
        );

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(exception.Error).IsEqualTo("There are no business hours configured.");
    }

    [Test]
    [DependsOn(nameof(CreateCalendar))]
    public async Task ListCalendars()
    {
        var client = await zammadStack.GetClientAsync();

        var calendars = await client.ListCalendarsAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(calendars).Contains(c => c.Id == CreatedCalendarId);
        // Zammad always keeps one default calendar
        await Assert.That(calendars).Contains(c => c.Default == true);
    }

    [Test]
    [DependsOn(nameof(ListCalendars))]
    public async Task GetCalendar()
    {
        var client = await zammadStack.GetClientAsync();

        var calendar = await client.GetCalendarAsync(CreatedCalendarId);

        await Assert.That(calendar).IsNotNull();
        await Assert.That(calendar!.Name).IsEqualTo(CalendarName);
        await Assert.That(calendar.BusinessHours!["mon"].Active).IsTrue();
    }

    [Test]
    [DependsOn(nameof(GetCalendar))]
    public async Task UpdateCalendar()
    {
        var client = await zammadStack.GetClientAsync();

        var calendar = await client.GetCalendarAsync(CreatedCalendarId);
        await Assert.That(calendar).IsNotNull();
        calendar!.Timezone = "America/New_York";
        calendar.BusinessHours!["sat"].Active = true;

        var updated = await client.UpdateCalendarAsync(CreatedCalendarId, calendar);

        await Assert.That(updated.Timezone).IsEqualTo("America/New_York");
        await Assert.That(updated.BusinessHours!["sat"].Active).IsTrue();
    }

    [Test]
    [DependsOn(nameof(UpdateCalendar))]
    public async Task DeleteCalendar()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteCalendarAsync(CreatedCalendarId);

        await Assert.That(await client.GetCalendarAsync(CreatedCalendarId)).IsNull();
    }

    [Test]
    public async Task ListCalendarTimezones()
    {
        var client = await zammadStack.GetClientAsync();

        var timezones = await client.ListCalendarTimezonesAsync();

        await Assert.That(timezones["UTC"]).IsEqualTo(0);
        await Assert.That(timezones["Asia/Kolkata"]).IsEqualTo(5);
        await Assert.That(timezones).ContainsKey("Europe/Berlin");
    }
}
