using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Zammad.Client.Abstractions;
using Zammad.Client.Core;

namespace Zammad.Client;

#nullable enable

public sealed partial class ZammadClient : IZammadClient
{
    private readonly HttpClient _client;

    public ZammadClient(HttpClient client, IOptions<ZammadOptions> options)
    {
        options.Value.ThrowIfInvalid();
        _client = client;
        _client.BaseAddress = options.Value.BaseUrl;

        if (!string.IsNullOrWhiteSpace(options.Value.OnBehalfOf))
        {
            _client.DefaultRequestHeaders.Add("X-On-Behalf-Of", options.Value.OnBehalfOf);
        }

        if (!string.IsNullOrWhiteSpace(options.Value.Token))
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Token",
                $"token={options.Value.Token}"
            );
        }
        else
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(options.Value.Username));
            Debug.Assert(!string.IsNullOrWhiteSpace(options.Value.Password));
            var credentials = Convert.ToBase64String(
                Encoding.GetEncoding("ISO-8859-1").GetBytes($"{options.Value.Username}:{options.Value.Password}")
            );
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }
    }

    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequest)
    {
        var httpResponse = await _client.SendAsync(httpRequest);
        if (!httpResponse.IsSuccessStatusCode)
        {
            throw new ZammadException(httpRequest, httpResponse);
        }

        return httpResponse;
    }

    private async Task<TResult?> GetAsync<TResult>(string path, string? query = null)
    {
        var builder = new UriBuilder(new Uri(_client.BaseAddress!, path));
        if (query is not null)
        {
            builder.Query = query;
        }

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, builder.Uri);

        var httpResponse = await SendAsync(httpRequest);

        var result = await httpResponse.ParseAsync<TResult>();

        return result;
    }

    private async Task<TResult?> PostAsync<TResult>(string path, object? content = null)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(_client.BaseAddress!, path));
        if (content is not null)
        {
            httpRequest.Content = JsonContent.Create(content, options: Serialization.Options);
        }

        var httpResponse = await SendAsync(httpRequest);

        var result = await httpResponse.ParseAsync<TResult>();

        return result;
    }

    private async Task<TResult?> PutAsync<TResult>(string path, object? content = null)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Put, new Uri(_client.BaseAddress!, path));
        if (content is not null)
        {
            httpRequest.Content = JsonContent.Create(content, options: Serialization.Options);
        }

        var httpResponse = await SendAsync(httpRequest);

        var result = await httpResponse.ParseAsync<TResult>();

        return result;
    }

    private async Task<TResult?> DeleteAsync<TResult>(string path, object? content = null)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, new Uri(_client.BaseAddress!, path));
        if (content is not null)
        {
            httpRequest.Content = JsonContent.Create(content, options: Serialization.Options);
        }

        var httpResponse = await SendAsync(httpRequest);

        var result = await httpResponse.ParseAsync<TResult>();

        return result;
    }
}
