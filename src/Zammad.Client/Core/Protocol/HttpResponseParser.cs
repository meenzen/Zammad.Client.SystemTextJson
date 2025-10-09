﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Zammad.Client.Core.Protocol
{
    public class HttpResponseParser
    {
        private HttpResponseMessage _httpResponse;

        public HttpResponseParser() { }

        public HttpResponseParser UseHttpResponse(HttpResponseMessage httpResponse)
        {
            ArgumentCheck.ThrowIfNull(httpResponse, nameof(httpResponse));
            _httpResponse = httpResponse;
            return this;
        }

        public bool ParseSuccessStatus() => _httpResponse.IsSuccessStatusCode;

        public HttpStatusCode ParseStatusCode() => _httpResponse.StatusCode;

        public async Task<TContent> ParseJsonContentAsync<TContent>()
        {
            if (
                _httpResponse.Content.Headers.ContentLength.HasValue
                && _httpResponse.Content.Headers.ContentLength == 0
            )
            {
                return default;
            }

            if (string.Compare(_httpResponse.Content.Headers.ContentType.MediaType, "application/json", true) != 0)
            {
                throw new NotSupportedException(
                    $"Content media type {_httpResponse.Content.Headers.ContentType.MediaType} is not supported."
                );
            }

            if (string.Compare(_httpResponse.Content.Headers.ContentType.CharSet, "utf-8", true) != 0)
            {
                throw new NotSupportedException(
                    $"Content charset {_httpResponse.Content.Headers.ContentType.CharSet} is not supported."
                );
            }

            var content = default(TContent);
            using (var httpResponseStream = await _httpResponse.Content.ReadAsStreamAsync())
            {
                content = await JsonSerializer.DeserializeAsync<TContent>(httpResponseStream);
            }
            return content;
        }

        public async Task<Stream> ParseStreamContentAsync()
        {
            if (
                _httpResponse.Content.Headers.ContentLength.HasValue
                && _httpResponse.Content.Headers.ContentLength == 0
            )
            {
                return Stream.Null;
            }

            return await _httpResponse.Content.ReadAsStreamAsync();
        }

        public async Task<TResult> ParseAsync<TResult>()
        {
            var result = default(object);
            switch (typeof(TResult).Name)
            {
                case nameof(Boolean):
                {
                    result = ParseSuccessStatus();
                    break;
                }
                case nameof(HttpStatusCode):
                {
                    result = ParseStatusCode();
                    break;
                }
                case nameof(Stream):
                {
                    result = await ParseStreamContentAsync();
                    break;
                }
                default:
                {
                    result = await ParseJsonContentAsync<TResult>();
                    break;
                }
            }
            return (TResult)result;
        }
    }
}
