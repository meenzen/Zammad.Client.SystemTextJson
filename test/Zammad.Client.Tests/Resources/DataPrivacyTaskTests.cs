using System.Text.Json;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Tests.Deserialization;

namespace Zammad.Client.Tests.Resources;

public class DataPrivacyTaskTests
{
    [Test]
    public async Task TypedAccessors_ReadPreferences()
    {
        var json = await TestFile.ReadStringAsync("../Deserialization/Responses", "dataPrivacyTask.json");
        var task = JsonSerializer.Deserialize<DataPrivacyTask>(json, Serialization.Options)!;

        await Assert.That(task.DeletableId).IsEqualTo(new UserId(5).ToTargetObjectId());
        await Assert.That(task.CustomerTickets).IsEquivalentTo(["87003"]);
        await Assert.That(task.CustomerTicketsCount).IsEqualTo(1);
        await Assert.That(task.OwnerTickets).IsEmpty();
        await Assert.That(task.OwnerTicketsCount).IsEqualTo(0);
        await Assert.That(task.Error).IsNull();
    }

    [Test]
    public async Task TypedAccessors_ReturnDefaultWithoutPreferences()
    {
        var task = new DataPrivacyTask();

        await Assert.That(task.CustomerTickets).IsNull();
        await Assert.That(task.CustomerTicketsCount).IsNull();
        await Assert.That(task.Error).IsNull();
    }
}
