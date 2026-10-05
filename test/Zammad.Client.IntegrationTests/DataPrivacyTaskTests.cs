using System.Net;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

/// <summary>
/// Only ever targets records these tests created. Tasks are irreversible, and Zammad refuses to delete the system user,
/// the current user and the last admin, but would delete any other seeded user.
/// </summary>
[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class DataPrivacyTaskTests(ZammadStackFixture zammadStack)
{
    // Setup/docker-entrypoint makes the scheduler run the tasks every 10 seconds instead of every 10 minutes
    private static readonly TimeSpan TaskTimeout = TimeSpan.FromSeconds(120);

    private static readonly string RandomName = TestSetup.RandomString();

    private static OrganizationId OrganizationId { get; set; } = OrganizationId.Empty;
    private static UserId CustomerId { get; set; } = UserId.Empty;
    private static Ticket? CustomerTicket { get; set; }
    private static DataPrivacyTaskId UserTaskId { get; set; } = DataPrivacyTaskId.Empty;

    private static TicketId TicketId { get; set; } = TicketId.Empty;
    private static DataPrivacyTaskId TicketTaskId { get; set; } = DataPrivacyTaskId.Empty;

    [Test]
    public async Task CreateCustomerWithTicket()
    {
        var client = await zammadStack.GetClientAsync();

        var organization = await client.CreateOrganizationAsync(
            new Organization { Name = "Data Privacy Org " + RandomName, Active = true }
        );
        OrganizationId = organization.Id;

        var customer = await client.CreateUserAsync(
            new User
            {
                FirstName = "Homer",
                LastName = "Simpson" + RandomName,
                Email = $"homer.privacy.{RandomName}@springfield.com",
                Login = $"homer.privacy.{RandomName}",
                OrganizationId = organization.Id,
                Active = true,
            }
        );
        CustomerId = customer.Id;

        CustomerTicket = await CreateTicketAsync(client, customer.Id);
    }

    [Test]
    [DependsOn(nameof(CreateCustomerWithTicket))]
    public async Task CreateDataPrivacyTask_RejectsOrganization()
    {
        var client = await zammadStack.GetClientAsync();

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.CreateDataPrivacyTaskAsync(
                new DataPrivacyTask { DeletableType = "Organization", DeletableId = OrganizationId.ToTargetObjectId() }
            )
        );

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(exception.Error).IsEqualTo("Data privacy task allows to delete a user or a ticket only.");
    }

    [Test]
    [DependsOn(nameof(CreateDataPrivacyTask_RejectsOrganization))]
    public async Task CreateUserDataPrivacyTask()
    {
        var client = await zammadStack.GetClientAsync();

        var task = await client.CreateUserDataPrivacyTaskAsync(CustomerId, deleteOrganization: true);
        UserTaskId = task.Id;

        await Assert.That(task.Id).IsNotEqualTo(DataPrivacyTaskId.Empty);
        await Assert.That(task.State).IsEqualTo(DataPrivacyTaskState.InProcess);
        await Assert.That(task.DeletableType).IsEqualTo(DataPrivacyTaskDeletableType.User);
        await Assert.That(task.DeletableId).IsEqualTo(CustomerId.ToTargetObjectId());

        // The preview of what the task deletes, with the user pseudonymized
        await Assert.That(task.CustomerTicketsCount).IsEqualTo(1);
        await Assert.That(task.CustomerTickets).IsEquivalentTo([CustomerTicket!.Number!]);
        await Assert.That(task.OwnerTicketsCount).IsEqualTo(0);
        await Assert.That(task.Preferences!["delete_organization"].GetString()).IsEqualTo("true");
        var user = task.Preferences["user"];
        await Assert.That(user.GetProperty("firstname").GetString()).IsEqualTo("H*r");
        await Assert.That(user.GetProperty("organization").GetString()).IsNotNull().And.Contains("*");
    }

    [Test]
    [DependsOn(nameof(CreateUserDataPrivacyTask))]
    public async Task CreateUserDataPrivacyTask_RejectsDuplicate()
    {
        var client = await zammadStack.GetClientAsync();

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.CreateUserDataPrivacyTaskAsync(CustomerId)
        );

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(exception.Error).IsEqualTo("Selected object is already queued for deletion.");
    }

    [Test]
    [DependsOn(nameof(CreateUserDataPrivacyTask))]
    public async Task GetDataPrivacyTask()
    {
        var client = await zammadStack.GetClientAsync();

        var task = await client.GetDataPrivacyTaskAsync(UserTaskId);

        await Assert.That(task).IsNotNull();
        await Assert.That(task!.Id).IsEqualTo(UserTaskId);
        await Assert.That(task.DeletableId).IsEqualTo(CustomerId.ToTargetObjectId());
    }

    [Test]
    [DependsOn(nameof(CreateUserDataPrivacyTask))]
    public async Task ListDataPrivacyTasks()
    {
        var client = await zammadStack.GetClientAsync();

        var tasks = await client.ListDataPrivacyTasksAsync(new Pagination { Page = 1, PerPage = 500 });

        await Assert.That(tasks).Contains(t => t.Id == UserTaskId);
    }

    [Test]
    [DependsOn(nameof(CreateUserDataPrivacyTask))]
    public async Task ListDataPrivacyTasksByState()
    {
        var client = await zammadStack.GetClientAsync();

        var tasks = await client.ListDataPrivacyTasksByStateAsync();

        // The scheduler may already have run the task
        var task = tasks.InProcess.Concat(tasks.Completed).FirstOrDefault(t => t.Id == UserTaskId);
        await Assert.That(task).IsNotNull();
        await Assert.That(task!.DeletableId).IsEqualTo(CustomerId.ToTargetObjectId());
        await Assert.That(tasks.Failed).DoesNotContain(t => t.Id == UserTaskId);
    }

    [Test]
    [DependsOn(nameof(CreateUserDataPrivacyTask_RejectsDuplicate))]
    [DependsOn(nameof(GetDataPrivacyTask))]
    [DependsOn(nameof(ListDataPrivacyTasks))]
    [DependsOn(nameof(ListDataPrivacyTasksByState))]
    public async Task UserDataPrivacyTask_DeletesUserTicketsAndOrganization()
    {
        var client = await zammadStack.GetClientAsync();

        var task = await WaitForTaskAsync(client, UserTaskId);

        await Assert.That(task.State).IsEqualTo(DataPrivacyTaskState.Completed);
        await Assert.That(await client.GetUserAsync(CustomerId)).IsNull();
        await Assert.That(await client.GetTicketAsync(CustomerTicket!.Id)).IsNull();
        await Assert.That(await client.GetOrganizationAsync(OrganizationId)).IsNull();
    }

    [Test]
    [DependsOn(nameof(UserDataPrivacyTask_DeletesUserTicketsAndOrganization))]
    public async Task DeleteDataPrivacyTask()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteDataPrivacyTaskAsync(UserTaskId);

        await Assert.That(await client.GetDataPrivacyTaskAsync(UserTaskId)).IsNull();
    }

    [Test]
    public async Task CreateTicketDataPrivacyTask()
    {
        var client = await zammadStack.GetClientAsync();
        var ticket = await CreateTicketAsync(client, new UserId(1));
        TicketId = ticket.Id;

        var task = await client.CreateTicketDataPrivacyTaskAsync(ticket.Id);
        TicketTaskId = task.Id;

        await Assert.That(task.DeletableType).IsEqualTo(DataPrivacyTaskDeletableType.Ticket);
        await Assert.That(task.DeletableId).IsEqualTo(ticket.Id.ToTargetObjectId());
        await Assert.That(task.CustomerTickets).IsEquivalentTo([ticket.Number!]);
        await Assert.That(task.Preferences!["ticket"].GetProperty("title").GetString()).Contains("*");
    }

    [Test]
    [DependsOn(nameof(CreateTicketDataPrivacyTask))]
    public async Task TicketDataPrivacyTask_DeletesTicket()
    {
        var client = await zammadStack.GetClientAsync();

        var task = await WaitForTaskAsync(client, TicketTaskId);

        await Assert.That(task.State).IsEqualTo(DataPrivacyTaskState.Completed);
        await Assert.That(await client.GetTicketAsync(TicketId)).IsNull();
    }

    [Test]
    [DependsOn(nameof(TicketDataPrivacyTask_DeletesTicket))]
    public async Task UpdateDataPrivacyTask_Retry()
    {
        var client = await zammadStack.GetClientAsync();
        var task = await client.GetDataPrivacyTaskAsync(TicketTaskId);
        await Assert.That(task).IsNotNull();

        // This is how a failed task is retried. The ticket is gone already, so the task just completes again.
        task!.State = DataPrivacyTaskState.InProcess;
        var updated = await client.UpdateDataPrivacyTaskAsync(TicketTaskId, task);
        await Assert.That(updated.State).IsEqualTo(DataPrivacyTaskState.InProcess);

        var completed = await WaitForTaskAsync(client, TicketTaskId);
        await Assert.That(completed.State).IsEqualTo(DataPrivacyTaskState.Completed);
        await Assert.That(completed.Error).IsNull();
    }

    [Test]
    [DependsOn(nameof(UpdateDataPrivacyTask_Retry))]
    public async Task DeleteTicketDataPrivacyTask()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteDataPrivacyTaskAsync(TicketTaskId);

        await Assert.That(await client.GetDataPrivacyTaskAsync(TicketTaskId)).IsNull();
    }

    private static async Task<Ticket> CreateTicketAsync(IZammadClient client, UserId customerId) =>
        await client.CreateTicketAsync(
            new Ticket
            {
                Title = "Data Privacy Ticket " + TestSetup.RandomString(),
                GroupId = new GroupId(1),
                CustomerId = customerId,
            },
            new TicketArticle
            {
                Subject = "Data Privacy",
                Body = "To be deleted",
                Type = "note",
            }
        );

    private static async Task<DataPrivacyTask> WaitForTaskAsync(IZammadClient client, DataPrivacyTaskId id)
    {
        var timeout = DateTimeOffset.UtcNow + TaskTimeout;
        while (true)
        {
            var task = await client.GetDataPrivacyTaskAsync(id) ?? throw new InvalidOperationException("Task is gone");
            if (task.State == DataPrivacyTaskState.Failed)
            {
                throw new InvalidOperationException($"Data privacy task failed: {task.Error}");
            }

            if (task.State != DataPrivacyTaskState.InProcess || DateTimeOffset.UtcNow > timeout)
            {
                return task;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
