using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

/// <summary>
/// The time accounting reports, see <see cref="ITimeAccountingService.DownloadTimeAccountingReportAsync"/>.
/// </summary>
public enum TimeAccountingReport
{
    /// <summary>
    /// One row per time accounting entry.
    /// </summary>
    ByActivity,

    /// <summary>
    /// One row per ticket.
    /// </summary>
    ByTicket,

    /// <summary>
    /// One row per customer and organization.
    /// </summary>
    ByCustomer,

    /// <summary>
    /// One row per organization.
    /// </summary>
    ByOrganization,
}

/// <summary>
/// A time accounting entry, see <see cref="ITimeAccountingService.GetTimeAccountingByActivityAsync"/>.
/// </summary>
public sealed class TimeAccountingActivityRow
{
    /// <summary>
    /// The ticket's database columns, without the association fields (e.g. <see cref="Resources.Ticket.ArticleIds"/>).
    /// </summary>
    [JsonPropertyName("ticket")]
    public Ticket? Ticket { get; set; }

    [JsonPropertyName("time_unit")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? TimeUnit { get; set; }

    /// <summary>
    /// The name of the activity type, or <c>-</c> if the entry has none. Only set if the
    /// <c>time_accounting_types</c> setting is enabled.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// The full name of the ticket's customer, or <c>-</c>.
    /// </summary>
    [JsonPropertyName("customer")]
    public string? Customer { get; set; }

    /// <summary>
    /// The name of the ticket's organization, or <c>-</c> if it has none.
    /// </summary>
    [JsonPropertyName("organization")]
    public string? Organization { get; set; }

    /// <summary>
    /// The full name of the user who created the entry.
    /// </summary>
    [JsonPropertyName("agent")]
    public string? Agent { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }
}

/// <summary>
/// The time accounted on a ticket in the month, see <see cref="ITimeAccountingService.GetTimeAccountingByTicketAsync"/>.
/// </summary>
public sealed class TimeAccountingTicketRow
{
    /// <summary>
    /// The ticket's database columns, without the association fields (e.g. <see cref="Resources.Ticket.ArticleIds"/>).
    /// </summary>
    [JsonPropertyName("ticket")]
    public Ticket? Ticket { get; set; }

    /// <summary>
    /// The sum of the ticket's entries in the month.
    /// </summary>
    [JsonPropertyName("time_unit")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? TimeUnit { get; set; }

    /// <summary>
    /// The full name of the ticket's customer, or <c>-</c>.
    /// </summary>
    [JsonPropertyName("customer")]
    public string? Customer { get; set; }

    /// <summary>
    /// The name of the ticket's organization, or <c>-</c> if it has none.
    /// </summary>
    [JsonPropertyName("organization")]
    public string? Organization { get; set; }

    /// <summary>
    /// The full name of the user who created the ticket's first entry in the month.
    /// </summary>
    [JsonPropertyName("agent")]
    public string? Agent { get; set; }
}

/// <summary>
/// The time accounted on a customer's tickets of one organization in the month, see
/// <see cref="ITimeAccountingService.GetTimeAccountingByCustomerAsync"/>.
/// </summary>
public sealed class TimeAccountingCustomerRow
{
    /// <summary>
    /// The customer's database columns, without the association fields (e.g. <see cref="User.RoleIds"/>).
    /// </summary>
    /// <remarks>
    /// Unlike the users endpoints, Zammad doesn't filter the <c>password</c> column (the password hash) here. It ends
    /// up in <see cref="User.ExtensionData"/>.
    /// </remarks>
    [JsonPropertyName("customer")]
    public User? Customer { get; set; }

    /// <summary>
    /// The organization of the tickets, or <c>null</c> for the customer's tickets without organization.
    /// </summary>
    [JsonPropertyName("organization")]
    public Organization? Organization { get; set; }

    [JsonPropertyName("time_unit")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? TimeUnit { get; set; }
}

/// <summary>
/// The time accounted on an organization's tickets in the month, see
/// <see cref="ITimeAccountingService.GetTimeAccountingByOrganizationAsync"/>.
/// </summary>
public sealed class TimeAccountingOrganizationRow
{
    /// <summary>
    /// The organization's database columns, without the association fields (e.g. <see cref="Organization.MemberIds"/>).
    /// </summary>
    [JsonPropertyName("organization")]
    public Organization? Organization { get; set; }

    [JsonPropertyName("time_unit")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal? TimeUnit { get; set; }
}
