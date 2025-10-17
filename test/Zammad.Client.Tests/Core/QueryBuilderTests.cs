using System;
using Xunit;
using Zammad.Client.Core;

namespace Zammad.Client.Tests.Core;

public sealed class QueryBuilderTests
{
    [Fact]
    public void ToString_WhenNoValues_ReturnsEmptyString()
    {
        var qb = new QueryBuilder();

        var result = qb.ToString();

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Add_String_AddsKeyValue_AndToStringBuildsQuery()
    {
        var qb = new QueryBuilder().Add("a", "1").Add("b", "two");

        var result = qb.ToString();

        Assert.StartsWith("?", result);
        Assert.Contains("a=1", result);
        Assert.Contains("b=two", result);
        Assert.Equal(2, CountPairs(result));
    }

    [Fact]
    public void Add_Int_AddsKeyValue_AsInvariantString()
    {
        var qb = new QueryBuilder().Add("limit", 25);

        var result = qb.ToString();

        Assert.Equal("?limit=25", result);
    }

    [Theory]
    [InlineData(true, "?active=true")]
    [InlineData(false, "?active=false")]
    public void Add_Bool_AddsKeyValue_AsLowercaseTrueFalse(bool input, string expected)
    {
        var qb = new QueryBuilder().Add("active", input);

        var result = qb.ToString();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Add_SameKey_OverwritesPreviousValue()
    {
        var qb = new QueryBuilder().Add("q", "first").Add("q", "second");

        var result = qb.ToString();

        Assert.Equal("?q=second", result);
    }

    [Fact]
    public void KeysAreSortedAscending_InOutput()
    {
        var qb = new QueryBuilder().Add("z", "1").Add("a", "2").Add("m", "3");

        var result = qb.ToString();

        Assert.Equal("?a=2&m=3&z=1", result);
    }

    [Fact]
    public void ValuesProperty_ExposesSnapshotWithCurrentData()
    {
        var qb = new QueryBuilder().Add("a", "1").Add("b", "2");

        var values = qb.Values;

        Assert.Equal(2, values.Count);
        Assert.Equal("1", values["a"]);
        Assert.Equal("2", values["b"]);
    }

    [Fact]
    public void ToString_EncodesKeysAndValues()
    {
        var qb = new QueryBuilder().Add("sp ace", "val ue").Add("sym&bol", "x/y?").Add("unicode-✓", "🙂");

        var result = qb.ToString();

        // Ensure it's a valid query format
        Assert.StartsWith("?", result);

        // Verify URL-encoding of special characters
        Assert.Contains("sp%20ace=val%20ue", result);
        Assert.Contains("sym%26bol=x%2Fy%3F", result);
        Assert.Contains("unicode-%E2%9C%93=%F0%9F%99%82", result);

        Assert.Equal(3, CountPairs(result));
    }

    private static int CountPairs(string query)
    {
        if (string.IsNullOrEmpty(query))
        {
            return 0;
        }

        string trimmed = query[0] switch
        {
            '?' => query[1..],
            _ => query,
        };

        return trimmed.Length switch
        {
            0 => 0,
            _ => trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries).Length,
        };
    }
}
