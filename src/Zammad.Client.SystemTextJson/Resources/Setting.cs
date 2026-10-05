using System.Text.Json;
using System.Text.Json.Serialization;
using Zammad.Client.Core;

namespace Zammad.Client.Resources;

/// <summary>
/// A system setting.
/// </summary>
/// <remarks>
/// Zammad masks the values of sensitive settings (names containing <c>_password</c>, <c>_secret</c> or <c>_key</c>,
/// and keys ending in <c>_key</c>, <c>token</c>, <c>secret</c> or <c>bind_pw</c> within object values) as
/// <c>**********</c>. Sending the mask back in an update keeps the stored value. Protected settings and settings
/// that need a permission the user doesn't have aren't returned at all.
/// </remarks>
public sealed class Setting
{
    [JsonPropertyName("id")]
    public SettingId Id { get; set; }

    /// <summary>
    /// The unique name, e.g. <c>product_name</c>.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The group of settings in the admin interface, e.g. <c>System::Branding</c>.
    /// </summary>
    [JsonPropertyName("area")]
    public string? Area { get; set; }

    /// <summary>
    /// The form definition for the admin interface (<c>form</c>), and possible values.
    /// </summary>
    [JsonPropertyName("options")]
    public JsonElement? Options { get; set; }

    /// <summary>
    /// The current value, wrapped in an object: <c>{"value": ...}</c>. Use <see cref="SettingExtensions.GetValue{T}"/>
    /// to read it.
    /// </summary>
    [JsonPropertyName("state_current")]
    public JsonElement? StateCurrent { get; set; }

    /// <summary>
    /// The default value, wrapped in an object like <see cref="StateCurrent"/>. Resetting a setting restores it.
    /// </summary>
    [JsonPropertyName("state_initial")]
    public JsonElement? StateInitial { get; set; }

    /// <summary>
    /// Whether the setting is sent to the web app.
    /// </summary>
    [JsonPropertyName("frontend")]
    public bool? Frontend { get; set; }

    /// <summary>
    /// E.g. <c>permission</c> (the permissions needed to read and change the setting), <c>prio</c> (the order in the
    /// admin interface) or <c>protected</c>.
    /// </summary>
    [JsonPropertyName("preferences")]
    public Dictionary<string, JsonElement>? Preferences { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; set; }
}

public static class SettingExtensions
{
    /// <summary>
    /// Returns the current value of the setting (<c>state_current.value</c>), or <c>default</c> if it isn't present or
    /// is <c>null</c>.
    /// </summary>
    /// <exception cref="JsonException">The value can't be converted to <typeparamref name="T"/>.</exception>
    public static T? GetValue<T>(this Setting setting) => GetStateValue<T>(setting.StateCurrent);

    /// <summary>
    /// Returns the default value of the setting (<c>state_initial.value</c>), or <c>default</c> if it isn't present or
    /// is <c>null</c>.
    /// </summary>
    /// <exception cref="JsonException">The value can't be converted to <typeparamref name="T"/>.</exception>
    public static T? GetInitialValue<T>(this Setting setting) => GetStateValue<T>(setting.StateInitial);

    /// <summary>
    /// Sets the current value of the setting (<c>state_current.value</c>), e.g. before passing it to
    /// <see cref="ISettingService.UpdateSettingAsync"/>.
    /// </summary>
    public static void SetValue<T>(this Setting setting, T value) =>
        setting.StateCurrent = JsonSerializer.SerializeToElement(new SettingState<T>(value), Serialization.Options);

    private static T? GetStateValue<T>(JsonElement? state) =>
        state is { ValueKind: JsonValueKind.Object } s && s.TryGetProperty("value", out var value)
            ? value.Deserialize<T>(Serialization.Options)
            : default;
}

/// <summary>
/// The <c>{"value": ...}</c> wrapper of <see cref="Setting.StateCurrent"/>.
/// </summary>
internal sealed class SettingState<T>(T value)
{
    // An explicit null clears the value
    [JsonPropertyName("value")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public T Value { get; } = value;
}
