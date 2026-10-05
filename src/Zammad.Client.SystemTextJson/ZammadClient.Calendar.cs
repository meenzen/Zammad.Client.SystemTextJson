using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

/// <summary>
/// Business hour calendars (<c>/api/v1/calendars</c>), used by SLAs. Requires the <c>admin.calendar</c> permission.
/// </summary>
public interface ICalendarService
{
    Task<List<Calendar>> ListCalendarsAsync(Pagination? pagination = null);

    Task<Calendar?> GetCalendarAsync(CalendarId id);

    /// <summary>
    /// Creates a calendar.
    /// </summary>
    /// <remarks>
    /// <see cref="Calendar.BusinessHours"/> needs at least one active day with a timeframe, otherwise Zammad responds
    /// with 422 "There are no business hours configured.". Zammad downloads <see cref="Calendar.IcalUrl"/> while
    /// saving, so it must be reachable from the Zammad server. If the download fails, the calendar is still saved and
    /// the error is in <see cref="Calendar.LastLog"/>. Only one calendar can be the <see cref="Calendar.Default"/>:
    /// setting it on one calendar unsets it on all others.
    /// </remarks>
    Task<Calendar> CreateCalendarAsync(Calendar calendar);

    Task<Calendar> UpdateCalendarAsync(CalendarId id, Calendar calendar);

    /// <summary>
    /// Deletes a calendar.
    /// </summary>
    /// <remarks>
    /// Fails with 422 "Can't delete, object has references." while an SLA uses the calendar.
    /// </remarks>
    Task DeleteCalendarAsync(CalendarId id);

    /// <summary>
    /// Returns all time zones Zammad knows, with their current UTC offset in whole hours (rounded down, e.g. -4 for UTC-03:30).
    /// </summary>
    /// <remarks>
    /// Also allowed with the <c>admin.trigger</c> or <c>admin.scheduler</c> permission.
    /// </remarks>
    Task<Dictionary<string, int>> ListCalendarTimezonesAsync();
}

public sealed partial class ZammadClient : ICalendarService
{
    private const string CalendarsEndpoint = "/api/v1/calendars";

    public async Task<List<Calendar>> ListCalendarsAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<Calendar>>(CalendarsEndpoint, builder.ToString()) ?? [];
    }

    public async Task<Calendar?> GetCalendarAsync(CalendarId id) =>
        await GetAsync<Calendar>($"{CalendarsEndpoint}/{id}");

    public async Task<Calendar> CreateCalendarAsync(Calendar calendar) =>
        await PostAsync<Calendar>(CalendarsEndpoint, calendar) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Calendar> UpdateCalendarAsync(CalendarId id, Calendar calendar) =>
        await PutAsync<Calendar>($"{CalendarsEndpoint}/{id}", calendar) ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteCalendarAsync(CalendarId id) => await DeleteAsync<bool>($"{CalendarsEndpoint}/{id}");

    public async Task<Dictionary<string, int>> ListCalendarTimezonesAsync()
    {
        var result =
            await GetAsync<CalendarTimezones>($"{CalendarsEndpoint}/timezones")
            ?? throw LogicException.UnexpectedNullResult;
        return result.Timezones ?? [];
    }
}
