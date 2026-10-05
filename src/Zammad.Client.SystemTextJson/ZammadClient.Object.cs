using Zammad.Client.Core;
using Zammad.Client.Resources;
using Object = Zammad.Client.Resources.Object;

namespace Zammad.Client;

public interface IObjectService
{
    Task<List<Object>> ListObjectsAsync();
    Task<Object?> GetObjectAsync(ObjectId id);
    Task<Object> CreateObjectAsync(Object @object);
    Task<Object> UpdateObjectAsync(ObjectId id, Object @object);

    /// <summary>
    /// Deletes an attribute. An attribute that hasn't been migrated yet is removed right away. Any other attribute is
    /// only marked as <see cref="Object.ToDelete"/>, and the next <see cref="ExecuteMigrationAsync"/> drops the column.
    /// </summary>
    Task DeleteObjectAsync(ObjectId id);

    Task ExecuteMigrationAsync();

    /// <summary>
    /// Discards all pending attribute changes that haven't been migrated yet, including those made by other clients.
    /// </summary>
    Task DiscardMigrationChangesAsync();
}

public sealed partial class ZammadClient : IObjectService
{
    private const string ObjectManagerAttributesEndpoint = "/api/v1/object_manager_attributes";

    public async Task<List<Object>> ListObjectsAsync() =>
        await GetAsync<List<Object>>(ObjectManagerAttributesEndpoint) ?? [];

    public async Task<Object?> GetObjectAsync(ObjectId id) =>
        await GetAsync<Object>($"{ObjectManagerAttributesEndpoint}/{id}");

    public async Task<Object> CreateObjectAsync(Object @object) =>
        await PostAsync<Object>(ObjectManagerAttributesEndpoint, @object) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Object> UpdateObjectAsync(ObjectId id, Object @object) =>
        await PutAsync<Object>($"{ObjectManagerAttributesEndpoint}/{id}", @object)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteObjectAsync(ObjectId id) =>
        await DeleteAsync<object>($"{ObjectManagerAttributesEndpoint}/{id}");

    public async Task ExecuteMigrationAsync() =>
        await PostAsync<bool>($"{ObjectManagerAttributesEndpoint}_execute_migrations");

    public async Task DiscardMigrationChangesAsync() =>
        await PostAsync<object>($"{ObjectManagerAttributesEndpoint}_discard_changes");
}
