using System.Threading;
using System.Threading.Tasks;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class OrganizationTests(ZammadStackFixture zammadStack)
{
    private static readonly string RandomName = TestSetup.RandomString();
    private static OrganizationId KrustyBurgerId { get; set; } = OrganizationId.Empty;
    private static OrganizationId SpringfieldNuclearPowerPlantId { get; set; } = OrganizationId.Empty;
    private static OrganizationId SpringfieldElementarySchoolId { get; set; } = OrganizationId.Empty;

    [Test]
    public async Task CreateOrganization()
    {
        var client = await zammadStack.GetClientAsync();

        var organization1 = await client.CreateOrganizationAsync(
            new Organization
            {
                Name = "Krusty Burger" + RandomName,
                Shared = true,
                Domain = "krustyburger.com",
                DomainAssignment = true,
                Active = true,
            }
        );

        var organization2 = await client.CreateOrganizationAsync(
            new Organization
            {
                Name = "Springfield Nuclear Power Plant" + RandomName,
                Shared = true,
                Domain = "nuclearpowerplant.com",
                DomainAssignment = true,
                Active = true,
            }
        );

        var organization3 = await client.CreateOrganizationAsync(
            new Organization
            {
                Name = "Springfield Elementary School" + RandomName,
                Shared = true,
                Domain = "springfield-elementaryschool.com",
                DomainAssignment = true,
                Active = true,
            }
        );

        await Assert.That(organization1).IsNotNull();
        await Assert.That(organization2).IsNotNull();
        await Assert.That(organization3).IsNotNull();

        KrustyBurgerId = organization1.Id;
        SpringfieldNuclearPowerPlantId = organization2.Id;
        SpringfieldElementarySchoolId = organization3.Id;
    }

    [Test]
    [DependsOn(nameof(CreateOrganization))]
    public async Task ListOrganizations()
    {
        var client = await zammadStack.GetClientAsync();

        var organizationList = await client.ListOrganizationsAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(organizationList).HasAtLeast(3);
        await Assert.That(organizationList).Contains(o => o.Id == KrustyBurgerId);
        await Assert.That(organizationList).Contains(o => o.Id == SpringfieldNuclearPowerPlantId);
        await Assert.That(organizationList).Contains(o => o.Id == SpringfieldElementarySchoolId);
    }

    [Test]
    [DependsOn(nameof(ListOrganizations))]
    public async Task OrganizationDetails()
    {
        var client = await zammadStack.GetClientAsync();

        var organization = await client.GetOrganizationAsync(KrustyBurgerId);

        await Assert.That(organization).IsNotNull();
        await Assert.That(organization.Id).IsEqualTo(KrustyBurgerId);
        await Assert.That(organization.Name).StartsWith("Krusty Burger");
        await Assert.That(organization.Shared).IsTrue();
        await Assert.That(organization.Domain).IsEqualTo("krustyburger.com");
        await Assert.That(organization.DomainAssignment).IsTrue();
        await Assert.That(organization.Active).IsTrue();
    }

    [Test]
    [DependsOn(nameof(OrganizationDetails))]
    public async Task SearchOrganizations(CancellationToken cancellationToken)
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, cancellationToken);
        var organizationSearch = await client.SearchOrganizationsAsync(
            new SearchQuery
            {
                Query = "Krusty Burger" + RandomName,
                Pagination = new Pagination { PerPage = 20 },
            }
        );

        await Assert.That(organizationSearch).HasSingleItem();
        await Assert.That(organizationSearch[0].Id).IsEqualTo(KrustyBurgerId);
    }

    [Test]
    [DependsOn(nameof(SearchOrganizations))]
    public async Task UpdateOrganization()
    {
        var client = await zammadStack.GetClientAsync();

        var organization1 = await client.GetOrganizationAsync(SpringfieldElementarySchoolId);
        await Assert.That(organization1).IsNotNull();
        organization1.Domain = "springfieldelementaryschool.com";

        var organization2 = await client.UpdateOrganizationAsync(SpringfieldElementarySchoolId, organization1);

        await Assert.That(organization2.Domain).IsEqualTo(organization1.Domain);
    }

    [Test]
    [DependsOn(nameof(UpdateOrganization))]
    public async Task DeleteOrganization()
    {
        var client = await zammadStack.GetClientAsync();

        var organization1 = await client.DeleteOrganizationAsync(KrustyBurgerId);
        var organization2 = await client.DeleteOrganizationAsync(SpringfieldNuclearPowerPlantId);
        var organization3 = await client.DeleteOrganizationAsync(SpringfieldElementarySchoolId);

        await Assert.That(organization1).IsTrue();
        await Assert.That(organization2).IsTrue();
        await Assert.That(organization3).IsTrue();
    }
}
