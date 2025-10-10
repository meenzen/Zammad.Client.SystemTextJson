using System.Collections.Generic;
using System.Threading.Tasks;
using Zammad.Client.Resources;

namespace Zammad.Client.Abstractions;

#nullable enable
public interface IOrganizationService
{
    Task<List<Organization>> GetOrganizationListAsync();
    Task<List<Organization>> GetOrganizationListAsync(int page, int count);
    Task<List<Organization>> SearchOrganizationAsync(string query, int limit);
    Task<List<Organization>> SearchOrganizationAsync(string query, int limit, string sortBy, string orderBy);
    Task<Organization?> GetOrganizationAsync(int id);
    Task<Organization> CreateOrganizationAsync(Organization organization);
    Task<Organization> UpdateOrganizationAsync(int id, Organization organization);
    Task<bool> DeleteOrganizationAsync(int id);
}
