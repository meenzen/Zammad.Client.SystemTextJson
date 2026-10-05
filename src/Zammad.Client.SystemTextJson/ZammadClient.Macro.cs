using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface IMacroService
{
    /// <summary>
    /// Lists macros.
    /// </summary>
    /// <remarks>
    /// Admins (<c>admin.macro</c>) get all macros. Agents only get the active macros that have no groups or at
    /// least one group they have <c>change</c> or <c>create</c> access to.
    /// </remarks>
    Task<List<Macro>> ListMacrosAsync(Pagination? pagination = null);

    /// <summary>
    /// Searches macros. Needs the <c>admin.macro</c> permission, agents can't search macros.
    /// </summary>
    /// <remarks>
    /// The search goes through Elasticsearch, so new or changed macros show up with a short delay.
    /// </remarks>
    Task<List<Macro>> SearchMacrosAsync(SearchQuery query, bool expand = true);

    /// <summary>
    /// Returns the macro, or <see langword="null"/> if it doesn't exist or isn't visible to the user (see
    /// <see cref="ListMacrosAsync"/>).
    /// </summary>
    Task<Macro?> GetMacroAsync(MacroId id);

    /// <summary>
    /// Creates a macro. Needs the <c>admin.macro</c> permission.
    /// </summary>
    /// <remarks>
    /// Macro names must be unique (case-insensitive). Zammad validates <see cref="Macro.Perform"/>, for example a
    /// <c>ticket.tags</c> action needs a non-empty <c>value</c>.
    /// </remarks>
    Task<Macro> CreateMacroAsync(Macro macro);

    /// <summary>
    /// Updates a macro. Needs the <c>admin.macro</c> permission.
    /// </summary>
    Task<Macro> UpdateMacroAsync(MacroId id, Macro macro);

    /// <summary>
    /// Deletes a macro. Needs the <c>admin.macro</c> permission.
    /// </summary>
    Task DeleteMacroAsync(MacroId id);
}

public sealed partial class ZammadClient : IMacroService
{
    private const string MacrosEndpoint = "/api/v1/macros";
    private const string MacrosSearchEndpoint = "/api/v1/macros/search";

    public async Task<List<Macro>> ListMacrosAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<Macro>>(MacrosEndpoint, builder.ToString()) ?? [];
    }

    public async Task<List<Macro>> SearchMacrosAsync(SearchQuery query, bool expand = true)
    {
        var builder = new QueryBuilder();
        builder.AddSearchQuery(query);
        builder.Add("expand", expand);
        return await GetAsync<List<Macro>>(MacrosSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<Macro?> GetMacroAsync(MacroId id) => await GetAsync<Macro>($"{MacrosEndpoint}/{id}");

    public async Task<Macro> CreateMacroAsync(Macro macro) =>
        await PostAsync<Macro>(MacrosEndpoint, macro) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Macro> UpdateMacroAsync(MacroId id, Macro macro) =>
        await PutAsync<Macro>($"{MacrosEndpoint}/{id}", macro) ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteMacroAsync(MacroId id) => await DeleteAsync<bool>($"{MacrosEndpoint}/{id}");
}
