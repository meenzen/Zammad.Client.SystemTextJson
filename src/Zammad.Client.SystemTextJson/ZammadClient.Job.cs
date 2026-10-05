using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

/// <summary>
/// Schedulers (<c>/api/v1/jobs</c>). Requires the <c>admin.scheduler</c> permission.
/// </summary>
/// <remarks>
/// The admin interface calls these "Schedulers", but the API and Zammad's model call them jobs. This client follows
/// the API, so the resource is <see cref="Job"/>. They are unrelated to Zammad's background jobs (see
/// <see cref="IMonitoringService.RestartFailedJobsAsync"/>).
/// </remarks>
public interface IJobService
{
    Task<List<Job>> ListJobsAsync(Pagination? pagination = null);

    /// <summary>
    /// Searches schedulers by name and note.
    /// </summary>
    /// <remarks>
    /// With a query, Zammad searches the Elasticsearch index, so new or changed schedulers show up with a delay.
    /// </remarks>
    Task<List<Job>> SearchJobsAsync(SearchQuery query);

    Task<Job?> GetJobAsync(JobId id);

    /// <summary>
    /// Creates a scheduler.
    /// </summary>
    /// <remarks>
    /// <see cref="Job.Name"/> (unique, case-insensitive), <see cref="Job.Timeplan"/>, <see cref="Job.Condition"/>
    /// and <see cref="Job.Perform"/> are required, and <see cref="Job.Object"/> must be <c>Ticket</c>,
    /// <c>User</c> or <c>Organization</c>. Zammad sets <see cref="Job.Matching"/> and <see cref="Job.NextRunAt"/>
    /// on every save. Active schedulers are run by Zammad's scheduler process once the timeplan matches and the
    /// last change is more than a minute old.
    /// </remarks>
    Task<Job> CreateJobAsync(Job job);

    Task<Job> UpdateJobAsync(JobId id, Job job);

    Task DeleteJobAsync(JobId id);
}

public sealed partial class ZammadClient : IJobService
{
    private const string JobsEndpoint = "/api/v1/jobs";
    private const string JobsSearchEndpoint = "/api/v1/jobs/search";

    public async Task<List<Job>> ListJobsAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<Job>>(JobsEndpoint, builder.ToString()) ?? [];
    }

    public async Task<List<Job>> SearchJobsAsync(SearchQuery query)
    {
        var builder = new QueryBuilder();
        builder.AddSearchQuery(query);
        return await GetAsync<List<Job>>(JobsSearchEndpoint, builder.ToString()) ?? [];
    }

    public async Task<Job?> GetJobAsync(JobId id) => await GetAsync<Job>($"{JobsEndpoint}/{id}");

    public async Task<Job> CreateJobAsync(Job job) =>
        await PostAsync<Job>(JobsEndpoint, job) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Job> UpdateJobAsync(JobId id, Job job) =>
        await PutAsync<Job>($"{JobsEndpoint}/{id}", job) ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteJobAsync(JobId id) => await DeleteAsync<bool>($"{JobsEndpoint}/{id}");
}
