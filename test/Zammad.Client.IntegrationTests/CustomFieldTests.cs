using System.Text.Json;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;
using Object = Zammad.Client.Resources.Object;

namespace Zammad.Client.IntegrationTests;

/// <summary>
/// Custom attributes created in the object manager are read and written through <see cref="CustomFieldExtensions"/>.
/// </summary>
/// <remarks>
/// The attributes are migrated by <see cref="ObjectTests.ExecuteMigration"/>, so the stack only restarts once.
/// </remarks>
[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class CustomFieldTests(ZammadStackFixture zammadStack)
{
    private static readonly string Id = TestSetup.RandomString();

    // Attribute names may only contain lowercase letters, numbers and '_'
    private static readonly string TicketText = $"cf_text_{Id}";
    private static readonly string TicketInteger = $"cf_integer_{Id}";
    private static readonly string TicketBoolean = $"cf_boolean_{Id}";
    private static readonly string TicketSelect = $"cf_select_{Id}";
    private static readonly string TicketMultiSelect = $"cf_multiselect_{Id}";
    private static readonly string TicketDate = $"cf_date_{Id}";
    private static readonly string UserText = $"cf_user_text_{Id}";
    private static readonly string OrganizationText = $"cf_organization_text_{Id}";

    private static readonly DateOnly DateValue = new(2026, 10, 5);

    private static TicketId? CreatedTicketId { get; set; }
    private static UserId? CreatedUserId { get; set; }
    private static OrganizationId? CreatedOrganizationId { get; set; }

    private static Object Attribute(string objectType, string name, string dataType, string dataOption) =>
        JsonSerializer.Deserialize<Object>(
            $$"""
            {
               "name": "{{name}}",
               "object": "{{objectType}}",
               "display": "{{name}}",
               "active": true,
               "position": 1600,
               "data_type": "{{dataType}}",
               "data_option": {{dataOption}}
            }
            """
        ) ?? throw new InvalidOperationException();

    [Test]
    public async Task CreateAttributes()
    {
        Object[] attributes =
        [
            Attribute("Ticket", TicketText, "input", """{ "type": "text", "maxlength": 120, "null": true }"""),
            Attribute("Ticket", TicketInteger, "integer", """{ "min": 0, "max": 1000, "null": true }"""),
            Attribute(
                "Ticket",
                TicketBoolean,
                "boolean",
                """{ "default": false, "options": { "true": "yes", "false": "no" }, "null": true }"""
            ),
            Attribute(
                "Ticket",
                TicketSelect,
                "select",
                """{ "default": "", "options": { "a": "Option A", "b": "Option B" }, "null": true }"""
            ),
            Attribute(
                "Ticket",
                TicketMultiSelect,
                "multiselect",
                """{ "default": [], "options": { "a": "Option A", "b": "Option B", "c": "Option C" }, "null": true }"""
            ),
            Attribute("Ticket", TicketDate, "date", """{ "diff": null, "null": true }"""),
            Attribute("User", UserText, "input", """{ "type": "text", "maxlength": 120, "null": true }"""),
            Attribute(
                "Organization",
                OrganizationText,
                "input",
                """{ "type": "text", "maxlength": 120, "null": true }"""
            ),
        ];

        var client = await zammadStack.GetClientAsync();

        foreach (var attribute in attributes)
        {
            var created = await client.CreateObjectAsync(attribute);
            await Assert.That(created.Id).IsNotEqualTo(ObjectId.Empty);
            await Assert.That(created.Name).IsEqualTo(attribute.Name);
        }
    }

    [Test]
    [DependsOn(nameof(CreateAttributes))]
    [DependsOn(typeof(ObjectTests), nameof(ObjectTests.ExecuteMigration))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task CreateTicketWithCustomFields()
    {
        var client = await zammadStack.GetClientAsync();

        var ticket = await client.CreateTicketAsync(
            new Ticket
            {
                Title = $"CustomFieldTests {Id}",
                GroupId = new GroupId(1),
                CustomerId = new UserId(1),
            }
                .WithCustomField(TicketText, "hello")
                .WithCustomField(TicketInteger, 42)
                .WithCustomField(TicketBoolean, true)
                .WithCustomField(TicketSelect, "b")
                .WithCustomField(TicketMultiSelect, new[] { "a", "c" })
                .WithCustomField(TicketDate, DateValue),
            new TicketArticle
            {
                Subject = $"CustomFieldTests {Id}",
                Body = "Custom fields",
                Type = "note",
            }
        );

        await Assert.That(ticket.Id).IsNotEqualTo(TicketId.Empty);
        CreatedTicketId = ticket.Id;

        await Assert.That(ticket.GetCustomField<string>(TicketText)).IsEqualTo("hello");
        await Assert.That(ticket.GetCustomField<int>(TicketInteger)).IsEqualTo(42);
        await Assert.That(ticket.GetCustomField<bool>(TicketBoolean)).IsTrue();
        await Assert.That(ticket.GetCustomField<string>(TicketSelect)).IsEqualTo("b");
        await Assert.That(ticket.GetCustomField<List<string>>(TicketMultiSelect)).IsEquivalentTo(["a", "c"]);
        await Assert.That(ticket.GetCustomField<DateOnly>(TicketDate)).IsEqualTo(DateValue);
    }

    [Test]
    [DependsOn(nameof(CreateTicketWithCustomFields))]
    public async Task GetTicketWithCustomFields()
    {
        await Assert.That(CreatedTicketId).IsNotNull();
        var client = await zammadStack.GetClientAsync();

        var ticket = await client.GetTicketAsync(CreatedTicketId!.Value);

        await Assert.That(ticket).IsNotNull();
        await Assert.That(ticket!.GetCustomField<string>(TicketText)).IsEqualTo("hello");
        await Assert.That(ticket.GetCustomField<int>(TicketInteger)).IsEqualTo(42);
        await Assert.That(ticket.GetCustomField<bool>(TicketBoolean)).IsTrue();
        await Assert.That(ticket.GetCustomField<string>(TicketSelect)).IsEqualTo("b");
        await Assert.That(ticket.GetCustomField<List<string>>(TicketMultiSelect)).IsEquivalentTo(["a", "c"]);
        await Assert.That(ticket.GetCustomField<DateOnly>(TicketDate)).IsEqualTo(DateValue);
    }

    [Test]
    [DependsOn(nameof(GetTicketWithCustomFields))]
    public async Task UpdateTicketCustomFields()
    {
        await Assert.That(CreatedTicketId).IsNotNull();
        var client = await zammadStack.GetClientAsync();

        var ticket = await client.UpdateTicketAsync(
            CreatedTicketId!.Value,
            new Ticket()
                .WithCustomField(TicketText, "updated")
                .WithCustomField(TicketMultiSelect, new[] { "b" })
                .WithCustomField(TicketInteger, (int?)null)
        );

        await Assert.That(ticket.GetCustomField<string>(TicketText)).IsEqualTo("updated");
        await Assert.That(ticket.GetCustomField<List<string>>(TicketMultiSelect)).IsEquivalentTo(["b"]);
        await Assert.That(ticket.GetCustomField<int?>(TicketInteger)).IsNull();
        // fields that weren't sent stay unchanged
        await Assert.That(ticket.GetCustomField<bool>(TicketBoolean)).IsTrue();
        await Assert.That(ticket.GetCustomField<string>(TicketSelect)).IsEqualTo("b");
        await Assert.That(ticket.GetCustomField<DateOnly>(TicketDate)).IsEqualTo(DateValue);
    }

    [Test]
    [DependsOn(nameof(UpdateTicketCustomFields))]
    public async Task UpdateTicketCustomFields_ModifyFetchedTicket()
    {
        await Assert.That(CreatedTicketId).IsNotNull();
        var client = await zammadStack.GetClientAsync();

        var ticket = await client.GetTicketAsync(CreatedTicketId!.Value);
        await Assert.That(ticket).IsNotNull();
        ticket!.SetCustomField(TicketInteger, 7);
        ticket.SetCustomField(TicketSelect, "a");

        var updated = await client.UpdateTicketAsync(CreatedTicketId.Value, ticket);

        await Assert.That(updated.GetCustomField<int>(TicketInteger)).IsEqualTo(7);
        await Assert.That(updated.GetCustomField<string>(TicketSelect)).IsEqualTo("a");
        await Assert.That(updated.GetCustomField<string>(TicketText)).IsEqualTo("updated");
    }

    [Test]
    [DependsOn(nameof(CreateAttributes))]
    [DependsOn(typeof(ObjectTests), nameof(ObjectTests.ExecuteMigration))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task CreateUserWithCustomField()
    {
        var client = await zammadStack.GetClientAsync();

        var user = await client.CreateUserAsync(
            new User
            {
                FirstName = "Custom",
                LastName = $"Field {Id}",
                Email = $"custom.field.{Id}@example.org",
                Login = $"custom.field.{Id}",
                Active = true,
            }.WithCustomField(UserText, "user value")
        );

        CreatedUserId = user.Id;
        await Assert.That(user.GetCustomField<string>(UserText)).IsEqualTo("user value");
    }

    [Test]
    [DependsOn(nameof(CreateUserWithCustomField))]
    public async Task UpdateUserCustomField()
    {
        await Assert.That(CreatedUserId).IsNotNull();
        var client = await zammadStack.GetClientAsync();

        var user = await client.GetUserAsync(CreatedUserId!.Value);
        await Assert.That(user).IsNotNull();
        await Assert.That(user!.GetCustomField<string>(UserText)).IsEqualTo("user value");

        user.SetCustomField(UserText, "updated user value");
        var updated = await client.UpdateUserAsync(CreatedUserId.Value, user);

        await Assert.That(updated.GetCustomField<string>(UserText)).IsEqualTo("updated user value");
    }

    [Test]
    [DependsOn(nameof(CreateAttributes))]
    [DependsOn(typeof(ObjectTests), nameof(ObjectTests.ExecuteMigration))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task CreateOrganizationWithCustomField()
    {
        var client = await zammadStack.GetClientAsync();

        var organization = await client.CreateOrganizationAsync(
            new Organization
            {
                Name = $"CustomFieldTests {Id}",
                Shared = true,
                Active = true,
            }.WithCustomField(OrganizationText, "organization value")
        );

        CreatedOrganizationId = organization.Id;
        await Assert.That(organization.GetCustomField<string>(OrganizationText)).IsEqualTo("organization value");
    }

    [Test]
    [DependsOn(nameof(CreateOrganizationWithCustomField))]
    public async Task UpdateOrganizationCustomField()
    {
        await Assert.That(CreatedOrganizationId).IsNotNull();
        var client = await zammadStack.GetClientAsync();

        var organization = await client.GetOrganizationAsync(CreatedOrganizationId!.Value);
        await Assert.That(organization).IsNotNull();
        await Assert.That(organization!.GetCustomField<string>(OrganizationText)).IsEqualTo("organization value");

        organization.ClearCustomField(OrganizationText);
        var updated = await client.UpdateOrganizationAsync(CreatedOrganizationId.Value, organization);

        await Assert.That(updated.GetCustomField<string>(OrganizationText)).IsNull();
    }
}
