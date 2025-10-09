using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Resources;

namespace Zammad.Client.Abstractions;

public interface IObjectService
{
    Task<IList<Object>> GetObjectListAsync();
    Task<Object> GetObjectAsync(int id);
    Task<Object> CreateObjectAsync(Object @object);
    Task<Object> UpdateObjectAsync(int id, Object @object);
    Task<bool> ExecuteMigrationAsync();
}
