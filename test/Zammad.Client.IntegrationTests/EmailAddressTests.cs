using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class EmailAddressTests(ZammadStackFixture zammadStack)
{
    private static readonly string RandomName = TestSetup.RandomString();
    private static readonly string Name = "Test Email Address " + RandomName;
    private static readonly string Email = $"test-{RandomName}@example.org".ToLowerInvariant();
    private static EmailAddressId CreatedEmailAddressId { get; set; } = EmailAddressId.Empty;

    [Test]
    public async Task CreateEmailAddress()
    {
        var client = await zammadStack.GetClientAsync();

        var emailAddress = await client.CreateEmailAddressAsync(new EmailAddress { Name = Name, Email = Email });

        await Assert.That(emailAddress).IsNotNull();
        await Assert.That(emailAddress.Id).IsNotEqualTo(EmailAddressId.Empty);
        await Assert.That(emailAddress.Name).IsEqualTo(Name);
        await Assert.That(emailAddress.Email).IsEqualTo(Email);

        CreatedEmailAddressId = emailAddress.Id;
    }

    [Test]
    [DependsOn(nameof(CreateEmailAddress))]
    public async Task ListEmailAddresses()
    {
        var client = await zammadStack.GetClientAsync();

        var emailAddresses = await client.ListEmailAddressesAsync();

        await Assert.That(emailAddresses).HasAtLeast(1);
        await Assert.That(emailAddresses).Contains(e => e.Id == CreatedEmailAddressId);
    }

    [Test]
    [DependsOn(nameof(CreateEmailAddress))]
    public async Task ListEmailAddresses_Pagination()
    {
        var client = await zammadStack.GetClientAsync();

        var emailAddresses = await client.ListEmailAddressesAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(emailAddresses).HasAtLeast(1);
        await Assert.That(emailAddresses).Contains(e => e.Id == CreatedEmailAddressId);
    }

    [Test]
    [DependsOn(nameof(ListEmailAddresses))]
    [DependsOn(nameof(ListEmailAddresses_Pagination))]
    public async Task GetEmailAddress()
    {
        var client = await zammadStack.GetClientAsync();

        var emailAddress = await client.GetEmailAddressAsync(CreatedEmailAddressId);

        await Assert.That(emailAddress).IsNotNull();
        await Assert.That(emailAddress!.Id).IsEqualTo(CreatedEmailAddressId);
        await Assert.That(emailAddress.Name).IsEqualTo(Name);
        await Assert.That(emailAddress.Email).IsEqualTo(Email);
    }

    [Test]
    [DependsOn(nameof(GetEmailAddress))]
    public async Task UpdateEmailAddress()
    {
        var client = await zammadStack.GetClientAsync();

        var emailAddress = await client.GetEmailAddressAsync(CreatedEmailAddressId);
        await Assert.That(emailAddress).IsNotNull();
        emailAddress!.Note = "Updated note";

        var updated = await client.UpdateEmailAddressAsync(CreatedEmailAddressId, emailAddress);

        await Assert.That(updated.Note).IsEqualTo("Updated note");
    }

    [Test]
    [DependsOn(nameof(UpdateEmailAddress))]
    public async Task DeleteEmailAddress()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteEmailAddressAsync(CreatedEmailAddressId);
        await Assert.That(await client.GetEmailAddressAsync(CreatedEmailAddressId)).IsNull();
    }
}
