using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zammad.Client.Core.Protocol;

#nullable enable

public class BasicHttpClientHandler : HttpClientHandlerBase
{
    private readonly AuthenticationHeaderValue _authenticationHeader;

    public BasicHttpClientHandler(string user, string password, string onBehalfOf)
        : base(onBehalfOf)
    {
        if (string.IsNullOrEmpty(user))
        {
            throw new ArgumentOutOfRangeException(nameof(user));
        }

        if (string.IsNullOrEmpty(password))
        {
            throw new ArgumentOutOfRangeException(nameof(password));
        }

        _authenticationHeader = CreateAuthenticationHeader(user, password);
    }

    private static AuthenticationHeaderValue CreateAuthenticationHeader(string user, string password) =>
        new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes($"{user}:{password}"))
        );

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        request.Headers.Authorization = _authenticationHeader;
        return base.SendAsync(request, cancellationToken);
    }
}
