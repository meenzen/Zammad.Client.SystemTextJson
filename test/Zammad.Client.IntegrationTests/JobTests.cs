using System.Net;
using System.Text.Json;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class JobTests(ZammadStackFixture zammadStack)
{
    private static readonly string RandomName = TestSetup.RandomString();
    private static readonly string JobName = "Test Scheduler " + RandomName;
    private static JobId CreatedJobId { get; set; } = JobId.Empty;

    [Test]
    public async Task CreateJob()
    {
        var client = await zammadStack.GetClientAsync();

        // Inactive and matching no ticket, so the scheduler process never runs it
        var job = await client.CreateJobAsync(
            new Job
            {
                Name = JobName,
                Active = false,
                Object = "Ticket",
                Timeplan = JsonSerializer.Deserialize<JsonElement>(
                    """{"days":{"Mon":true},"hours":{"9":true},"minutes":{"0":true}}"""
                ),
                Condition = JsonSerializer.Deserialize<JsonElement>(
                    $$$"""{"ticket.title":{"operator":"contains","value":"{{{RandomName}}}"}}"""
                ),
                Perform = JsonSerializer.Deserialize<JsonElement>("""{"ticket.priority_id":{"value":"3"}}"""),
            }
        );

        await Assert.That(job.Id).IsNotEqualTo(JobId.Empty);
        await Assert.That(job.Name).IsEqualTo(JobName);
        await Assert.That(job.Active).IsFalse();
        await Assert.That(job.Object).IsEqualTo("Ticket");
        await Assert.That(job.Matching).IsEqualTo(0);
        await Assert.That(job.NextRunAt).IsNull();
        await Assert.That(job.Timeplan!.Value.GetProperty("days").GetProperty("Mon").GetBoolean()).IsTrue();

        CreatedJobId = job.Id;
    }

    [Test]
    public async Task CreateJob_ThrowsWithoutObject()
    {
        var client = await zammadStack.GetClientAsync();

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.CreateJobAsync(new Job { Name = "Invalid Scheduler " + RandomName })
        );

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(exception.Error).IsEqualTo("Object can't be blank");
    }

    [Test]
    [DependsOn(nameof(CreateJob))]
    public async Task ListJobs()
    {
        var client = await zammadStack.GetClientAsync();

        var jobs = await client.ListJobsAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(jobs).Contains(j => j.Id == CreatedJobId);
    }

    [Test]
    [DependsOn(nameof(CreateJob))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task SearchJobs(CancellationToken cancellationToken)
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, cancellationToken);
        var jobs = await client.SearchJobsAsync(new SearchQuery { Query = RandomName });

        await Assert.That(jobs).HasSingleItem();
        await Assert.That(jobs[0].Id).IsEqualTo(CreatedJobId);
    }

    [Test]
    [DependsOn(nameof(ListJobs))]
    public async Task GetJob()
    {
        var client = await zammadStack.GetClientAsync();

        var job = await client.GetJobAsync(CreatedJobId);

        await Assert.That(job).IsNotNull();
        await Assert.That(job!.Name).IsEqualTo(JobName);
    }

    [Test]
    [DependsOn(nameof(GetJob))]
    [DependsOn(nameof(SearchJobs))]
    public async Task UpdateJob()
    {
        var client = await zammadStack.GetClientAsync();

        var job = await client.GetJobAsync(CreatedJobId);
        await Assert.That(job).IsNotNull();
        job!.Note = "updated";

        var updated = await client.UpdateJobAsync(CreatedJobId, job);

        await Assert.That(updated.Note).IsEqualTo("updated");
        await Assert.That(updated.Active).IsFalse();
    }

    [Test]
    [DependsOn(nameof(UpdateJob))]
    public async Task DeleteJob()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteJobAsync(CreatedJobId);

        await Assert.That(await client.GetJobAsync(CreatedJobId)).IsNull();
    }
}
