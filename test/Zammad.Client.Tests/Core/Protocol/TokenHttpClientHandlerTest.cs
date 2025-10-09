﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Zammad.Client.Core.Protocol
{
    public class TokenHttpClientHandlerTest
    {
        [Theory(Skip = "This test does not make any sense")]
        [InlineData("token", "token=token")]
        [InlineData("TOKEN", "token=TOKEN")]
        public async Task TokenHttpClientHandler_Success_Test(string token, string expected)
        {
            using (var httpHandler = new TokenHttpClientHandler(token, null))
            using (var httpClient = new HttpClient(httpHandler))
            {
                var response = await httpClient.GetAsync("https://zammad.com");
                Assert.Equal("Token", response.RequestMessage.Headers.Authorization.Scheme);
                Assert.Equal(expected, response.RequestMessage.Headers.Authorization.Parameter);
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void TokenHttpClientHandler_Fail_Test(string token)
        {
            Assert.ThrowsAny<ArgumentException>(() =>
            {
                var httpHandler = new TokenHttpClientHandler(token, null);
            });
        }
    }
}
