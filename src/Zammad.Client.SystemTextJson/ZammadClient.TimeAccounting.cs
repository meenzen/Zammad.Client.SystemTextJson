using System.Globalization;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

/// <summary>
/// Time accounting across all tickets: entries, activity types and the monthly reports.
/// </summary>
/// <remarks>
/// Requires the <c>admin.time_accounting</c> permission. For the entries of one ticket, which agents can also access,
/// see <see cref="ITicketAccountingService"/>. Admins can create entries even if the <c>time_accounting</c> setting is
/// disabled.
/// </remarks>
public interface ITimeAccountingService
{
    /// <summary>
    /// Lists the time accounting entries of all tickets, ordered by ID.
    /// </summary>
    Task<List<TicketAccounting>> ListTimeAccountingsAsync(Pagination? pagination = null);

    Task<TicketAccounting?> GetTimeAccountingAsync(TimeAccountingId id);

    /// <summary>
    /// Creates a time accounting entry. <see cref="TicketAccounting.TicketId"/> is required.
    /// </summary>
    /// <remarks>
    /// Zammad adds the entry's <see cref="TicketAccounting.TimeUnit"/> to the ticket's <see cref="Ticket.TimeUnit"/>.
    /// An article can only have one entry, and it has to belong to the ticket.
    /// </remarks>
    Task<TicketAccounting> CreateTimeAccountingAsync(TicketAccounting accounting);

    Task<TicketAccounting> UpdateTimeAccountingAsync(TimeAccountingId id, TicketAccounting accounting);
    Task DeleteTimeAccountingAsync(TimeAccountingId id);

    /// <summary>
    /// Lists the activity types, including inactive ones.
    /// </summary>
    /// <remarks>
    /// Zammad has no endpoints to get or delete a single type. Deactivate types instead of deleting them.
    /// </remarks>
    Task<List<TimeAccountingType>> ListTimeAccountingTypesAsync(Pagination? pagination = null);

    Task<TimeAccountingType> CreateTimeAccountingTypeAsync(TimeAccountingType type);
    Task<TimeAccountingType> UpdateTimeAccountingTypeAsync(TimeAccountingTypeId id, TimeAccountingType type);

    /// <summary>
    /// Lists the time accounting entries created in a month, with ticket, customer and agent.
    /// </summary>
    /// <remarks>
    /// The month is in Zammad's default time zone (<c>timezone_default</c> setting).
    /// </remarks>
    /// <param name="year">The year, e.g. 2026.</param>
    /// <param name="month">The month, 1 to 12.</param>
    /// <param name="limit">If set, only the last <paramref name="limit"/> rows are returned.</param>
    Task<List<TimeAccountingActivityRow>> GetTimeAccountingByActivityAsync(int year, int month, int? limit = null);

    /// <summary>
    /// Sums up the time accounting entries created in a month per ticket.
    /// </summary>
    /// <inheritdoc cref="GetTimeAccountingByActivityAsync" path="/remarks"/>
    /// <inheritdoc cref="GetTimeAccountingByActivityAsync" path="/param"/>
    Task<List<TimeAccountingTicketRow>> GetTimeAccountingByTicketAsync(int year, int month, int? limit = null);

    /// <summary>
    /// Sums up the time accounting entries created in a month per customer and organization of the tickets.
    /// </summary>
    /// <inheritdoc cref="GetTimeAccountingByActivityAsync" path="/remarks"/>
    /// <inheritdoc cref="GetTimeAccountingByActivityAsync" path="/param"/>
    Task<List<TimeAccountingCustomerRow>> GetTimeAccountingByCustomerAsync(int year, int month, int? limit = null);

    /// <summary>
    /// Sums up the time accounting entries created in a month per organization of the tickets. Tickets without
    /// organization are left out.
    /// </summary>
    /// <inheritdoc cref="GetTimeAccountingByActivityAsync" path="/remarks"/>
    /// <inheritdoc cref="GetTimeAccountingByActivityAsync" path="/param"/>
    Task<List<TimeAccountingOrganizationRow>> GetTimeAccountingByOrganizationAsync(
        int year,
        int month,
        int? limit = null
    );

    /// <summary>
    /// Downloads a report as an Excel file (<c>.xlsx</c>).
    /// </summary>
    /// <remarks>
    /// The file contains all rows. Unlike the JSON report, <see cref="TimeAccountingReport.ByTicket"/> has one column
    /// per ticket attribute.
    /// </remarks>
    /// <param name="report">The report.</param>
    /// <param name="year">The year, e.g. 2026.</param>
    /// <param name="month">The month, 1 to 12, in Zammad's default time zone.</param>
    /// <param name="timezone">
    /// The IANA time zone for the dates in the file, e.g. <c>Europe/Berlin</c>. Zammad uses its default time zone if not set.
    /// </param>
    Task<Stream> DownloadTimeAccountingReportAsync(
        TimeAccountingReport report,
        int year,
        int month,
        string? timezone = null
    );
}

public sealed partial class ZammadClient : ITimeAccountingService
{
    private const string TimeAccountingsEndpoint = "/api/v1/time_accountings";
    private const string TimeAccountingTypesEndpoint = "/api/v1/time_accounting/types";
    private const string TimeAccountingLogEndpoint = "/api/v1/time_accounting/log";

    public async Task<List<TicketAccounting>> ListTimeAccountingsAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<TicketAccounting>>(TimeAccountingsEndpoint, builder.ToString()) ?? [];
    }

    public async Task<TicketAccounting?> GetTimeAccountingAsync(TimeAccountingId id) =>
        await GetAsync<TicketAccounting>($"{TimeAccountingsEndpoint}/{id}");

    public async Task<TicketAccounting> CreateTimeAccountingAsync(TicketAccounting accounting) =>
        await PostAsync<TicketAccounting>(TimeAccountingsEndpoint, accounting)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<TicketAccounting> UpdateTimeAccountingAsync(TimeAccountingId id, TicketAccounting accounting) =>
        await PutAsync<TicketAccounting>($"{TimeAccountingsEndpoint}/{id}", accounting)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteTimeAccountingAsync(TimeAccountingId id) =>
        await DeleteAsync<bool>($"{TimeAccountingsEndpoint}/{id}");

    public async Task<List<TimeAccountingType>> ListTimeAccountingTypesAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<TimeAccountingType>>(TimeAccountingTypesEndpoint, builder.ToString()) ?? [];
    }

    public async Task<TimeAccountingType> CreateTimeAccountingTypeAsync(TimeAccountingType type) =>
        await PostAsync<TimeAccountingType>(TimeAccountingTypesEndpoint, type)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<TimeAccountingType> UpdateTimeAccountingTypeAsync(
        TimeAccountingTypeId id,
        TimeAccountingType type
    ) =>
        await PutAsync<TimeAccountingType>($"{TimeAccountingTypesEndpoint}/{id}", type)
        ?? throw LogicException.UnexpectedNullResult;

    public Task<List<TimeAccountingActivityRow>> GetTimeAccountingByActivityAsync(
        int year,
        int month,
        int? limit = null
    ) => GetTimeAccountingReportAsync<TimeAccountingActivityRow>(TimeAccountingReport.ByActivity, year, month, limit);

    public Task<List<TimeAccountingTicketRow>> GetTimeAccountingByTicketAsync(int year, int month, int? limit = null) =>
        GetTimeAccountingReportAsync<TimeAccountingTicketRow>(TimeAccountingReport.ByTicket, year, month, limit);

    public Task<List<TimeAccountingCustomerRow>> GetTimeAccountingByCustomerAsync(
        int year,
        int month,
        int? limit = null
    ) => GetTimeAccountingReportAsync<TimeAccountingCustomerRow>(TimeAccountingReport.ByCustomer, year, month, limit);

    public Task<List<TimeAccountingOrganizationRow>> GetTimeAccountingByOrganizationAsync(
        int year,
        int month,
        int? limit = null
    ) =>
        GetTimeAccountingReportAsync<TimeAccountingOrganizationRow>(
            TimeAccountingReport.ByOrganization,
            year,
            month,
            limit
        );

    public async Task<Stream> DownloadTimeAccountingReportAsync(
        TimeAccountingReport report,
        int year,
        int month,
        string? timezone = null
    )
    {
        var builder = new QueryBuilder();
        builder.Add("download", true);
        if (timezone is not null)
        {
            builder.Add("timezone", timezone);
        }

        return await GetAsync<Stream>(GetTimeAccountingReportPath(report, year, month), builder.ToString())
            ?? throw LogicException.UnexpectedNullResult;
    }

    private async Task<List<TRow>> GetTimeAccountingReportAsync<TRow>(
        TimeAccountingReport report,
        int year,
        int month,
        int? limit
    )
    {
        var builder = new QueryBuilder();
        if (limit is { } l)
        {
            builder.Add("limit", l);
        }

        return await GetAsync<List<TRow>>(GetTimeAccountingReportPath(report, year, month), builder.ToString()) ?? [];
    }

    private static string GetTimeAccountingReportPath(TimeAccountingReport report, int year, int month)
    {
        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), month, "The month must be between 1 and 12.");
        }

        var name = report switch
        {
            TimeAccountingReport.ByActivity => "by_activity",
            TimeAccountingReport.ByTicket => "by_ticket",
            TimeAccountingReport.ByCustomer => "by_customer",
            TimeAccountingReport.ByOrganization => "by_organization",
            _ => throw new ArgumentOutOfRangeException(nameof(report), report, "Unknown report."),
        };

        var y = year.ToString(CultureInfo.InvariantCulture);
        var m = month.ToString(CultureInfo.InvariantCulture);
        return $"{TimeAccountingLogEndpoint}/{name}/{y}/{m}";
    }
}
