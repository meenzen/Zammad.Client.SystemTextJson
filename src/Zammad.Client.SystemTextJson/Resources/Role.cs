using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class Role
{
    [JsonPropertyName("id")]
    public RoleId Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("preferences")]
    public Dictionary<string, JsonElement>? Preferences { get; set; }

    /// <summary>
    /// Whether new users who sign up themselves get this role.
    /// </summary>
    /// <remarks>
    /// Zammad rejects this (422) for roles with permissions that don't allow signup, e.g. <c>ticket.agent</c>.
    /// </remarks>
    [JsonPropertyName("default_at_signup")]
    public bool? DefaultAtSignup { get; set; }

    /// <summary>
    /// Whether the role is active.
    /// </summary>
    /// <remarks>
    /// Roles can't be deleted, deactivate them instead. Zammad rejects deactivating the last role with admin
    /// permissions (422).
    /// </remarks>
    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("note")]
    public string? Note { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonPropertyName("permission_ids")]
    public List<PermissionId>? PermissionIds { get; set; }

    [JsonPropertyName("knowledge_base_permission_ids")]
    public List<KnowledgeBasePermissionId>? KnowledgeBasePermissionIds { get; set; }

    /// <summary>
    /// Group access of the role, mapped to the access levels (e.g. <c>full</c>, <c>read</c>).
    /// </summary>
    /// <remarks>
    /// Zammad removes all groups from roles without the <c>ticket.agent</c> permission. For inactive roles, this is
    /// always empty, even though Zammad keeps the group access.
    /// </remarks>
    [JsonPropertyName("group_ids")]
    public Dictionary<GroupId, List<string>>? GroupIds { get; set; }

    /// <summary>
    /// Permission names, e.g. <c>ticket.agent</c>. Only returned when expanded, but can also be sent instead of
    /// <see cref="PermissionIds"/> when creating or updating a role.
    /// </summary>
    /// <remarks>
    /// Zammad ignores this if <see cref="PermissionIds"/> is also set.
    /// </remarks>
    [JsonPropertyName("permissions")]
    public List<string>? Permissions { get; set; }

    [JsonPropertyName("knowledge_base_permissions")]
    public List<string>? KnowledgeBasePermissions { get; set; }

    /// <summary>
    /// Group access of the role by group name. Only returned when expanded.
    /// </summary>
    [JsonPropertyName("groups")]
    public Dictionary<string, List<string>>? Groups { get; set; }

    [JsonPropertyName("created_by")]
    public string? CreatedBy { get; set; }

    [JsonPropertyName("updated_by")]
    public string? UpdatedBy { get; set; }
}
