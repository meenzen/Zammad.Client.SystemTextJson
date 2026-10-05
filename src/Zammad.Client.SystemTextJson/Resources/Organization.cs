using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class Organization : IHasCustomFields
{
    [JsonPropertyName("id")]
    public OrganizationId Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("shared")]
    public required bool Shared { get; set; }

    [JsonPropertyName("domain")]
    public string Domain { get; set; } = string.Empty;

    [JsonPropertyName("domain_assignment")]
    public bool DomainAssignment { get; set; }

    [JsonPropertyName("active")]
    public required bool Active { get; set; }

    [JsonPropertyName("vip")]
    public bool Vip { get; set; }

    [JsonPropertyName("note")]
    public string Note { get; set; } = string.Empty;

    [JsonPropertyName("member_ids")]
    public List<UserId>? MemberIds { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonPropertyName("secondary_member_ids")]
    public List<UserId>? SecondaryMemberIds { get; set; }

    [JsonPropertyName("members")]
    public List<string>? Members { get; set; }

    [JsonPropertyName("created_by")]
    public string? CreatedBy { get; set; }

    [JsonPropertyName("updated_by")]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Additional properties that are not explicitly defined in this class.
    /// </summary>
    /// <remarks>
    /// This will contain custom attributes configured in the Zammad object manager.
    /// </remarks>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
