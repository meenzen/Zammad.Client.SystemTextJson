using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Zammad.Client.Core.Protocol;

#nullable enable

public class TokenHttpClientHandler : HttpClientHandlerBase
{
    private readonly AuthenticationHeaderValue _authenticationHeader;

    public TokenHttpClientHandler(string token, string onBehalfOf)
        : base(onBehalfOf)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentOutOfRangeException(nameof(token));
        }

        _authenticationHeader = CreateAuthenticationHeader(token);
    }

    private static AuthenticationHeaderValue CreateAuthenticationHeader(string token) =>
        new AuthenticationHeaderValue("Token", $"token={token}");

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        request.Headers.Authorization = _authenticationHeader;
        return base.SendAsync(request, cancellationToken);
    }
}
