using System.Text.Json;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;
using Object = Zammad.Client.Resources.Object;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class ObjectTests(ZammadStackFixture zammadStack)
{
    private static readonly string RandomName = TestSetup.RandomString();
    private static ObjectId CreatedObjectId { get; set; } = ObjectId.Empty;

    [Test]
    public async Task ListObjects()
    {
        var client = await zammadStack.GetClientAsync();

        var objects = await client.ListObjectsAsync();

        await Assert.That(objects).IsNotEmpty();
    }

    [Test]
    [DependsOn(nameof(ListObjects))]
    public async Task GetObject()
    {
        var client = await zammadStack.GetClientAsync();

        var objects = await client.ListObjectsAsync();
        var firstObject = objects[0];

        var obj = await client.GetObjectAsync(firstObject.Id);

        await Assert.That(obj).IsNotNull();
        await Assert.That(obj!.Id).IsEqualTo(firstObject.Id);
        await Assert.That(obj.Name).IsEqualTo(firstObject.Name);
    }

    [Test]
    [DependsOn(nameof(GetObject))]
    public async Task CreateObject()
    {
        // Example payload from https://docs.zammad.org/en/latest/api/object.html
        const string payloadJson = """
            {
               "name": "sample_boolean",
               "object": "Ticket",
               "display": "Sample Boolean",
               "active": false,
               "position": 1550,
               "data_type": "boolean",
               "data_option": {
                  "options": {
                     "true": "very correct indeed",
                     "false": "very incorrect indeed"
                  }
               },
               "screens": {
                  "create_middle": {
                     "ticket.customer": {
                        "shown": true,
                        "required": false,
                        "item_class": "column"
                     },
                     "ticket.agent": {
                        "shown": true,
                        "required": false,
                        "item_class": "column"
                     }
                  },
                  "edit": {
                     "ticket.customer": {
                        "shown": true,
                        "required": false
                     },
                     "ticket.agent": {
                        "shown": true,
                        "required": true
                     }
                  }
               }
            }
            """;
        var payloadObject = JsonSerializer.Deserialize<Object>(payloadJson);
        await Assert.That(payloadObject).IsNotNull();

        var client = await zammadStack.GetClientAsync();
        var obj = await client.CreateObjectAsync(payloadObject);

        await Assert.That(obj).IsNotNull();
        await Assert.That(obj.Id).IsNotEqualTo(ObjectId.Empty);

        CreatedObjectId = obj.Id;
    }

    [Test]
    [DependsOn(nameof(CreateObject))]
    [NotInParallel]
    public async Task ExecuteMigration()
    {
        var client = await zammadStack.GetClientAsync();

        var result = await client.ExecuteMigrationAsync();
        await Assert.That(result).IsTrue();

        await Task.Delay(TimeSpan.FromSeconds(10));
        await zammadStack.RestartAsync();
    }

    [Test]
    [DependsOn(nameof(ExecuteMigration))]
    public async Task UpdateObject()
    {
        const string payloadJson = """
            {
               "id": 50,
               "name": "sample_boolean",
               "object": "Ticket",
               "display": "Sample Boolean",
               "data_type": "boolean",
               "position": 1200,
               "data_option": {
                  "options": {
                     "true": "yes",
                     "false": "no"
                  },
                  "default": "false"
               }
            }
            """;
        var obj = JsonSerializer.Deserialize<Object>(payloadJson);
        await Assert.That(obj).IsNotNull();
        obj.Id = CreatedObjectId;

        var client = await zammadStack.GetClientAsync();

        var updated = await client.UpdateObjectAsync(CreatedObjectId, obj);

        await Assert.That(updated.DataOptionNew).IsNotNull();
    }
}
