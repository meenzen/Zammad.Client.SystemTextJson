using System.Threading.Tasks;
using Xunit;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[TestCaseOrderer(typeof(TestOrderer))]
public class OrganizationTests(ZammadStackFixture zammadStack)
{
    private static readonly string RandomName = TestSetup.RandomString();
    private static int NotFromTestOrganizationCount { get; set; } = 0;
    private static OrganizationId KrustyBurgerId { get; set; } = OrganizationId.Empty;
    private static OrganizationId SpringfieldNuclearPowerPlantId { get; set; } = OrganizationId.Empty;
    private static OrganizationId SpringfieldElementarySchoolId { get; set; } = OrganizationId.Empty;

    [Fact, Order(TestOrder.OrganizationListBefore)]
    public async Task Organization_List_Before_Test()
    {
        var client = await zammadStack.GetClientAsync();

        var organizationList = await client.ListOrganizationsAsync(1, 100);

        Assert.NotNull(organizationList);
        NotFromTestOrganizationCount = organizationList.Count;
    }

    [Fact, Order(TestOrder.OrganizationCreate)]
    public async Task Organization_Create_Test()
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

        Assert.NotNull(organization1);
        Assert.NotNull(organization2);
        Assert.NotNull(organization3);

        KrustyBurgerId = organization1.Id;
        SpringfieldNuclearPowerPlantId = organization2.Id;
        SpringfieldElementarySchoolId = organization3.Id;
    }

    [Fact, Order(TestOrder.OrganizationList)]
    public async Task Organization_List_Test()
    {
        var client = await zammadStack.GetClientAsync();

        var organizationList = await client.ListOrganizationsAsync(1, 100);

        Assert.Equal(NotFromTestOrganizationCount + 3, organizationList.Count);
    }

    [Fact, Order(TestOrder.OrganizationDetail)]
    public async Task Organization_Detail_Test()
    {
        var client = await zammadStack.GetClientAsync();

        var organization = await client.GetOrganizationAsync(KrustyBurgerId);

        Assert.Equal(KrustyBurgerId, organization.Id);
        Assert.StartsWith("Krusty Burger", organization.Name);
        Assert.True(organization.Shared);
        Assert.Equal("krustyburger.com", organization.Domain);
        Assert.True(organization.DomainAssignment);
        Assert.True(organization.Active);
    }

    [Fact, Order(TestOrder.OrganizationSearch)]
    public async Task Organization_Search_Test()
    {
        var client = await zammadStack.GetClientAsync();

        await Task.Delay(TestSetup.IndexerDelay, TestContext.Current.CancellationToken);
        var organizationSearch = await client.SearchOrganizationsAsync("Krusty Burger" + RandomName, 20);

        Assert.Single(organizationSearch);
        Assert.Equal(KrustyBurgerId, organizationSearch[0].Id);
    }

    [Fact, Order(TestOrder.OrganizationUpdate)]
    public async Task Organization_Update_Test()
    {
        var client = await zammadStack.GetClientAsync();

        var organization1 = await client.GetOrganizationAsync(SpringfieldElementarySchoolId);
        organization1.Domain = "springfieldelementaryschool.com";

        var organization2 = await client.UpdateOrganizationAsync(SpringfieldElementarySchoolId, organization1);

        Assert.Equal(organization1.Domain, organization2.Domain);
    }

    [Fact, Order(TestOrder.OrganizationDelete)]
    public async Task Organization_Delete_Test()
    {
        var client = await zammadStack.GetClientAsync();

        var organization1 = await client.DeleteOrganizationAsync(KrustyBurgerId);
        var organization2 = await client.DeleteOrganizationAsync(SpringfieldNuclearPowerPlantId);
        var organization3 = await client.DeleteOrganizationAsync(SpringfieldElementarySchoolId);

        Assert.True(organization1);
        Assert.True(organization2);
        Assert.True(organization3);
    }
}