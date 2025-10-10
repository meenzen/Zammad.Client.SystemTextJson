using System;
using System.Threading.Tasks;
using Xunit;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[TestCaseOrderer(typeof(TestOrderer))]
public class OrganizationClientTest
{
    private static string randomName = Guid.NewGuid().ToString("N").Substring(0, 8);
    private static int NotFromTestOrganizationCount { get; set; } = 0;
    private static int KrustyBurgerId { get; set; } = 0;
    private static int SpringfieldNuclearPowerPlantId { get; set; } = 0;
    private static int SpringfieldElementarySchoolId { get; set; } = 0;

    [Fact, Order(TestOrder.OrganizationListBefore)]
    public async Task Organization_List_Before_Test()
    {
        var client = TestHelper.Client;

        var organizationList = await client.GetOrganizationListAsync(1, 100);

        Assert.NotNull(organizationList);
        NotFromTestOrganizationCount = organizationList.Count;
    }

    [Fact, Order(TestOrder.OrganizationCreate)]
    public async Task Organization_Create_Test()
    {
        var client = TestHelper.Client;

        var organization1 = await client.CreateOrganizationAsync(
            new Organization
            {
                Name = "Krusty Burger" + randomName,
                Shared = true,
                Domain = "krustyburger.com",
                DomainAssignment = true,
                Active = true,
            }
        );

        var organization2 = await client.CreateOrganizationAsync(
            new Organization
            {
                Name = "Springfield Nuclear Power Plant" + randomName,
                Shared = true,
                Domain = "nuclearpowerplant.com",
                DomainAssignment = true,
                Active = true,
            }
        );

        var organization3 = await client.CreateOrganizationAsync(
            new Organization
            {
                Name = "Springfield Elementary School" + randomName,
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
        var client = TestHelper.Client;

        var organizationList = await client.GetOrganizationListAsync(1, 100);

        Assert.Equal(NotFromTestOrganizationCount + 3, organizationList.Count);
    }

    [Fact, Order(TestOrder.OrganizationDetail)]
    public async Task Organization_Detail_Test()
    {
        var client = TestHelper.Client;

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
        var client = TestHelper.Client;

        await Task.Delay(5000, TestContext.Current.CancellationToken); // Wait for Zammad search indexer
        var organizationSearch = await client.SearchOrganizationAsync("Krusty Burger" + randomName, 20);

        Assert.Single(organizationSearch);
        Assert.Equal(KrustyBurgerId, organizationSearch[0].Id);
    }

    [Fact, Order(TestOrder.OrganizationUpdate)]
    public async Task Organization_Update_Test()
    {
        var client = TestHelper.Client;

        var organization1 = await client.GetOrganizationAsync(SpringfieldElementarySchoolId);
        organization1.Domain = "springfieldelementaryschool.com";

        var organization2 = await client.UpdateOrganizationAsync(SpringfieldElementarySchoolId, organization1);

        Assert.Equal(organization1.Domain, organization2.Domain);
    }

    [Fact, Order(TestOrder.OrganizationDelete)]
    public async Task Organization_Delete_Test()
    {
        var client = TestHelper.Client;

        var organization1 = await client.DeleteOrganizationAsync(KrustyBurgerId);
        var organization2 = await client.DeleteOrganizationAsync(SpringfieldNuclearPowerPlantId);
        var organization3 = await client.DeleteOrganizationAsync(SpringfieldElementarySchoolId);

        Assert.True(organization1);
        Assert.True(organization2);
        Assert.True(organization3);
    }
}
