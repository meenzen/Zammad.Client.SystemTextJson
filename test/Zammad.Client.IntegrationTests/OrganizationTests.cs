using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class OrganizationTests(ZammadStackFixture zammadStack)
{
    private static readonly string RandomName = TestSetup.RandomString();
    private static readonly string OrganizationName = "Krusty Burger" + RandomName;
    private static OrganizationId KrustyBurgerId { get; set; } = OrganizationId.Empty;

    [Test]
    public async Task CreateOrganization()
    {
        var client = await zammadStack.GetClientAsync();

        var organization = await client.CreateOrganizationAsync(
            new Organization
            {
                Name = OrganizationName,
                Shared = true,
                Domain = "krustyburger.com",
                DomainAssignment = true,
                Active = true,
            }
        );

        await Assert.That(organization).IsNotNull();

        KrustyBurgerId = organization.Id;
    }

    [Test]
    [DependsOn(nameof(CreateOrganization))]
    public async Task ListOrganizations()
    {
        var client = await zammadStack.GetClientAsync();

        var organizationList = await client.ListOrganizationsAsync();

        await Assert.That(organizationList).HasAtLeast(1);
        await Assert.That(organizationList).Contains(o => o.Id == KrustyBurgerId);
    }

    [Test]
    [DependsOn(nameof(CreateOrganization))]
    public async Task ListOrganizations_Pagination()
    {
        var client = await zammadStack.GetClientAsync();

        var organizationList = await client.ListOrganizationsAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(organizationList).HasAtLeast(1);
        await Assert.That(organizationList).Contains(o => o.Id == KrustyBurgerId);
    }

    [Test]
    [DependsOn(nameof(CreateOrganization))]
    public async Task GetOrganization()
    {
        var client = await zammadStack.GetClientAsync();

        var organization = await client.GetOrganizationAsync(KrustyBurgerId);

        await Assert.That(organization).IsNotNull();
        await Assert.That(organization.Id).IsEqualTo(KrustyBurgerId);
        await Assert.That(organization.Name).IsEqualTo(OrganizationName);
        await Assert.That(organization.Shared).IsTrue();
        await Assert.That(organization.Domain).IsEqualTo("krustyburger.com");
        await Assert.That(organization.DomainAssignment).IsTrue();
        await Assert.That(organization.Active).IsTrue();
    }

    [Test]
    [DependsOn(nameof(CreateOrganization))]
    [Retry(TestSetup.RetryCount, BackoffMs = TestSetup.BackoffMs)]
    public async Task SearchOrganizations(CancellationToken cancellationToken)
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, cancellationToken);
        var organizationSearch = await client.SearchOrganizationsAsync(
            new SearchQuery
            {
                Query = OrganizationName,
                Pagination = new Pagination { PerPage = 20 },
            }
        );

        await Assert.That(organizationSearch).HasSingleItem();
        await Assert.That(organizationSearch[0].Id).IsEqualTo(KrustyBurgerId);
    }

    [Test]
    [DependsOn(nameof(CreateOrganization))]
    [DependsOn(nameof(ListOrganizations))]
    [DependsOn(nameof(ListOrganizations_Pagination))]
    [DependsOn(nameof(GetOrganization))]
    [DependsOn(nameof(SearchOrganizations))]
    public async Task UpdateOrganization()
    {
        var client = await zammadStack.GetClientAsync();

        var organization = await client.GetOrganizationAsync(KrustyBurgerId);
        await Assert.That(organization).IsNotNull();
        organization.Domain = "krustyburger.org";

        var organization2 = await client.UpdateOrganizationAsync(KrustyBurgerId, organization);

        await Assert.That(organization2.Domain).IsEqualTo(organization.Domain);
    }

    [Test]
    [DependsOn(nameof(UpdateOrganization))]
    public async Task DeleteOrganization()
    {
        var client = await zammadStack.GetClientAsync();

        var organization1 = await client.DeleteOrganizationAsync(KrustyBurgerId);

        await Assert.That(organization1).IsTrue();
    }
}
