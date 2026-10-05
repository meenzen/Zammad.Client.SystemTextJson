using System.Text.Json;
using System.Text.Json.Serialization;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client.Tests.Resources;

public class CustomFieldExtensionsTests
{
    private static Ticket Deserialize(string json) =>
        JsonSerializer.Deserialize<Ticket>(json, Serialization.Options) ?? throw new InvalidOperationException();

    private static JsonElement SerializeToElement(object value) =>
        JsonSerializer.SerializeToElement(value, value.GetType(), Serialization.Options);

    [Test]
    public async Task GetCustomField_ReadsTypedValues()
    {
        var ticket = Deserialize(
            """
            {
              "id": 1,
              "cf_text": "hello",
              "cf_integer": 42,
              "cf_boolean": true,
              "cf_datetime": "2026-10-05T12:30:00.000Z",
              "cf_multiselect": ["a", "b"],
              "cf_null": null
            }
            """
        );

        await Assert.That(ticket.GetCustomField<string>("cf_text")).IsEqualTo("hello");
        await Assert.That(ticket.GetCustomField<int>("cf_integer")).IsEqualTo(42);
        await Assert.That(ticket.GetCustomField<bool>("cf_boolean")).IsTrue();
        await Assert
            .That(ticket.GetCustomField<DateTimeOffset>("cf_datetime"))
            .IsEqualTo(new DateTimeOffset(2026, 10, 5, 12, 30, 0, TimeSpan.Zero));
        await Assert.That(ticket.GetCustomField<List<string>>("cf_multiselect")).IsEquivalentTo(["a", "b"]);
        await Assert.That(ticket.GetCustomField<string>("cf_null")).IsNull();
        await Assert.That(ticket.GetCustomField<int?>("cf_null")).IsNull();
    }

    [Test]
    public async Task GetCustomField_MissingField_ReturnsDefault()
    {
        var ticket = new Ticket();

        await Assert.That(ticket.GetCustomField<string>("missing")).IsNull();
        await Assert.That(ticket.GetCustomField<int>("missing")).IsEqualTo(0);
        await Assert.That(ticket.GetCustomField<int?>("missing")).IsNull();
    }

    [Test]
    public async Task GetCustomField_WrongType_Throws()
    {
        var ticket = Deserialize("""{ "cf_text": "hello" }""");

        await Assert.That(() => ticket.GetCustomField<int>("cf_text")).Throws<JsonException>();
    }

    [Test]
    public async Task TryGetCustomField_DistinguishesMissingFromNull()
    {
        var ticket = Deserialize("""{ "cf_null": null }""");

        var foundNull = ticket.TryGetCustomField<string>("cf_null", out var nullValue);
        var foundMissing = ticket.TryGetCustomField<string>("missing", out var missingValue);

        await Assert.That(foundNull).IsTrue();
        await Assert.That(nullValue).IsNull();
        await Assert.That(foundMissing).IsFalse();
        await Assert.That(missingValue).IsNull();
        await Assert.That(ticket.HasCustomField("cf_null")).IsTrue();
        await Assert.That(ticket.HasCustomField("missing")).IsFalse();
    }

    [Test]
    public async Task SetCustomField_CreatesExtensionData()
    {
        var ticket = new Ticket();

        ticket.SetCustomField("cf_text", "hello");

        await Assert.That(ticket.ExtensionData).IsNotNull();
        await Assert.That(ticket.ExtensionData!["cf_text"].GetString()).IsEqualTo("hello");
    }

    [Test]
    public async Task SetCustomField_OverwritesExistingValue()
    {
        var ticket = Deserialize("""{ "cf_integer": 1 }""");

        ticket.SetCustomField("cf_integer", 2);

        await Assert.That(ticket.GetCustomField<int>("cf_integer")).IsEqualTo(2);
    }

    [Test]
    public async Task SetCustomField_IsSerializedAsTopLevelProperty()
    {
        var ticket = new Ticket { Title = "Title" };
        ticket.SetCustomField("cf_text", "hello");
        ticket.SetCustomField("cf_integer", 42);
        ticket.SetCustomField("cf_boolean", false);
        ticket.SetCustomField("cf_multiselect", new[] { "a", "b" });
        ticket.SetCustomField<string?>("cf_cleared", null);

        var json = SerializeToElement(ticket);

        await Assert.That(json.GetProperty("title").GetString()).IsEqualTo("Title");
        await Assert.That(json.GetProperty("cf_text").GetString()).IsEqualTo("hello");
        await Assert.That(json.GetProperty("cf_integer").GetInt32()).IsEqualTo(42);
        await Assert.That(json.GetProperty("cf_boolean").GetBoolean()).IsFalse();
        await Assert.That(json.GetProperty("cf_multiselect").GetArrayLength()).IsEqualTo(2);
        // An explicit null must be sent, otherwise the field can't be cleared.
        await Assert.That(json.GetProperty("cf_cleared").ValueKind).IsEqualTo(JsonValueKind.Null);
    }

    [Test]
    public async Task ClearCustomField_SendsNull()
    {
        var ticket = new Ticket().WithCustomField("cf_text", "hello");

        ticket.ClearCustomField("cf_text");

        await Assert.That(ticket.HasCustomField("cf_text")).IsTrue();
        await Assert.That(ticket.GetCustomField<string>("cf_text")).IsNull();
        await Assert.That(SerializeToElement(ticket).GetProperty("cf_text").ValueKind).IsEqualTo(JsonValueKind.Null);
    }

    [Test]
    public async Task WithCustomField_ReturnsSameInstance()
    {
        var user = new User { Login = "login" };

        var result = user.WithCustomField("cf_text", "hello").WithCustomField("cf_integer", 1);

        await Assert.That(result).IsSameReferenceAs(user);
        await Assert.That(user.GetCustomField<string>("cf_text")).IsEqualTo("hello");
        await Assert.That(user.GetCustomField<int>("cf_integer")).IsEqualTo(1);
    }

    [Test]
    public async Task RemoveCustomField_RemovesField()
    {
        var organization = new Organization
        {
            Name = "Name",
            Shared = false,
            Active = true,
        }.WithCustomField("cf_text", "hello");

        await Assert.That(organization.RemoveCustomField("cf_text")).IsTrue();
        await Assert.That(organization.RemoveCustomField("cf_text")).IsFalse();
        await Assert.That(organization.HasCustomField("cf_text")).IsFalse();
        await Assert.That(new Group { Name = "Name", Active = true }.RemoveCustomField("cf_text")).IsFalse();
    }

    private sealed class TicketFields
    {
        [JsonPropertyName("cf_text")]
        public string? Text { get; set; }

        [JsonPropertyName("cf_integer")]
        public int? Integer { get; set; }

        [JsonPropertyName("cf_multiselect")]
        public List<string>? MultiSelect { get; set; }
    }

    [Test]
    public async Task GetCustomFields_MapsToClass()
    {
        var ticket = Deserialize(
            """
            {
              "id": 1,
              "title": "Title",
              "cf_text": "hello",
              "cf_integer": 42,
              "cf_multiselect": ["a", "b"],
              "cf_unmapped": "ignored"
            }
            """
        );

        var fields = ticket.GetCustomFields<TicketFields>();

        await Assert.That(fields.Text).IsEqualTo("hello");
        await Assert.That(fields.Integer).IsEqualTo(42);
        await Assert.That(fields.MultiSelect).IsEquivalentTo(["a", "b"]);
    }

    [Test]
    public async Task GetCustomFields_WithoutExtensionData_ReturnsEmptyInstance()
    {
        var fields = new Ticket().GetCustomFields<TicketFields>();

        await Assert.That(fields.Text).IsNull();
        await Assert.That(fields.Integer).IsNull();
    }

    [Test]
    public async Task SetCustomFields_MergesProperties()
    {
        var ticket = Deserialize("""{ "cf_existing": "kept", "cf_text": "old" }""");

        ticket.SetCustomFields(new TicketFields { Text = "new", Integer = 7 });

        await Assert.That(ticket.GetCustomField<string>("cf_existing")).IsEqualTo("kept");
        await Assert.That(ticket.GetCustomField<string>("cf_text")).IsEqualTo("new");
        await Assert.That(ticket.GetCustomField<int>("cf_integer")).IsEqualTo(7);
        // null properties are skipped by the client's serializer options
        await Assert.That(ticket.HasCustomField("cf_multiselect")).IsFalse();
    }

    [Test]
    public async Task SetCustomFields_AnonymousType()
    {
        var ticket = new Ticket();

        ticket.SetCustomFields(new { cf_text = "hello", cf_integer = 1 });

        await Assert.That(ticket.GetCustomField<string>("cf_text")).IsEqualTo("hello");
        await Assert.That(ticket.GetCustomField<int>("cf_integer")).IsEqualTo(1);
    }

    [Test]
    public async Task SetCustomFields_NotAnObject_Throws()
    {
        var ticket = new Ticket();

        await Assert.That(() => ticket.SetCustomFields(42)).Throws<ArgumentException>();
    }

    [Test]
    public async Task RoundTrip_PreservesCustomFields()
    {
        var ticket = new Ticket { Title = "Title" }
            .WithCustomField("cf_text", "hello")
            .WithCustomField("cf_integer", 42);

        var json = JsonSerializer.Serialize(ticket, Serialization.Options);
        var result = Deserialize(json);

        await Assert.That(result.Title).IsEqualTo("Title");
        await Assert.That(result.GetCustomField<string>("cf_text")).IsEqualTo("hello");
        await Assert.That(result.GetCustomField<int>("cf_integer")).IsEqualTo(42);
    }
}
