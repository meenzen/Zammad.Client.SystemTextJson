using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Zammad.Client.Resources;

#nullable enable
public class Object
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("object_lookup_id")]
    public int? ObjectLookupId { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("display")]
    public string? Display { get; set; }

    [JsonPropertyName("data_type")]
    public string? DataType { get; set; }

    [JsonPropertyName("data_option")]
    public ObjectDataOption? DataOption { get; set; }

    [JsonPropertyName("data_option_new")]
    public dynamic? DataOptionNew { get; set; }

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
    public int? UpdatedById { get; set; }

    [JsonPropertyName("created_by_id")]
    public int? CreatedById { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class ObjectDataOption
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("relation")]
    public string? Relation { get; set; }

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

    [JsonPropertyName("item_class")]
    public string? ItemClass { get; set; }

    [JsonPropertyName("permission")]
    public List<string>? Permissions { get; set; }
}
