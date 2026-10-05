using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface ITemplateService
{
    /// <summary>
    /// Lists ticket templates.
    /// </summary>
    /// <remarks>
    /// Admins (<c>admin.template</c>) get all templates, agents only the active ones.
    /// </remarks>
    Task<List<Template>> ListTemplatesAsync(Pagination? pagination = null);

    /// <summary>
    /// Searches templates. Needs the <c>admin.template</c> permission, agents can't search templates.
    /// </summary>
    /// <remarks>
    /// The search goes through Elasticsearch, so new or changed templates show up with a short delay.
    /// </remarks>
    Task<List<Template>> SearchTemplatesAsync(SearchQuery query, bool expand = true);

    /// <summary>
    /// Returns the template, or <see langword="null"/> if it doesn't exist or isn't visible to the user (agents
    /// only see active templates).
    /// </summary>
    Task<Template?> GetTemplateAsync(TemplateId id);

    /// <summary>
    /// Creates a template. Needs the <c>admin.template</c> permission.
    /// </summary>
    Task<Template> CreateTemplateAsync(Template template);

    /// <summary>
    /// Updates a template. Needs the <c>admin.template</c> permission.
    /// </summary>
    Task<Template> UpdateTemplateAsync(TemplateId id, Template template);

    /// <summary>
    /// Deletes a template. Needs the <c>admin.template</c> permission.
    /// </summary>
    Task DeleteTemplateAsync(TemplateId id);
}

public sealed partial class ZammadClient : ITemplateService
{
    private const string TemplatesEndpoint = "/api/v1/templates";
    private const string TemplatesSearchEndpoint = "/api/v1/templates/search";

    public async Task<List<Template>> ListTemplatesAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<Template>>(TemplatesEndpoint, builder.ToString()) ?? [];
    }

    public async Task<List<Template>> SearchTemplatesAsync(SearchQuery query, bool expand = true)
    {
        var builder = new QueryBuilder();
        builder.AddSearchQuery(query);
        builder.Add("expand", expand);
        return await GetAsync<List<Template>>(TemplatesSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<Template?> GetTemplateAsync(TemplateId id) =>
        await GetAsync<Template>($"{TemplatesEndpoint}/{id}");

    public async Task<Template> CreateTemplateAsync(Template template) =>
        await PostAsync<Template>(TemplatesEndpoint, template) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Template> UpdateTemplateAsync(TemplateId id, Template template) =>
        await PutAsync<Template>($"{TemplatesEndpoint}/{id}", template) ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteTemplateAsync(TemplateId id) => await DeleteAsync<bool>($"{TemplatesEndpoint}/{id}");
}
