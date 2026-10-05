using System.Text.Json;

namespace Zammad.Client.Resources;

/// <summary>
/// A resource that can carry custom attributes configured in the Zammad object manager.
/// </summary>
/// <remarks>
/// Use the extension methods in <see cref="CustomFieldExtensions"/> to read and write custom fields without dealing with
/// <see cref="JsonElement"/> directly.
/// </remarks>
public interface IHasCustomFields
{
    /// <summary>
    /// Properties that are not explicitly defined on the resource, keyed by their JSON name.
    /// </summary>
    Dictionary<string, JsonElement>? ExtensionData { get; set; }
}
