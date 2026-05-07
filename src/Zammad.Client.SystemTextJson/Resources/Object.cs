using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

public sealed class Object
{
    [JsonPropertyName("id")]
    public ObjectId Id { get; set; }

    [JsonPropertyName("object_lookup_id")]
    public ObjectLookupId? ObjectLookupId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("display")]
    public string? Display { get; set; }

    [JsonPropertyName("data_type")]
    public string? DataType { get; set; }

    [JsonPropertyName("data_option")]
    public ObjectDataOption? DataOption { get; set; }

    [JsonPropertyName("data_option_new")]
    public ObjectDataOption? DataOptionNew { get; set; }

    [JsonPropertyName("editable")]
    public bool? Editable { get; set; }

    [JsonPropertyName("active")]
    public bool? Active { get; set; }

    [JsonPropertyName("screens")]
    public dynamic? Screens { get; set; }

    [JsonPropertyName("to_create")]
    public bool? ToCreate { get; set; }

    [JsonPropertyName("to_migrate")]
    public bool? ToMigrate { get; set; }

    [JsonPropertyName("to_delete")]
    public bool? ToDelete { get; set; }

    [JsonPropertyName("to_config")]
    public bool? ToConfig { get; set; }

    [JsonPropertyName("position")]
    public int? Position { get; set; }

    [JsonPropertyName("updated_by_id")]
    public UserId? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public UserId? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }

    [JsonPropertyName("object")]
    public ObjectType? ObjectType { get; set; }

    [JsonPropertyName("deletable")]
    public bool? Deletable { get; set; }

    [JsonPropertyName("not_deletable_reason")]
    public string? NotDeletableReason { get; set; }
}

public class ObjectDataOption
{
    [JsonPropertyName("default")]
    public JsonElement? Default { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("relation")]
    public string? Relation { get; set; }

    [JsonPropertyName("relation_condition")]
    public dynamic? RelationCondition { get; set; }

    [JsonPropertyName("autocapitalize")]
    public bool? Autocapitalize { get; set; }

    [JsonPropertyName("multiple")]
    public bool? Multiple { get; set; }

    [JsonPropertyName("guess")]
    public bool? Guess { get; set; }

    [JsonPropertyName("null")]
    public bool? Null { get; set; }

    [JsonPropertyName("limit")]
    public int? Limit { get; set; }

    [JsonPropertyName("placeholder")]
    public string? Placeholder { get; set; }

    [JsonPropertyName("minLengt")]
    public int? MinLengt { get; set; }

    [JsonPropertyName("maxlength")]
    public int? MaxLength { get; set; }

    [JsonPropertyName("translate")]
    public bool? Translate { get; set; }

    [JsonPropertyName("filter")]
    public List<JsonElement>? Filter { get; set; }

    [JsonPropertyName("only_shown_if_selectable")]
    public bool? OnlyShownIfSelectable { get; set; }

    [JsonPropertyName("item_class")]
    public string? ItemClass { get; set; }

    [JsonPropertyName("permission")]
    public List<string>? Permissions { get; set; }

    [JsonPropertyName("options")]
    public dynamic? Options { get; set; }

    [JsonPropertyName("nulloption")]
    public bool? NullOption { get; set; }

    [JsonPropertyName("future")]
    public bool? Future { get; set; }

    [JsonPropertyName("past")]
    public bool? Past { get; set; }

    [JsonPropertyName("diff")]
    public int? Diff { get; set; }

    [JsonPropertyName("linktemplate")]
    public string? LinkTemplate { get; set; }

    [JsonPropertyName("min")]
    public int? Min { get; set; }

    [JsonPropertyName("max")]
    public int? Max { get; set; }
}
