using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;

namespace Zammad.Client.Core;

[SuppressMessage("Roslynator", "RCS1194:Implement exception constructors")]
public sealed class ZammadException : Exception
{
    private const int MaxMessageContentLength = 1000;

    public ZammadException(HttpRequestMessage request, HttpResponseMessage response)
        : this(request, response, null) { }

    public ZammadException(HttpRequestMessage request, HttpResponseMessage response, string? content)
        : this(request, response, content, null) { }

    /// <summary>
    /// For responses that report an error in another field than <c>error_human</c>/<c>error</c>, e.g. the
    /// <c>{"result": "failed", "message": "..."}</c> that some endpoints return with 200 OK.
    /// </summary>
    internal ZammadException(HttpRequestMessage request, HttpResponseMessage response, string? content, string? error)
        : base(BuildMessage(request, response, content, error))
    {
        Request = request;
        Response = response;
        Content = content;
        _error = error;
    }

    private readonly string? _error;

    public HttpRequestMessage Request { get; }
    public HttpResponseMessage Response { get; }
    public HttpStatusCode Code => Response.StatusCode;

    /// <summary>
    /// The raw response body returned by Zammad, if it could be read.
    /// </summary>
    public string? Content { get; }

    /// <summary>
    /// The error message returned by Zammad (<c>error_human</c> or <c>error</c> field), if present.
    /// </summary>
    /// <remarks>
    /// For endpoints that report failures with 200 OK and a <c>message</c> field instead (ticket merge), this is that
    /// message.
    /// </remarks>
    public string? Error => _error ?? ParseError(Content);

    private static string BuildMessage(
        HttpRequestMessage request,
        HttpResponseMessage response,
        string? content,
        string? error
    )
    {
        var message =
            $"{request.Method} {request.RequestUri?.AbsolutePath} failed with {(int)response.StatusCode} {response.ReasonPhrase}";

        if ((error ?? ParseError(content) ?? content) is not { } detail || string.IsNullOrWhiteSpace(detail))
        {
            return message;
        }

        if (detail.Length > MaxMessageContentLength)
        {
            detail = detail.Substring(0, MaxMessageContentLength) + "...";
        }

        return $"{message}: {detail}";
    }

    private static string? ParseError(string? content)
    {
        if (content is not { } json || string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var name in new[] { "error_human", "error" })
            {
                if (
                    document.RootElement.TryGetProperty(name, out var property)
                    && property.ValueKind == JsonValueKind.String
                    && property.GetString() is { } error
                    && !string.IsNullOrWhiteSpace(error)
                )
                {
                    return error;
                }
            }

            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
