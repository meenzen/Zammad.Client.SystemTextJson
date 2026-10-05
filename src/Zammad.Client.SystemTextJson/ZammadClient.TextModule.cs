using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface ITextModuleService
{
    /// <summary>
    /// Lists text modules.
    /// </summary>
    /// <remarks>
    /// Admins (<c>admin.text_module</c>) get all text modules. Agents only get the active text modules that have
    /// no groups or at least one group they have <c>change</c> or <c>create</c> access to.
    /// </remarks>
    Task<List<TextModule>> ListTextModulesAsync(Pagination? pagination = null);

    /// <summary>
    /// Searches text modules. Needs the <c>admin.text_module</c> permission, agents can't search text modules.
    /// </summary>
    /// <remarks>
    /// The search goes through Elasticsearch, so new or changed text modules show up with a short delay.
    /// </remarks>
    Task<List<TextModule>> SearchTextModulesAsync(SearchQuery query, bool expand = true);

    /// <summary>
    /// Returns the text module, or <see langword="null"/> if it doesn't exist.
    /// </summary>
    /// <remarks>
    /// Agents get a 403 <see cref="ZammadException"/> for text modules that are limited to groups they have no
    /// <c>read</c> access to.
    /// </remarks>
    Task<TextModule?> GetTextModuleAsync(TextModuleId id);

    /// <summary>
    /// Creates a text module. Needs the <c>admin.text_module</c> permission.
    /// </summary>
    /// <remarks>
    /// <see cref="TextModule.Content"/> is HTML. If it contains no characters that HTML would escape
    /// (<c>&lt;</c>, <c>&gt;</c>, <c>&amp;</c>, quotes), Zammad treats it as plain text and replaces line breaks
    /// with <c>&lt;br&gt;</c>, so <c>"a\nb"</c> comes back as <c>"a&lt;br&gt;b"</c>.
    /// </remarks>
    Task<TextModule> CreateTextModuleAsync(TextModule textModule);

    /// <summary>
    /// Updates a text module. Needs the <c>admin.text_module</c> permission.
    /// </summary>
    Task<TextModule> UpdateTextModuleAsync(TextModuleId id, TextModule textModule);

    /// <summary>
    /// Deletes a text module. Needs the <c>admin.text_module</c> permission.
    /// </summary>
    Task DeleteTextModuleAsync(TextModuleId id);
}

public sealed partial class ZammadClient : ITextModuleService
{
    private const string TextModulesEndpoint = "/api/v1/text_modules";
    private const string TextModulesSearchEndpoint = "/api/v1/text_modules/search";

    public async Task<List<TextModule>> ListTextModulesAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<TextModule>>(TextModulesEndpoint, builder.ToString()) ?? [];
    }

    public async Task<List<TextModule>> SearchTextModulesAsync(SearchQuery query, bool expand = true)
    {
        var builder = new QueryBuilder();
        builder.AddSearchQuery(query);
        builder.Add("expand", expand);
        return await GetAsync<List<TextModule>>(TextModulesSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<TextModule?> GetTextModuleAsync(TextModuleId id) =>
        await GetAsync<TextModule>($"{TextModulesEndpoint}/{id}");

    public async Task<TextModule> CreateTextModuleAsync(TextModule textModule) =>
        await PostAsync<TextModule>(TextModulesEndpoint, textModule) ?? throw LogicException.UnexpectedNullResult;

    public async Task<TextModule> UpdateTextModuleAsync(TextModuleId id, TextModule textModule) =>
        await PutAsync<TextModule>($"{TextModulesEndpoint}/{id}", textModule)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteTextModuleAsync(TextModuleId id) => await DeleteAsync<bool>($"{TextModulesEndpoint}/{id}");
}
