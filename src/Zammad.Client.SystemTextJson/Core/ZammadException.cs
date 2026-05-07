using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace Zammad.Client.Core;

[SuppressMessage("Roslynator", "RCS1194:Implement exception constructors")]
public sealed class ZammadException(HttpRequestMessage request, HttpResponseMessage response)
    : Exception(response.ReasonPhrase)
{
    public HttpRequestMessage Request { get; } = request;
    public HttpResponseMessage Response { get; } = response;
    public HttpStatusCode Code => Response.StatusCode;
}
