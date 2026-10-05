using System.Text.Json;
using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client.Tests.Core;

public class StringIdListConverterTests
{
    [Test]
    public async Task Read_AcceptsStringsAndNumbers()
    {
        var checklist = JsonSerializer.Deserialize<Checklist>(
            """{"id":1,"sorted_item_ids":["3","1",2]}""",
            Serialization.Options
        );

        await Assert
            .That(checklist!.SortedItemIds)
            .IsEquivalentTo(
                [new ChecklistItemId(3), new ChecklistItemId(1), new ChecklistItemId(2)],
                TUnit.Assertions.Enums.CollectionOrdering.Matching
            );
    }

    [Test]
    public async Task Read_AcceptsNull()
    {
        var checklist = JsonSerializer.Deserialize<Checklist>(
            """{"id":1,"sorted_item_ids":null}""",
            Serialization.Options
        );

        await Assert.That(checklist!.SortedItemIds).IsNull();
    }

    [Test]
    public async Task Read_RejectsInvalidIds()
    {
        await Assert
            .That(() =>
                JsonSerializer.Deserialize<Checklist>("""{"id":1,"sorted_item_ids":["x"]}""", Serialization.Options)
            )
            .Throws<JsonException>();
    }

    [Test]
    public async Task Write_WritesStrings()
    {
        var json = JsonSerializer.Serialize(
            new ChecklistTemplate { SortedItemIds = [new ChecklistTemplateItemId(5), new ChecklistTemplateItemId(4)] },
            Serialization.Options
        );

        await Assert.That(json).Contains("\"sorted_item_ids\":[\"5\",\"4\"]");
    }
}
