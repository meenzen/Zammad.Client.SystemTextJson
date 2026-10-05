using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

/// <summary>
/// A ticket overview (a saved ticket list in the agent UI).
/// </summary>
public sealed class Overview
{
    [JsonPropertyName("id")]
    public OverviewId Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The URL slug (<c>#ticket/view/&lt;link&gt;</c>). Unique, Zammad generates it from the name if empty.
    /// </summary>
    [JsonPropertyName("link")]
    public string? Link { get; set; }

    /// <summary>
    /// Sort position, overviews are listed by ascending prio.
    /// </summary>
    [JsonPropertyName("prio")]
    public int? Prio { get; set; }

    /// <summary>
    /// The ticket selector, keyed by attribute, e.g.
    /// <c>{"ticket.state_id": {"operator": "is", "value": ["1", "2"]}, "ticket.owner_id": {"operator": "is",
    /// "pre_condition": "current_user.id"}}</c>.
    /// </summary>
    /// <remarks>
    /// Build a value with <see cref="JsonSerializer.SerializeToElement{TValue}(TValue, JsonSerializerOptions?)"/>.
    /// </remarks>
    [JsonPropertyName("condition")]
    public JsonElement? Condition { get; set; }

    [JsonPropertyName("order")]
    public OverviewOrder? Order { get; set; }

    /// <summary>
    /// Ticket attribute to group by, e.g. <c>customer</c>, <c>priority</c> or <c>state</c>.
    /// </summary>
    [JsonPropertyName("group_by")]
    public string? GroupBy { get; set; }

    /// <summary>
    /// <c>ASC</c> or <c>DESC</c>.
    /// </summary>
    [JsonPropertyName("group_direction")]
    public string? GroupDirection { get; set; }

    /// <summary>
    /// Only show the overview to users whose organization is shared.
    /// </summary>
    [JsonPropertyName("organization_shared")]
    public bool? OrganizationShared { get; set; }

    /// <summary>
    /// Only show the overview to users who are the out-of-office replacement of another user.
    /// </summary>
    [JsonPropertyName("out_of_office")]
    public bool? OutOfOffice { get; set; }

    [JsonPropertyName("view")]
    public OverviewView? View { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    /// <summary>
    /// Roles that can use the overview. Required, at least one.
    /// </summary>
    [JsonPropertyName("role_ids")]
    public List<RoleId>? RoleIds { get; set; }

    /// <summary>
    /// If not empty, only these users (who must also have one of the roles) can use the overview.
    /// </summary>
    [JsonPropertyName("user_ids")]
    public List<UserId>? UserIds { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Role names, only set in expanded responses.
    /// </summary>
    [JsonPropertyName("roles")]
    public List<string>? Roles { get; set; }

    /// <summary>
    /// User logins, only set in expanded responses.
    /// </summary>
    [JsonPropertyName("users")]
    public List<string>? Users { get; set; }

    [JsonPropertyName("created_by")]
    public string? CreatedBy { get; set; }

    [JsonPropertyName("updated_by")]
    public string? UpdatedBy { get; set; }
}

public sealed class OverviewOrder
{
    /// <summary>
    /// Ticket attribute to sort by, e.g. <c>created_at</c>.
    /// </summary>
    [JsonPropertyName("by")]
    public string? By { get; set; }

    /// <summary>
    /// <c>ASC</c> or <c>DESC</c>.
    /// </summary>
    [JsonPropertyName("direction")]
    public string? Direction { get; set; }
}

/// <summary>
/// The columns of the overview.
/// </summary>
/// <remarks>
/// Zammad's UIs only read and write <see cref="Columns"/> (<c>s</c>). The seeded overviews also have <c>d</c>,
/// <c>m</c> and <c>view_mode_default</c>, which nothing in Zammad 7.2 uses anymore.
/// </remarks>
public sealed class OverviewView
{
    /// <summary>
    /// Ticket attributes shown as columns, e.g. <c>["number", "title", "customer", "state", "created_at"]</c>.
    /// </summary>
    [JsonPropertyName("s")]
    public List<string>? Columns { get; set; }

    [JsonPropertyName("d")]
    public List<string>? D { get; set; }

    [JsonPropertyName("m")]
    public List<string>? M { get; set; }

    [JsonPropertyName("view_mode_default")]
    public string? ViewModeDefault { get; set; }
}
