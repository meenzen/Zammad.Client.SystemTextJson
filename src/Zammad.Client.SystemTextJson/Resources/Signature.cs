using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

/// <summary>
/// An email signature. Groups reference it through <see cref="Group.SignatureId"/>.
/// </summary>
public sealed class Signature
{
    [JsonPropertyName("id")]
    public SignatureId Id { get; set; }

    /// <summary>
    /// Unique, at most 100 characters.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// The signature as HTML. It may contain placeholders such as <c>#{user.firstname}</c>.
    /// </summary>
    [JsonPropertyName("body")]
    public string? Body { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    /// <summary>
    /// At most 250 characters.
    /// </summary>
    [JsonPropertyName("note")]
    public string? Note { get; set; }

    /// <summary>
    /// The groups that use this signature, i.e. whose <see cref="Group.SignatureId"/> points at it.
    /// </summary>
    [JsonPropertyName("group_ids")]
    public List<GroupId>? GroupIds { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Group names, only set in expanded responses.
    /// </summary>
    [JsonPropertyName("groups")]
    public List<string>? Groups { get; set; }

    [JsonPropertyName("created_by")]
    public string? CreatedBy { get; set; }

    [JsonPropertyName("updated_by")]
    public string? UpdatedBy { get; set; }
}
