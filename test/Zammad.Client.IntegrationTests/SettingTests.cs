using System.Net;
using System.Text.Json;
using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

/// <summary>
/// Settings are global and all tests share one Zammad instance. These tests only change <c>organization</c>, the
/// organization name shown in the web app and in email footers, which no other test depends on, and reset it at the
/// end. Its default is an empty string.
/// </summary>
[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class SettingTests(ZammadStackFixture zammadStack)
{
    private const string SettingName = "organization";
    private const string SettingArea = "System::Branding";

    private static readonly string RandomName = TestSetup.RandomString();
    private static SettingId SettingId { get; set; } = SettingId.Empty;

    [Test]
    public async Task ListSettings()
    {
        var client = await zammadStack.GetClientAsync();

        var settings = await client.ListSettingsAsync();

        // Set by Setup/autowizard.json
        var productName = settings.Find(s => s.Name == "product_name");
        await Assert.That(productName).IsNotNull();
        await Assert.That(productName!.GetValue<string>()).IsEqualTo("Zammad Test System");
        await Assert.That(productName.GetInitialValue<string>()).IsEqualTo("Zammad Helpdesk");

        // Protected settings aren't listed
        await Assert.That(settings).DoesNotContain(s => s.Name == "application_secret");

        // Sensitive values are masked
        var ldap = settings.Find(s => s.Name == "auth_ldap");
        await Assert.That(ldap).IsNotNull();
        await Assert
            .That(ldap!.GetValue<Dictionary<string, JsonElement>>()!["bind_pw"].GetString())
            .IsEqualTo("**********");
    }

    [Test]
    public async Task ListSettingsByArea()
    {
        var client = await zammadStack.GetClientAsync();

        var settings = await client.ListSettingsByAreaAsync(SettingArea);

        await Assert.That(settings).Contains(s => s.Name == SettingName);
        await Assert.That(settings).All(s => s.Area == SettingArea);
    }

    [Test]
    public async Task GetSettingByName()
    {
        var client = await zammadStack.GetClientAsync();

        var setting = await client.GetSettingByNameAsync(SettingName);

        await Assert.That(setting).IsNotNull();
        await Assert.That(setting!.Area).IsEqualTo(SettingArea);
        SettingId = setting.Id;
    }

    [Test]
    public async Task GetSettingByName_ReturnsNullForUnknownOrProtectedSetting()
    {
        var client = await zammadStack.GetClientAsync();

        await Assert.That(await client.GetSettingByNameAsync("does_not_exist_" + RandomName)).IsNull();
        await Assert.That(await client.GetSettingByNameAsync("application_secret")).IsNull();
    }

    [Test]
    [DependsOn(nameof(GetSettingByName))]
    public async Task GetSetting()
    {
        var client = await zammadStack.GetClientAsync();

        var setting = await client.GetSettingAsync(SettingId);

        await Assert.That(setting).IsNotNull();
        await Assert.That(setting!.Id).IsEqualTo(SettingId);
        await Assert.That(setting.Name).IsEqualTo(SettingName);
        await Assert.That(setting.GetInitialValue<string>()).IsEqualTo("");
    }

    [Test]
    public async Task GetSetting_ThrowsForMissingSetting()
    {
        var client = await zammadStack.GetClientAsync();

        var exception = await Assert.ThrowsAsync<ZammadException>(() => client.GetSettingAsync(new SettingId(999_999)));

        // Not a 404: the controller policy fails to authorize a setting it didn't find
        await Assert.That(exception!.Code).IsEqualTo(HttpStatusCode.InternalServerError);
        await Assert.That(exception.Error).IsEqualTo("unable to find policy `NilClassPolicy` for `nil`");
    }

    [Test]
    [DependsOn(nameof(GetSetting))]
    public async Task UpdateSettingValue()
    {
        var client = await zammadStack.GetClientAsync();
        var value = "Value Org " + RandomName;

        var updated = await client.UpdateSettingValueAsync(SettingId, value);

        await Assert.That(updated.Id).IsEqualTo(SettingId);
        await Assert.That(updated.GetValue<string>()).IsEqualTo(value);
        var setting = await client.GetSettingAsync(SettingId);
        await Assert.That(setting!.GetValue<string>()).IsEqualTo(value);
    }

    [Test]
    [DependsOn(nameof(UpdateSettingValue))]
    public async Task UpdateSetting()
    {
        var client = await zammadStack.GetClientAsync();
        var setting = await client.GetSettingAsync(SettingId);
        await Assert.That(setting).IsNotNull();
        var value = "Raw Org " + RandomName;
        setting!.SetValue(value);

        var updated = await client.UpdateSettingAsync(SettingId, setting);

        await Assert.That(updated.GetValue<string>()).IsEqualTo(value);
        await Assert.That(updated.Name).IsEqualTo(SettingName);
    }

    [Test]
    [DependsOn(nameof(UpdateSetting))]
    public async Task ResetSetting()
    {
        var client = await zammadStack.GetClientAsync();

        var reset = await client.ResetSettingAsync(SettingId);

        await Assert.That(reset.GetValue<string>()).IsEqualTo("");
        var setting = await client.GetSettingAsync(SettingId);
        await Assert.That(setting!.GetValue<string>()).IsEqualTo("");
    }
}
