using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

public interface ITagService
{
    Task<List<string>> ListTagsAsync(string objectName, ObjectId objectId);
    Task<List<Tag>> SearchTagsAsync(string term);
    Task<bool> AddTagAsync(string objectName, ObjectId objectId, string tagName);
    Task<bool> RemoveTagAsync(string objectName, ObjectId objectId, string tagName);
    Task<List<Tag>> ListTagsAdminAsync();
    Task<bool> CreateTagAdminAsync(Tag tag);
    Task<bool> RenameTagAdminAsync(Tag tag);
    Task<bool> DeleteTagAdminAsync(Tag tag);
}

public sealed partial class ZammadClient : ITagService
{
    private const string TagsEndpoint = "/api/v1/tags";
    private const string TagSearchEndpoint = "/api/v1/tag_search";
    private const string TagListEndpoint = "/api/v1/tag_list";

    public async Task<List<string>> ListTagsAsync(string objectName, ObjectId objectId)
    {
        var builder = new QueryBuilder();
        builder.Add("object", objectName);
        builder.Add("o_id", objectId.ToString());
        var tagList = await GetAsync<StringTagList>(TagsEndpoint, builder.ToString());
        return tagList?.Tags ?? [];
    }

    public async Task<List<Tag>> SearchTagsAsync(string term)
    {
        var builder = new QueryBuilder();
        builder.Add("term", term);
        return await GetAsync<List<Tag>>(TagSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<bool> AddTagAsync(string objectName, ObjectId objectId, string tagName)
    {
        var builder = new QueryBuilder();
        builder.Add("object", objectName);
        builder.Add("o_id", objectId.ToString());
        builder.Add("item", tagName);
        return await GetAsync<bool>($"{TagsEndpoint}/add", builder.ToString());
    }

    public async Task<bool> RemoveTagAsync(string objectName, ObjectId objectId, string tagName)
    {
        var builder = new QueryBuilder();
        builder.Add("object", objectName);
        builder.Add("o_id", objectId.ToString());
        builder.Add("item", tagName);
        return await GetAsync<bool>($"{TagsEndpoint}/remove", builder.ToString());
    }

    public async Task<List<Tag>> ListTagsAdminAsync() => await GetAsync<List<Tag>>(TagListEndpoint) ?? [];

    public async Task<bool> CreateTagAdminAsync(Tag tag) => await PostAsync<bool>(TagListEndpoint, tag);

    public async Task<bool> RenameTagAdminAsync(Tag tag) => await PutAsync<bool>(TagListEndpoint, tag);

    public async Task<bool> DeleteTagAdminAsync(Tag tag) => await DeleteAsync<bool>(TagListEndpoint, tag);
}
