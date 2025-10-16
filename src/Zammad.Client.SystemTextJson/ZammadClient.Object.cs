using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface IObjectService
{
    Task<List<Object>> ListObjectsAsync();
    Task<Object?> GetObjectAsync(ObjectId id);
    Task<Object> CreateObjectAsync(Object @object);
    Task<Object> UpdateObjectAsync(ObjectId id, Object @object);
    Task<bool> ExecuteMigrationAsync();
}

public sealed partial class ZammadClient : IObjectService
{
    public async Task<List<Object>> ListObjectsAsync() =>
        await GetAsync<List<Object>>("/api/v1/object_manager_attributes") ?? [];

    public async Task<Object?> GetObjectAsync(ObjectId id) =>
        await GetAsync<Object>($"/api/v1/object_manager_attributes/{id}");

    public async Task<Object> CreateObjectAsync(Object @object) =>
        await PostAsync<Object>("/api/v1/object_manager_attributes", @object)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<Object> UpdateObjectAsync(ObjectId id, Object @object) =>
        await PutAsync<Object>($"/api/v1/object_manager_attributes/{id}", @object)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> ExecuteMigrationAsync() =>
        await PostAsync<bool>("/api/v1/object_manager_attributes_execute_migrations");
}
