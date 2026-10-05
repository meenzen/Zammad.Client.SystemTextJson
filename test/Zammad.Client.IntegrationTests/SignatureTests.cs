using Zammad.Client.Core;
using Zammad.Client.IntegrationTests.Infrastructure;
using Zammad.Client.IntegrationTests.Setup;
using Zammad.Client.Resources;

namespace Zammad.Client.IntegrationTests;

[ClassDataSource<ZammadStackFixture>(Shared = SharedType.PerAssembly)]
public class SignatureTests(ZammadStackFixture zammadStack)
{
    private static readonly string RandomName = TestSetup.RandomString();
    private static readonly string SignatureName = "TestSignature" + RandomName;
    private static SignatureId CreatedSignatureId { get; set; } = SignatureId.Empty;

    [Test]
    public async Task CreateSignature()
    {
        var client = await zammadStack.GetClientAsync();

        var signature = await client.CreateSignatureAsync(
            new Signature
            {
                Name = SignatureName,
                Body = "<p>Kind regards</p><p>#{user.firstname}</p>",
                Note = "Test note",
                Active = true,
            }
        );

        await Assert.That(signature.Id).IsNotEqualTo(SignatureId.Empty);
        await Assert.That(signature.Name).IsEqualTo(SignatureName);
        await Assert.That(signature.Body).IsEqualTo("<p>Kind regards</p><p>#{user.firstname}</p>");
        await Assert.That(signature.GroupIds).IsEmpty();

        CreatedSignatureId = signature.Id;
    }

    [Test]
    [DependsOn(nameof(CreateSignature))]
    public async Task ListSignatures()
    {
        var client = await zammadStack.GetClientAsync();

        var signatures = await client.ListSignaturesAsync(new Pagination { Page = 1, PerPage = 100 });

        await Assert.That(signatures).Contains(s => s.Id == CreatedSignatureId);
    }

    [Test]
    public async Task ListSignatures_ContainsDefaultSignatureUsedByGroup()
    {
        var client = await zammadStack.GetClientAsync();

        // the auto wizard assigns the seeded "default" signature to the "Users" group
        var signatures = await client.ListSignaturesAsync();
        var group = await client.GetGroupAsync(new GroupId(1));

        await Assert.That(group?.SignatureId).IsNotNull();
        var signature = signatures.Single(s => s.Id == group!.SignatureId);
        await Assert.That(signature.Name).IsEqualTo("default");
        await Assert.That(signature.GroupIds).Contains(group!.Id);
    }

    [Test]
    [DependsOn(nameof(CreateSignature))]
    public async Task GetSignature()
    {
        var client = await zammadStack.GetClientAsync();

        var signature = await client.GetSignatureAsync(CreatedSignatureId);

        await Assert.That(signature).IsNotNull();
        await Assert.That(signature!.Name).IsEqualTo(SignatureName);
        await Assert.That(signature.Note).IsEqualTo("Test note");
        await Assert.That(signature.Active).IsTrue();
    }

    [Test]
    [DependsOn(nameof(CreateSignature))]
    public async Task GetSignature_AsAgent()
    {
        var client = await zammadStack.GetClientOnBehalfOfAsync("agent1@example.org");

        await Assert.That(await client.GetSignatureAsync(CreatedSignatureId)).IsNotNull();
    }

    [Test]
    [DependsOn(nameof(ListSignatures))]
    [DependsOn(nameof(GetSignature))]
    [DependsOn(nameof(GetSignature_AsAgent))]
    public async Task UpdateSignature()
    {
        var client = await zammadStack.GetClientAsync();

        var signature = await client.GetSignatureAsync(CreatedSignatureId);
        await Assert.That(signature).IsNotNull();
        signature!.Body = "<p>Updated</p>";
        signature.Active = false;

        var updated = await client.UpdateSignatureAsync(CreatedSignatureId, signature);

        await Assert.That(updated.Body).IsEqualTo("<p>Updated</p>");
        await Assert.That(updated.Active).IsFalse();
    }

    [Test]
    [DependsOn(nameof(UpdateSignature))]
    public async Task DeleteSignature()
    {
        var client = await zammadStack.GetClientAsync();

        await client.DeleteSignatureAsync(CreatedSignatureId);
        await Assert.That(await client.GetSignatureAsync(CreatedSignatureId)).IsNull();
    }
}
