using Zammad.Client.Core;
using Zammad.Client.Resources;

namespace Zammad.Client;

public interface IEmailAddressService
{
    Task<List<EmailAddress>> ListEmailAddressesAsync(Pagination? pagination = null);
    Task<EmailAddress?> GetEmailAddressAsync(EmailAddressId id);
    Task<EmailAddress> CreateEmailAddressAsync(EmailAddress emailAddress);
    Task<EmailAddress> UpdateEmailAddressAsync(EmailAddressId id, EmailAddress emailAddress);
    Task DeleteEmailAddressAsync(EmailAddressId id);
}

public sealed partial class ZammadClient : IEmailAddressService
{
    private const string EmailAddressesEndpoint = "/api/v1/email_addresses";

    public async Task<List<EmailAddress>> ListEmailAddressesAsync(Pagination? pagination = null)
    {
        var builder = new QueryBuilder();
        builder.AddPagination(pagination);
        return await GetAsync<List<EmailAddress>>(EmailAddressesEndpoint, builder.ToString()) ?? [];
    }

    public async Task<EmailAddress?> GetEmailAddressAsync(EmailAddressId id) =>
        await GetAsync<EmailAddress>($"{EmailAddressesEndpoint}/{id}");

    public async Task<EmailAddress> CreateEmailAddressAsync(EmailAddress emailAddress) =>
        await PostAsync<EmailAddress>(EmailAddressesEndpoint, emailAddress)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task<EmailAddress> UpdateEmailAddressAsync(EmailAddressId id, EmailAddress emailAddress) =>
        await PutAsync<EmailAddress>($"{EmailAddressesEndpoint}/{id}", emailAddress)
        ?? throw LogicException.UnexpectedNullResult;

    public async Task DeleteEmailAddressAsync(EmailAddressId id) =>
        await DeleteAsync<bool>($"{EmailAddressesEndpoint}/{id}");
}
