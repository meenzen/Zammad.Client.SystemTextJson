using System.Collections.Generic;
using System.Threading.Tasks;
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
    public async Task<List<string>> ListTagsAsync(string objectName, ObjectId objectId)
    {
        var tagList = await GetAsync<StringTagList>("/api/v1/tags", $"object={objectName}&o_id={objectId}");
        return tagList?.Tags ?? [];
    }

    public async Task<List<Tag>> SearchTagsAsync(string term) =>
        await GetAsync<List<Tag>>("/api/v1/tag_search", $"term={term}") ?? [];

    public async Task<bool> AddTagAsync(string objectName, ObjectId objectId, string tagName) =>
        await GetAsync<bool>("/api/v1/tags/add", $"object={objectName}&o_id={objectId}&item={tagName}");

    public async Task<bool> RemoveTagAsync(string objectName, ObjectId objectId, string tagName) =>
        await GetAsync<bool>("/api/v1/tags/remove", $"object={objectName}&o_id={objectId}&item={tagName}");

    public async Task<List<Tag>> ListTagsAdminAsync() => await GetAsync<List<Tag>>("/api/v1/tag_list") ?? [];

    public async Task<bool> CreateTagAdminAsync(Tag tag) => await PostAsync<bool>("/api/v1/tag_list", tag);

    public async Task<bool> RenameTagAdminAsync(Tag tag) => await PutAsync<bool>("/api/v1/tag_list", tag);

    public async Task<bool> DeleteTagAdminAsync(Tag tag) => await DeleteAsync<bool>("/api/v1/tag_list", tag);
}
