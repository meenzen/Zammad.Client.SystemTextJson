using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class Calendar
{
    [JsonPropertyName("id")]
    public CalendarId Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// IANA time zone, e.g. <c>Europe/Berlin</c> (see <see cref="ICalendarService.ListCalendarTimezonesAsync"/>).
    /// </summary>
    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    /// <summary>
    /// Business hours per day (<c>mon</c> to <c>sun</c>).
    /// </summary>
    [JsonPropertyName("business_hours")]
    public Dictionary<string, CalendarBusinessDay>? BusinessHours { get; set; }

    /// <summary>
    /// Whether this is the default calendar.
    /// </summary>
    /// <remarks>
    /// Zammad always keeps one default calendar: if none is left, it makes the oldest calendar the default. That
    /// happens after the save, so the response to the create or update can still show the old value.
    /// </remarks>
    [JsonPropertyName("default")]
    public bool? Default { get; set; }

    /// <summary>
    /// URL of an iCalendar feed with public holidays, downloaded by Zammad on save and periodically by the scheduler.
    /// </summary>
    [JsonPropertyName("ical_url")]
    public string? IcalUrl { get; set; }

    /// <summary>
    /// Public holidays by date (<c>yyyy-MM-dd</c>).
    /// </summary>
    [JsonPropertyName("public_holidays")]
    public Dictionary<string, CalendarPublicHoliday>? PublicHolidays { get; set; }

    /// <summary>
    /// The error of the last <see cref="IcalUrl"/> download, if it failed.
    /// </summary>
    [JsonPropertyName("last_log")]
    public string? LastLog { get; set; }

    [JsonPropertyName("last_sync")]
    public DateTimeOffset? LastSync { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class CalendarBusinessDay
{
    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    /// <summary>
    /// Pairs of start and end times, e.g. <c>[["09:00", "12:00"], ["13:00", "17:00"]]</c>. Use <c>23:59</c> for the
    /// end of the day.
    /// </summary>
    [JsonPropertyName("timeframes")]
    public List<List<string>>? Timeframes { get; set; }
}

public sealed class CalendarPublicHoliday
{
    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("summary")]
    public string? Summary { get; set; }

    /// <summary>
    /// Hash of the <see cref="Calendar.IcalUrl"/> the holiday was imported from.
    /// </summary>
    [JsonPropertyName("feed")]
    public string? Feed { get; set; }

    /// <summary>
    /// Set when an imported holiday was removed, so the next import doesn't add it again.
    /// </summary>
    [JsonPropertyName("removed")]
    public bool? Removed { get; set; }
}
