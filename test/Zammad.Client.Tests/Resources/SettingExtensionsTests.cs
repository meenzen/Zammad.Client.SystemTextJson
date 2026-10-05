using System.Text.Json;
using Zammad.Client.Core;
using Zammad.Client.Resources;
using Zammad.Client.Resources.Internal;

namespace Zammad.Client.Tests.Resources;

public class SettingExtensionsTests
{
    private static Setting Deserialize(string json) =>
        JsonSerializer.Deserialize<Setting>(json, Serialization.Options) ?? throw new InvalidOperationException();

    [Test]
    public async Task GetValue_ReadsTypedValues()
    {
        var setting = Deserialize(
            """{"id":1,"state_current":{"value":{"enabled":true}},"state_initial":{"value":"x"}}"""
        );

        await Assert.That(setting.GetValue<Dictionary<string, bool>>()!["enabled"]).IsTrue();
        await Assert.That(setting.GetInitialValue<string>()).IsEqualTo("x");
    }

    [Test]
    public async Task GetValue_ReturnsDefaultWithoutValue()
    {
        var setting = Deserialize("""{"id":1,"state_current":{},"state_initial":{"value":null}}""");

        await Assert.That(setting.GetValue<string>()).IsNull();
        await Assert.That(setting.GetInitialValue<int?>()).IsNull();
        await Assert.That(new Setting().GetValue<string>()).IsNull();
    }

    [Test]
    public async Task SetValue_WrapsValue()
    {
        var setting = new Setting();

        setting.SetValue(42);
        await Assert.That(setting.StateCurrent!.Value.GetRawText()).IsEqualTo("""{"value":42}""");
        await Assert.That(setting.GetValue<int>()).IsEqualTo(42);

        setting.SetValue<string?>(null);
        await Assert.That(setting.StateCurrent!.Value.GetRawText()).IsEqualTo("""{"value":null}""");
    }

    [Test]
    public async Task SettingValueRequest_SendsExplicitNull()
    {
        var json = JsonSerializer.Serialize(new SettingValueRequest<string?>(null), Serialization.Options);

        await Assert.That(json).IsEqualTo("""{"state_current":{"value":null}}""");
    }
}
