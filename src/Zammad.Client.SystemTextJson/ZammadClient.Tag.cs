using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

public interface ITagService
{
    Task<List<string>> ListTagsAsync(ObjectType type, TargetObjectId targetId);
    Task<List<TagSearchResult>> SearchTagsAsync(string term);
    Task<List<TagSearchResult>> SearchTagsAsync(string term, int limit);
    Task<bool> AddTagAsync(ObjectType type, TargetObjectId targetId, string tag);
    Task<bool> RemoveTagAsync(ObjectType type, TargetObjectId targetId, string tag);
    Task<List<Tag>> ListTagsAdminAsync();
    Task CreateTagAdminAsync(string tag);
    Task RenameTagAdminAsync(TagId id, string tag);
    Task DeleteTagAdminAsync(TagId id);
}

public sealed partial class ZammadClient : ITagService
{
    private const string TagsEndpoint = "/api/v1/tags";
    private const string TagSearchEndpoint = "/api/v1/tag_search";
    private const string TagListEndpoint = "/api/v1/tag_list";

    public async Task<List<string>> ListTagsAsync(ObjectType type, TargetObjectId targetId)
    {
        var builder = new QueryBuilder();
        builder.Add("object", type.ToString());
        builder.Add("o_id", targetId.ToString());
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

    public async Task<bool> AddTagAsync(ObjectType type, TargetObjectId targetId, string tag) =>
        await PostAsync<bool>(
            $"{TagsEndpoint}/add",
            new TagRequest
            {
                ObjectType = type,
                ObjectId = targetId,
                Item = tag,
            }
        );

    public async Task<bool> RemoveTagAsync(ObjectType type, TargetObjectId targetId, string tag) =>
        await DeleteAsync<bool>(
            $"{TagsEndpoint}/remove",
            new TagRequest
            {
                ObjectType = type,
                ObjectId = targetId,
                Item = tag,
            }
        );

    public async Task<List<Tag>> ListTagsAdminAsync() => await GetAsync<List<Tag>>(TagListEndpoint) ?? [];

    public async Task CreateTagAdminAsync(string tag) =>
        await PostAsync<object>(TagListEndpoint, new TagAdminRequest { Name = tag });

    public async Task RenameTagAdminAsync(TagId id, string tag) =>
        await PutAsync<object>($"{TagListEndpoint}/{id}", new TagAdminRequest { Name = tag });

    public async Task DeleteTagAdminAsync(TagId id) => await DeleteAsync<object>($"{TagListEndpoint}/{id}");
}
