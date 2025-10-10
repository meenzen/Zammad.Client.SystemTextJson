using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Zammad.Client.Core;

#nullable enable

internal static class HttpResponseParser
{
    internal static async Task<TResult?> ParseAsync<TResult>(this HttpResponseMessage httpResponse)
    {
        object? result;
        switch (typeof(TResult).Name)
        {
            case nameof(Boolean):
                result = httpResponse.IsSuccessStatusCode;
                break;
            case nameof(HttpStatusCode):
                result = httpResponse.StatusCode;
                break;
            case nameof(Stream):
                Stream stream;
                if (
                    httpResponse.Content.Headers.ContentLength.HasValue
                    && httpResponse.Content.Headers.ContentLength == 0
                )
                {
                    stream = Stream.Null;
                }
                else
                {
                    stream = await httpResponse.Content.ReadAsStreamAsync();
                }

                result = stream;
                break;
            default:
                TResult? ret;
                if (httpResponse.Content.Headers.ContentLength is 0)
                {
                    ret = default;
                }
                else
                {
                    var contentType = httpResponse.Content.Headers.ContentType;
                    if (!string.Equals(contentType?.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new NotSupportedException(
                            $"Content media type {contentType?.MediaType} is not supported."
                        );
                    }

                    if (!string.Equals(contentType?.CharSet, "utf-8", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new NotSupportedException($"Content charset {contentType?.CharSet} is not supported.");
                    }

                    TResult? content;
                    using (var contentStream = await httpResponse.Content.ReadAsStreamAsync())
                    {
                        content = await JsonSerializer.DeserializeAsync<TResult>(contentStream);
                    }

                    ret = content;
                }

                result = ret;
                break;
        }

        return (TResult?)result;
    }
}
