using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Abstractions;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public sealed partial class ZammadClient : IObjectService
{
    public Task<IList<Object>> GetObjectListAsync() => GetAsync<IList<Object>>("/api/v1/object_manager_attributes");

    public Task<Object> GetObjectAsync(int id) => GetAsync<Object>($"/api/v1/object_manager_attributes/{id}");

    public Task<Object> CreateObjectAsync(Object @object) =>
        PostAsync<Object>("/api/v1/object_manager_attributes", @object);

    public Task<Object> UpdateObjectAsync(int id, Object @object) =>
        PutAsync<Object>($"/api/v1/object_manager_attributes/{id}", @object);

    public Task<bool> ExecuteMigrationAsync() =>
        PostAsync<bool>("/api/v1/object_manager_attributes_execute_migrations");
}
