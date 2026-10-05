using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

/// <summary>
/// System settings. Requires an <c>admin.*</c> permission, and each setting can require a more specific one (see
/// <see cref="Setting.Preferences"/>, <c>permission</c>).
/// </summary>
/// <remarks>
/// Settings are global: changing one affects the whole Zammad instance. Zammad caches settings for up to 15 seconds
/// per process, so a change can take that long to have an effect everywhere.
/// </remarks>
public interface ISettingService
{
    /// <summary>
    /// Lists all settings the user is allowed to see. Values of sensitive settings are masked.
    /// </summary>
    Task<List<Setting>> ListSettingsAsync();

    /// <summary>
    /// Lists the settings of an area, e.g. <c>System::Branding</c>.
    /// </summary>
    /// <remarks>
    /// Zammad has a <c>/settings/area/:area</c> route, but it ignores the area and returns all settings, so this
    /// method filters the list on the client.
    /// </remarks>
    Task<List<Setting>> ListSettingsByAreaAsync(string area);

    /// <summary>
    /// Returns the setting.
    /// </summary>
    /// <remarks>
    /// Zammad only accepts the numeric ID here, not the name, see <see cref="GetSettingByNameAsync"/>.
    /// </remarks>
    /// <exception cref="ZammadException">
    /// 403 if the setting is protected or the user lacks its permission. 500 (not 404) if the setting doesn't exist,
    /// so this method doesn't return <c>null</c> for unknown IDs, as of Zammad 7.2.
    /// </exception>
    Task<Setting?> GetSettingAsync(SettingId id);

    /// <summary>
    /// Returns the setting with the given name (e.g. <c>product_name</c>), or <c>null</c> if it doesn't exist or the
    /// user isn't allowed to see it.
    /// </summary>
    /// <remarks>
    /// Zammad only looks up settings by ID, so this loads the list of all settings.
    /// </remarks>
    Task<Setting?> GetSettingByNameAsync(string name);

    /// <summary>
    /// Updates a setting. Only <see cref="Setting.StateCurrent"/>, <see cref="Setting.Title"/> and
    /// <see cref="Setting.Description"/> can be changed. Zammad ignores the other fields, and keeps the existing
    /// <see cref="Setting.Preferences"/> (it only adds new keys).
    /// </summary>
    /// <remarks>
    /// To only change the value, use <see cref="UpdateSettingValueAsync{T}"/>.
    /// </remarks>
    Task<Setting> UpdateSettingAsync(SettingId id, Setting setting);

    /// <summary>
    /// Sets the value of a setting (<c>state_current.value</c>).
    /// </summary>
    /// <param name="id">The setting.</param>
    /// <param name="value">
    /// The value, serialized as JSON, e.g. a <c>string</c>, <c>bool</c>, <c>int</c> or an object with
    /// <c>[JsonPropertyName]</c> properties. <c>null</c> sends an explicit <c>null</c>.
    /// </param>
    /// <exception cref="ZammadException">422 if Zammad rejects the value.</exception>
    Task<Setting> UpdateSettingValueAsync<T>(SettingId id, T value);

    /// <summary>
    /// Resets a setting to its default value (<see cref="Setting.StateInitial"/>).
    /// </summary>
    /// <remarks>
    /// The default is the value from Zammad's seeds, not one set by the auto wizard or the setup.
    /// </remarks>
    Task<Setting> ResetSettingAsync(SettingId id);
}

public sealed partial class ZammadClient : ISettingService
{
    private const string SettingsEndpoint = "/api/v1/settings";

    public async Task<List<Setting>> ListSettingsAsync() => await GetAsync<List<Setting>>(SettingsEndpoint) ?? [];

    public async Task<List<Setting>> ListSettingsByAreaAsync(string area) =>
        (await ListSettingsAsync()).FindAll(s => string.Equals(s.Area, area, StringComparison.Ordinal));

    public async Task<Setting?> GetSettingAsync(SettingId id) => await GetAsync<Setting>($"{SettingsEndpoint}/{id}");

    public async Task<Setting?> GetSettingByNameAsync(string name) =>
        (await ListSettingsAsync()).Find(s => string.Equals(s.Name, name, StringComparison.Ordinal));

    public async Task<Setting> UpdateSettingAsync(SettingId id, Setting setting) =>
        await PutAsync<Setting>($"{SettingsEndpoint}/{id}", setting) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Setting> UpdateSettingValueAsync<T>(SettingId id, T value) =>
        await PutAsync<Setting>($"{SettingsEndpoint}/{id}", new SettingValueRequest<T>(value))
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<Setting> ResetSettingAsync(SettingId id) =>
        await PostAsync<Setting>($"{SettingsEndpoint}/reset/{id}") ?? throw LogicException.UnexpectedNullResult;
}
