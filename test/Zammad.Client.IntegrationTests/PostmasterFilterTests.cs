using System.Net;
using System.Text.Json;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class PostmasterFilterTests(ZammadStackFixture zammadStack)
{
    private static readonly string RandomName = TestSetup.RandomString();
    private static readonly string FilterName = "Test Postmaster Filter " + RandomName;
    private static PostmasterFilterId CreatedFilterId { get; set; } = PostmasterFilterId.Empty;

    private static JsonElement Perform =>
        JsonSerializer.Deserialize<JsonElement>("""{"x-zammad-ticket-priority_id":{"value":"3"}}""");

    [Test]
    public async Task CreatePostmasterFilter()
    {
        var client = await zammadStack.GetClientAsync();

        var filter = await client.CreatePostmasterFilterAsync(
            new PostmasterFilter
            {
                Name = FilterName,
                Channel = "email",
                Active = false,
                Match = JsonSerializer.Deserialize<JsonElement>(
                    $$$"""{"from":{"operator":"contains","value":"{{{RandomName}}}@example.org"}}"""
                ),
                Perform = Perform,
                Note = "created by integration test",
            }
        );

        await Assert.That(filter.Id).IsNotEqualTo(PostmasterFilterId.Empty);
        await Assert.That(filter.Name).IsEqualTo(FilterName);
        await Assert.That(filter.Channel).IsEqualTo("email");
        await Assert.That(filter.Active).IsFalse();
        await Assert
            .That(filter.Match!.Value.GetProperty("from").GetProperty("operator").GetString())
            .IsEqualTo("contains");

        CreatedFilterId = filter.Id;
    }

    [Test]
    public async Task CreatePostmasterFilter_ThrowsWithoutMatch()
    {
        var client = await zammadStack.GetClientAsync();

        var exception = await Assert.ThrowsAsync<ZammadException>(() =>
            client.CreatePostmasterFilterAsync(
                new PostmasterFilter
                {
                    Name = "Invalid Postmaster Filter " + RandomName,
                    Channel = "email",
                    Active = false,
                    Perform = Perform,
                }
            )
        );

        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.UnprocessableEntity);
        await Assert.That(exception.Error).IsEqualTo("At least one match rule is required, but none was provided.");
    }

    [Test]
    [DependsOn(nameof(CreatePostmasterFilter))]
    public async Task ListPostmasterFilters()
    {
        var client = await zammadStack.GetClientAsync();

        var filters = await client.ListPostmasterFiltersAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(filters).Contains(f => f.Id == CreatedFilterId);
    }

    [Test]
    [DependsOn(nameof(ListPostmasterFilters))]
    public async Task GetPostmasterFilter()
    {
        var client = await zammadStack.GetClientAsync();

        var filter = await client.GetPostmasterFilterAsync(CreatedFilterId);

        await Assert.That(filter).IsNotNull();
        await Assert.That(filter!.Name).IsEqualTo(FilterName);
        await Assert.That(filter.Note).IsEqualTo("created by integration test");
    }

    [Test]
    [DependsOn(nameof(GetPostmasterFilter))]
    public async Task UpdatePostmasterFilter()
    {
        var client = await zammadStack.GetClientAsync();

        var filter = await client.GetPostmasterFilterAsync(CreatedFilterId);
        await Assert.That(filter).IsNotNull();
        filter!.Match = JsonSerializer.Deserialize<JsonElement>(
            $$$"""{"subject":{"operator":"matches regex","value":"^{{{RandomName}}}"}}"""
        );

        var updated = await client.UpdatePostmasterFilterAsync(CreatedFilterId, filter);

        await Assert
            .That(updated.Match!.Value.GetProperty("subject").GetProperty("operator").GetString())
            .IsEqualTo("matches regex");
        await Assert.That(updated.Match.Value.TryGetProperty("from", out _)).IsFalse();
    }

    [Test]
    [DependsOn(nameof(UpdatePostmasterFilter))]
    public async Task DeletePostmasterFilter()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeletePostmasterFilterAsync(CreatedFilterId);

        await Assert.That(await client.GetPostmasterFilterAsync(CreatedFilterId)).IsNull();
    }
}
