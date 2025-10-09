﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Zammad.Client.Core.Protocol;

public class BasicHttpClientHandlerTest
{
    [Theory(Skip = "This test does not make any sense")]
    [InlineData("zammad", "secure", "emFtbWFkOnNlY3VyZQ==")]
    [InlineData("ZAMMAD", "SECURE", "WkFNTUFEOlNFQ1VSRQ==")]
    public async Task BasicHttpClientHandler_Success_Test(string user, string password, string expected)
    {
        using (var httpHandler = new BasicHttpClientHandler(user, password, null))
        using (var httpClient = new HttpClient(httpHandler))
        {
            var response = await httpClient.GetAsync("https://zammad.com");
            Assert.Equal("Basic", response.RequestMessage.Headers.Authorization.Scheme);
            Assert.Equal(expected, response.RequestMessage.Headers.Authorization.Parameter);
        }
    }

    [Theory]
    [InlineData("zammad", "")]
    [InlineData("", "secure")]
    [InlineData("", "")]
    [InlineData("zammad", null)]
    [InlineData(null, "secure")]
    [InlineData(null, null)]
    public void BasicHttpClientHandler_Fail_Test(string user, string password)
    {
        Assert.ThrowsAny<ArgumentException>(() =>
        {
            var httpHandler = new BasicHttpClientHandler(user, password, null);
        });
    }
}
