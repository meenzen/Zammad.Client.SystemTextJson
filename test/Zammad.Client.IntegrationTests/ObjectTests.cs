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
        var client = await zammadStack.GetClientAsync();

        var obj = await client.CreateObjectAsync(
            new Object
            {
                Name = "custom_attr_" + RandomName.ToLower(),
                Display = "Custom Attribute " + RandomName,
                DataType = "input",
                Active = true,
            }
        );

        await Assert.That(obj).IsNotNull();
        await Assert.That(obj.Id).IsNotEqualTo(ObjectId.Empty);
        await Assert.That(obj.Name).StartsWith("custom_attr_");

        CreatedObjectId = obj.Id;
    }

    [Test]
    [DependsOn(nameof(CreateObject))]
    public async Task UpdateObject()
    {
        var client = await zammadStack.GetClientAsync();

        var obj = await client.GetObjectAsync(CreatedObjectId);
        await Assert.That(obj).IsNotNull();
        obj!.Display = "Updated Custom Attribute " + RandomName;

        var updated = await client.UpdateObjectAsync(CreatedObjectId, obj);

        await Assert.That(updated.Display).IsEqualTo(obj.Display);
    }

    [Test]
    [DependsOn(nameof(UpdateObject))]
    public async Task ExecuteMigration()
    {
        var client = await zammadStack.GetClientAsync();

        var result = await client.ExecuteMigrationAsync();

        await Assert.That(result).IsTrue();
    }
}
