using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Zammad.Client.Core;

[SuppressMessage("Roslynator", "RCS1194:Implement exception constructors")]
public class ZammadException : Exception
{
    public ZammadException(HttpRequestMessage request, HttpResponseMessage response)
        : base(GetMessage(response))
    {
        Request = request;
        Response = response;
    }

    private static string GetMessage(HttpResponseMessage response) => response.ReasonPhrase;

    public HttpRequestMessage Request { get; }
    public HttpResponseMessage Response { get; }
    public HttpStatusCode? Code => Response?.StatusCode;

    public async Task<string> GetResponseContentAsStringAsync() => await Response?.Content?.ReadAsStringAsync();
}
