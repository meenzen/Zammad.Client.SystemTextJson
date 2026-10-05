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
        : base(BuildMessage(request, response, content))
    {
        Request = request;
        Response = response;
        Content = content;
    }

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
    public string? Error => ParseError(Content);

    private static string BuildMessage(HttpRequestMessage request, HttpResponseMessage response, string? content)
    {
        var message =
            $"{request.Method} {request.RequestUri?.AbsolutePath} failed with {(int)response.StatusCode} {response.ReasonPhrase}";

        if ((ParseError(content) ?? content) is not { } detail || string.IsNullOrWhiteSpace(detail))
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
                )
                {
                    return property.GetString();
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
