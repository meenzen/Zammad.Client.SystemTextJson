using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Zammad.Client.Resources;

namespace Zammad.Client.Core;

/// <summary>
/// Reads a list of IDs that Zammad stores as strings (e.g. <c>"sorted_item_ids": ["1", "2"]</c>), and also accepts
/// numbers. Writes the IDs as strings.
/// </summary>
internal abstract class StringIdListConverter<TId> : JsonConverter<List<TId>>
    where TId : struct
{
    protected abstract TId Create(int value);

    protected abstract int GetValue(TId id);

    public override List<TId>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Expected an array of IDs, got {reader.TokenType}.");
        }

        var result = new List<TId>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            var value = reader.TokenType switch
            {
                JsonTokenType.Number => reader.GetInt32(),
                JsonTokenType.String
                    when int.TryParse(
                        reader.GetString(),
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out var parsed
                    ) => parsed,
                _ => throw new JsonException($"Expected an ID, got {reader.TokenType}."),
            };
            result.Add(Create(value));
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, List<TId> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var id in value)
        {
            writer.WriteStringValue(GetValue(id).ToString(CultureInfo.InvariantCulture));
        }

        writer.WriteEndArray();
    }
}

internal sealed class ChecklistItemIdListConverter : StringIdListConverter<ChecklistItemId>
{
    protected override ChecklistItemId Create(int value) => new(value);

    protected override int GetValue(ChecklistItemId id) => id.Value;
}

internal sealed class ChecklistTemplateItemIdListConverter : StringIdListConverter<ChecklistTemplateItemId>
{
    protected override ChecklistTemplateItemId Create(int value) => new(value);

    protected override int GetValue(ChecklistTemplateItemId id) => id.Value;
}
