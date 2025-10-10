using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Abstractions;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client;

public sealed partial class ZammadClient : ITagService
{
    public async Task<List<string>> GetTagListAsync(string objectName, int objectId)
    {
        var tagList = await GetAsync<StringTagList>("/api/v1/tags", $"object={objectName}&o_id={objectId}");
        return tagList.Tags;
    }

    public Task<List<Tag>> SearchTagAsync(string term) => GetAsync<List<Tag>>("/api/v1/tag_search", $"term={term}");

    public Task<bool> AddTagAsync(string objectName, int objectId, string tagName) =>
        GetAsync<bool>("/api/v1/tags/add", $"object={objectName}&o_id={objectId}&item={tagName}");

    public Task<bool> RemoveTagAsync(string objectName, int objectId, string tagName) =>
        GetAsync<bool>("/api/v1/tags/remove", $"object={objectName}&o_id={objectId}&item={tagName}");

    public Task<List<Tag>> GetTagListAdminAsync() => GetAsync<List<Tag>>("/api/v1/tag_list");

    public Task<bool> CreateTagAdminAsync(Tag tag) => PostAsync<bool>("/api/v1/tag_list", tag);

    public Task<bool> RenameTagAdminAsync(Tag tag) => PutAsync<bool>("/api/v1/tag_list", tag);

    public Task<bool> DeleteTagAdminAsync(Tag tag) => DeleteAsync<bool>("/api/v1/tag_list", tag);
}
