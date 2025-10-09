using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zammad.Client.Core.Protocol;

public class HttpRequestBuilder
{
    private HttpMethod _method;
    private UriBuilder _requestUriBuilder;
    private HttpContent _content;

    public HttpRequestBuilder() => _requestUriBuilder = new UriBuilder();

    public HttpRequestBuilder UseGet()
    {
        _method = HttpMethod.Get;
        return this;
    }

    public HttpRequestBuilder UsePost()
    {
        _method = HttpMethod.Post;
        return this;
    }

    public HttpRequestBuilder UsePut()
    {
        _method = HttpMethod.Put;
        return this;
    }

    public HttpRequestBuilder UseDelete()
    {
        _method = HttpMethod.Delete;
        return this;
    }

    public HttpRequestBuilder UseRequestUri(string requestUri)
    {
        ArgumentCheck.ThrowIfNullOrEmpty(requestUri, nameof(requestUri));

        return UseRequestUri(new Uri(requestUri));
    }

    public HttpRequestBuilder UseRequestUri(Uri requestUri)
    {
        ArgumentCheck.ThrowIfNull(requestUri, nameof(requestUri));

        _requestUriBuilder = new UriBuilder(requestUri);
        return this;
    }

    public HttpRequestBuilder AddPath(string path)
    {
        ArgumentCheck.ThrowIfNullOrEmpty(path, nameof(path));

        var pathBuilder = new StringBuilder(_requestUriBuilder.Path);
        if (pathBuilder.Length == 0)
        {
            if (path[0] != '/')
            {
                pathBuilder.Append('/');
            }
        }
        else
        {
            if (pathBuilder[pathBuilder.Length - 1] == '/' && path[0] == '/')
            {
                pathBuilder.Remove(pathBuilder.Length - 1, 1);
            }
            else if (pathBuilder[pathBuilder.Length - 1] != '/' && path[0] != '/')
            {
                pathBuilder.Append('/');
            }
        }
        pathBuilder.Append(path);
        _requestUriBuilder.Path = pathBuilder.ToString();
        return this;
    }

    public HttpRequestBuilder UseQuery(string query)
    {
        if (query is null)
        {
            query = string.Empty;
        }
        _requestUriBuilder.Query = query;
        return this;
    }

    public HttpRequestBuilder AddQuery(string key, string value)
    {
        ArgumentCheck.ThrowIfNullOrEmpty(key, nameof(key));
        ArgumentCheck.ThrowIfNull(value, nameof(value));

        var queryBuilder = new StringBuilder(_requestUriBuilder.Query);
        if (queryBuilder.Length == 0)
        {
            queryBuilder.Append('?');
        }
        else
        {
            queryBuilder.Append('&');
        }
        queryBuilder.AppendFormat("{0}={1}", key, Uri.EscapeDataString(value));
        _requestUriBuilder.Query = queryBuilder.ToString();
        return this;
    }

    public HttpRequestBuilder UseJsonContent(object json)
    {
        if (json is null)
        {
            _content = null;
            return this;
        }

        var jsonString = JsonSerializer.Serialize(
            json,
            new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }
        );
        _content = new StringContent(jsonString);
        _content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return this;
    }

    public HttpRequestMessage Build()
    {
        var httpRequest = new HttpRequestMessage
        {
            Method = _method,
            RequestUri = _requestUriBuilder.Uri,
            Content = _content,
        };
        return httpRequest;
    }
}
