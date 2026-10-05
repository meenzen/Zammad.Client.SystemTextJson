using System.Text.Json;
using Zammad.Client.Core;

namespace Zammad.Client.Resources;

/// <summary>
/// Typed access to custom attributes (custom fields) configured in the Zammad object manager.
/// </summary>
/// <remarks>
/// Values are converted with the same JSON options the client uses for requests, so any type that
/// <see cref="JsonSerializer"/> can handle works, e.g. <c>string</c>, <c>int</c>, <c>bool</c>, <c>DateTimeOffset</c> or
/// <c>List&lt;string&gt;</c> for multi-select fields.
/// </remarks>
public static class CustomFieldExtensions
{
    /// <summary>
    /// Returns the value of a custom field, or <c>default</c> if it isn't present or is <c>null</c>.
    /// </summary>
    /// <exception cref="JsonException">The value can't be converted to <typeparamref name="T"/>.</exception>
    public static T? GetCustomField<T>(this IHasCustomFields resource, string name) =>
        resource.TryGetCustomField<T>(name, out var value) ? value : default;

    /// <summary>
    /// Gets the value of a custom field.
    /// </summary>
    /// <returns><c>true</c> if the field is present, even if its value is <c>null</c>.</returns>
    /// <exception cref="JsonException">The value can't be converted to <typeparamref name="T"/>.</exception>
    public static bool TryGetCustomField<T>(this IHasCustomFields resource, string name, out T? value)
    {
        if (resource.ExtensionData is not { } data || !data.TryGetValue(name, out var element))
        {
            value = default;
            return false;
        }

        value = element.Deserialize<T>(Serialization.Options);
        return true;
    }

    /// <summary>
    /// Returns <c>true</c> if the custom field is present, even if its value is <c>null</c>.
    /// </summary>
    public static bool HasCustomField(this IHasCustomFields resource, string name) =>
        resource.ExtensionData?.ContainsKey(name) == true;

    /// <summary>
    /// Sets the value of a custom field. Setting <c>null</c> sends an explicit <c>null</c>, which clears the field in
    /// Zammad.
    /// </summary>
    public static void SetCustomField<T>(this IHasCustomFields resource, string name, T value)
    {
        resource.ExtensionData ??= [];
        resource.ExtensionData[name] = JsonSerializer.SerializeToElement(value, Serialization.Options);
    }

    /// <summary>
    /// Sets the value of a custom field and returns the resource, so it can be used in expressions.
    /// </summary>
    /// <example>
    /// <code>
    /// var ticket = await client.CreateTicketAsync(
    ///     new Ticket { Title = "Printer is on fire" }.WithCustomField("product", "Printer 3000"),
    ///     article
    /// );
    /// </code>
    /// </example>
    public static TResource WithCustomField<TResource, TValue>(this TResource resource, string name, TValue value)
        where TResource : IHasCustomFields
    {
        resource.SetCustomField(name, value);
        return resource;
    }

    /// <summary>
    /// Sets a custom field to <c>null</c>, which clears its value in Zammad when the resource is sent.
    /// </summary>
    public static void ClearCustomField(this IHasCustomFields resource, string name) =>
        resource.SetCustomField<object?>(name, null);

    /// <summary>
    /// Removes a custom field, so it isn't sent to Zammad. This doesn't clear the field in Zammad, use
    /// <see cref="ClearCustomField"/> for that.
    /// </summary>
    /// <returns><c>true</c> if the field was present.</returns>
    public static bool RemoveCustomField(this IHasCustomFields resource, string name) =>
        resource.ExtensionData?.Remove(name) == true;

    /// <summary>
    /// Converts all custom fields to an instance of <typeparamref name="T"/>, mapping JSON names to properties the
    /// same way the client does for its own resources (use <c>[JsonPropertyName]</c> on the properties).
    /// </summary>
    /// <exception cref="JsonException">The values can't be converted to <typeparamref name="T"/>.</exception>
    public static T GetCustomFields<T>(this IHasCustomFields resource)
        where T : new()
    {
        if (resource.ExtensionData is not { Count: > 0 } data)
        {
            return new T();
        }

        var element = JsonSerializer.SerializeToElement(data, Serialization.Options);
        return element.Deserialize<T>(Serialization.Options) ?? new T();
    }

    /// <summary>
    /// Sets every property of <paramref name="fields"/> as a custom field. Properties that are <c>null</c> are skipped,
    /// other custom fields already present on the resource are kept.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="fields"/> doesn't serialize to a JSON object.</exception>
    public static void SetCustomFields<T>(this IHasCustomFields resource, T fields)
    {
        var element = JsonSerializer.SerializeToElement(fields, Serialization.Options);
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException(
                $"Custom fields must serialize to a JSON object, but got {element.ValueKind}.",
                nameof(fields)
            );
        }

        resource.ExtensionData ??= [];
        foreach (var property in element.EnumerateObject())
        {
            resource.ExtensionData[property.Name] = property.Value;
        }
    }
}
