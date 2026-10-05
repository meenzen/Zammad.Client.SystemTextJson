using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

/// <summary>
/// Manages email signatures. Zammad has no search endpoint for signatures.
/// </summary>
public interface ISignatureService
{
    /// <summary>
    /// Lists signatures.
    /// </summary>
    /// <remarks>
    /// Needs <c>ticket.agent</c> or one of the email channel admin permissions (<c>admin.channel_email</c>,
    /// <c>admin.channel_google</c>, <c>admin.channel_microsoft365</c>, <c>admin.channel_microsoft_graph</c>).
    /// </remarks>
    Task<List<Signature>> ListSignaturesAsync(Pagination? pagination = null);

    Task<Signature?> GetSignatureAsync(SignatureId id);

    /// <summary>
    /// Creates a signature. Needs one of the email channel admin permissions (see <see cref="ListSignaturesAsync"/>).
    /// </summary>
    /// <remarks>
    /// <see cref="Signature.Body"/> is HTML and may contain placeholders such as <c>#{user.firstname}</c>, which
    /// Zammad fills in when the signature is added to an article.
    /// </remarks>
    Task<Signature> CreateSignatureAsync(Signature signature);

    /// <summary>
    /// Updates a signature. Needs one of the email channel admin permissions.
    /// </summary>
    Task<Signature> UpdateSignatureAsync(SignatureId id, Signature signature);

    /// <summary>
    /// Deletes a signature. Needs one of the email channel admin permissions.
    /// </summary>
    Task DeleteSignatureAsync(SignatureId id);
}

public sealed partial class ZammadClient : ISignatureService
{
    private const string SignaturesEndpoint = "/api/v1/signatures";

    public async Task<List<Signature>> ListSignaturesAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<Signature>>(SignaturesEndpoint, builder.ToString()) ?? [];
    }

    public async Task<Signature?> GetSignatureAsync(SignatureId id) =>
        await GetAsync<Signature>($"{SignaturesEndpoint}/{id}");

    public async Task<Signature> CreateSignatureAsync(Signature signature) =>
        await PostAsync<Signature>(SignaturesEndpoint, signature) ?? throw LogicException.UnexpectedNullResult;

    public async Task<Signature> UpdateSignatureAsync(SignatureId id, Signature signature) =>
        await PutAsync<Signature>($"{SignaturesEndpoint}/{id}", signature) ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteSignatureAsync(SignatureId id) => await DeleteAsync<bool>($"{SignaturesEndpoint}/{id}");
}
