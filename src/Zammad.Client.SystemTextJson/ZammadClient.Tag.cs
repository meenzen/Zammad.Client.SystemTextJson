using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

public interface ITagService
{
    Task<List<string>> ListTagsAsync(ObjectType objectType, TargetObjectId objectId);
    Task<List<TagSearchResult>> SearchTagsAsync(string term);
    Task<List<TagSearchResult>> SearchTagsAsync(string term, int limit);
    Task<bool> AddTagAsync(ObjectType objectType, TargetObjectId objectId, string tagName);
    Task<bool> RemoveTagAsync(ObjectType objectType, TargetObjectId objectId, string tagName);
    Task<List<Tag>> ListTagsAdminAsync();
    Task CreateTagAdminAsync(string name);
    Task RenameTagAdminAsync(TagId id, string name);
    Task DeleteTagAdminAsync(TagId id);
}

public sealed partial class ZammadClient : ITagService
{
    private const string TagsEndpoint = "/api/v1/tags";
    private const string TagSearchEndpoint = "/api/v1/tag_search";
    private const string TagListEndpoint = "/api/v1/tag_list";

    public async Task<List<string>> ListTagsAsync(ObjectType objectType, TargetObjectId objectId)
    {
        var builder = new QueryBuilder();
        builder.Add("object", objectType.ToString());
        builder.Add("o_id", objectId.ToString());
        var tagList = await GetAsync<StringTagList>(TagsEndpoint, builder.ToString());
        return tagList?.Tags ?? [];
    }

    public async Task<List<TagSearchResult>> SearchTagsAsync(string term)
    {
        var builder = new QueryBuilder();
        builder.Add("term", term);
        return await GetAsync<List<TagSearchResult>>(TagSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<List<TagSearchResult>> SearchTagsAsync(string term, int limit)
    {
        var builder = new QueryBuilder();
        builder.Add("term", term);
        builder.Add("limit", limit);
        return await GetAsync<List<TagSearchResult>>(TagSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<bool> AddTagAsync(ObjectType objectType, TargetObjectId objectId, string tagName) =>
        await PostAsync<bool>(
            $"{TagsEndpoint}/add",
            new TagRequest
            {
                ObjectType = objectType,
                ObjectId = objectId,
                Item = tagName,
            }
        );

    public async Task<bool> RemoveTagAsync(ObjectType objectType, TargetObjectId objectId, string tagName) =>
        await DeleteAsync<bool>(
            $"{TagsEndpoint}/remove",
            new TagRequest
            {
                ObjectType = objectType,
                ObjectId = objectId,
                Item = tagName,
            }
        );

    public async Task<List<Tag>> ListTagsAdminAsync() => await GetAsync<List<Tag>>(TagListEndpoint) ?? [];

    public async Task CreateTagAdminAsync(string name) =>
        await PostAsync<object>(TagListEndpoint, new TagAdminRequest { Name = name });

    public async Task RenameTagAdminAsync(TagId id, string name) =>
        await PutAsync<object>($"{TagListEndpoint}/{id}", new TagAdminRequest { Name = name });

    public async Task DeleteTagAdminAsync(TagId id) => await DeleteAsync<object>($"{TagListEndpoint}/{id}");
}
