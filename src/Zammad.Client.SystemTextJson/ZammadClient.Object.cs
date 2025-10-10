using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

#nullable enable

public interface IObjectService
{
    Task<List<Object>> GetObjectListAsync();
    Task<Object?> GetObjectAsync(int id);
    Task<Object> CreateObjectAsync(Object @object);
    Task<Object> UpdateObjectAsync(int id, Object @object);
    Task<bool> ExecuteMigrationAsync();
}

public sealed partial class ZammadClient : IObjectService
{
    public async Task<List<Object>> GetObjectListAsync() =>
        await GetAsync<List<Object>>("/api/v1/object_manager_attributes") ?? [];

    public async Task<Object?> GetObjectAsync(int id) =>
        await GetAsync<Object>($"/api/v1/object_manager_attributes/{id}");

    public async Task<Object> CreateObjectAsync(Object @object) =>
        await PostAsync<Object>("/api/v1/object_manager_attributes", @object)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<Object> UpdateObjectAsync(int id, Object @object) =>
        await PutAsync<Object>($"/api/v1/object_manager_attributes/{id}", @object)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<bool> ExecuteMigrationAsync() =>
        await PostAsync<bool>("/api/v1/object_manager_attributes_execute_migrations");
}
