using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;

namespace Zammad.Client.Core;

#nullable enable

[SuppressMessage("Roslynator", "RCS1194:Implement exception constructors")]
public class ZammadException(HttpRequestMessage request, HttpResponseMessage response)
    : Exception(response.ReasonPhrase)
{
    public HttpRequestMessage Request { get; } = request;
    public HttpResponseMessage Response { get; } = response;
    public HttpStatusCode Code => Response.StatusCode;
}
