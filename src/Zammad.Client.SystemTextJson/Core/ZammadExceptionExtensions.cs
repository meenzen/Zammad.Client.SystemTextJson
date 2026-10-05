using System.Text.Json;
using Zammad.Client.Resources;

namespace Zammad.Client.Core;

/// <summary>
/// Reads endpoint specific details from the response body of a <see cref="ZammadException"/>.
/// </summary>
public static class ZammadExceptionExtensions
{
    /// <summary>
    /// The ticket that made a mass update or mass macro fail (<c>{"error": true, "ticket_id": ...}</c>), or
    /// <c>null</c> if the response doesn't name one.
    /// </summary>
    /// <remarks>
    /// Zammad returns this with 422 when the current user lacks change permission on a ticket, or when saving a
    /// record fails validation. In the latter case Zammad puts the ID of the record that failed into
    /// <c>ticket_id</c>, which is an article ID if the article of a mass update was invalid.
    /// </remarks>
    public static TicketId? GetFailedTicketId(this ZammadException exception) =>
        TryGetProperty(exception, "ticket_id", out var value)
        && value.ValueKind == JsonValueKind.Number
        && value.TryGetInt32(out var id)
            ? new TicketId(id)
            : null;

    /// <summary>
    /// The tickets outside the macro's groups that made a mass macro fail with 422 "Macro group restrictions do not
    /// cover all tickets". Empty if the response doesn't list any.
    /// </summary>
    public static List<TicketId> GetBlockingTicketIds(this ZammadException exception)
    {
        if (!TryGetProperty(exception, "blocking_tickets", out var value) || value.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var ids = new List<TicketId>();
        foreach (var element in value.EnumerateArray())
        {
            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt32(out var id))
            {
                ids.Add(new TicketId(id));
            }
        }

        return ids;
    }

    private static bool TryGetProperty(ZammadException exception, string name, out JsonElement value)
    {
        value = default;
        if (exception.Content is not { } json || string.IsNullOrWhiteSpace(json))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (
                document.RootElement.ValueKind != JsonValueKind.Object
                || !document.RootElement.TryGetProperty(name, out var property)
            )
            {
                return false;
            }

            value = property.Clone();
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}
