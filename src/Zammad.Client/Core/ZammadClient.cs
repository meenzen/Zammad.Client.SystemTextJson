using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Zammad.Client.Core.Protocol;

namespace Zammad.Client.Core;

public abstract class ZammadClient
{
    private readonly ZammadAccount _account;

    protected ZammadClient(ZammadAccount account) =>
        _account = account ?? throw new ArgumentNullException(nameof(account));

    protected async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest)
    {
        using (var httpClient = CreateHttpClient())
        {
            var httpResponse = await httpClient.SendAsync(httpRequest);
            if (!httpResponse.IsSuccessStatusCode)
            {
                throw new ZammadException(httpRequest, httpResponse);
            }

            return httpResponse;
        }
    }

    private HttpClient CreateHttpClient() => new HttpClient(CreateHttpHandler());

    private HttpClientHandler CreateHttpHandler()
    {
        switch (_account.Authentication)
        {
            case ZammadAuthentication.Basic:
                return new BasicHttpClientHandler(_account.User, _account.Password, _account.OnBehalfOf);
            case ZammadAuthentication.Token:
                return new TokenHttpClientHandler(_account.Token, _account.OnBehalfOf);
            default:
                throw new NotImplementedException();
        }
    }

    protected async Task<TResult> GetAsync<TResult>(string path, string query = null)
    {
        var builder = new UriBuilder(new Uri(_account.Endpoint, path));
        if (query is not null)
        {
            builder.Query = query;
        }

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, builder.Uri);

        var httpResponse = await SendAsync(httpRequest);

        var result = await httpResponse.ParseAsync<TResult>();

        return result;
    }

    protected async Task<TResult> PostAsync<TResult>(string path, object content = null)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(_account.Endpoint, path));
        if (content is not null)
        {
            httpRequest.Content = JsonContent.Create(content, options: Serialization.Options);
        }

        var httpResponse = await SendAsync(httpRequest);

        var result = await httpResponse.ParseAsync<TResult>();

        return result;
    }

    protected async Task<TResult> PutAsync<TResult>(string path, object content = null)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Put, new Uri(_account.Endpoint, path));
        if (content is not null)
        {
            httpRequest.Content = JsonContent.Create(content, options: Serialization.Options);
        }

        var httpResponse = await SendAsync(httpRequest);

        var result = await httpResponse.ParseAsync<TResult>();

        return result;
    }

    protected async Task<TResult> DeleteAsync<TResult>(string path, object content = null)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, new Uri(_account.Endpoint, path));
        if (content is not null)
        {
            httpRequest.Content = JsonContent.Create(content, options: Serialization.Options);
        }

        var httpResponse = await SendAsync(httpRequest);

        var result = await httpResponse.ParseAsync<TResult>();

        return result;
    }
}
