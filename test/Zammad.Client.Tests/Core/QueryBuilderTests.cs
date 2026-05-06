using System;
using System.Threading.Tasks;
using Zammad.Client.Core;

namespace Zammad.Client.Tests.Core;

public sealed class QueryBuilderTests
{
    [Test]
    public async Task ToString_WhenNoValues_ReturnsEmptyString()
    {
        var qb = new QueryBuilder();

        var result = qb.ToString();

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Add_String_AddsKeyValue_AndToStringBuildsQuery()
    {
        var qb = new QueryBuilder().Add("a", "1").Add("b", "two");

        var result = qb.ToString();

        await Assert.That(result).StartsWith("?");
        await Assert.That(result).Contains("a=1");
        await Assert.That(result).Contains("b=two");
        await Assert.That(CountPairs(result)).IsEqualTo(2);
    }

    [Test]
    public async Task Add_Int_AddsKeyValue_AsInvariantString()
    {
        var qb = new QueryBuilder().Add("limit", 25);

        var result = qb.ToString();

        await Assert.That(result).IsEqualTo("?limit=25");
    }

    [Test]
    [Arguments(true, "?active=true")]
    [Arguments(false, "?active=false")]
    public async Task Add_Bool_AddsKeyValue_AsLowercaseTrueFalse(bool input, string expected)
    {
        var qb = new QueryBuilder().Add("active", input);

        var result = qb.ToString();

        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task Add_SameKey_OverwritesPreviousValue()
    {
        var qb = new QueryBuilder().Add("q", "first").Add("q", "second");

        var result = qb.ToString();

        await Assert.That(result).IsEqualTo("?q=second");
    }

    [Test]
    public async Task KeysAreSortedAscending_InOutput()
    {
        var qb = new QueryBuilder().Add("z", "1").Add("a", "2").Add("m", "3");

        var result = qb.ToString();

        await Assert.That(result).IsEqualTo("?a=2&m=3&z=1");
    }

    [Test]
    public async Task ValuesProperty_ExposesSnapshotWithCurrentData()
    {
        var qb = new QueryBuilder().Add("a", "1").Add("b", "2");

        var values = qb.Values;

        await Assert.That(values.Count).IsEqualTo(2);
        await Assert.That(values["a"]).IsEqualTo("1");
        await Assert.That(values["b"]).IsEqualTo("2");
    }

    [Test]
    public async Task ToString_EncodesKeysAndValues()
    {
        var qb = new QueryBuilder().Add("sp ace", "val ue").Add("sym&bol", "x/y?").Add("unicode-✓", "🙂");

        var result = qb.ToString();

        // Ensure it's a valid query format
        await Assert.That(result).StartsWith("?");

        // Verify URL-encoding of special characters
        await Assert.That(result).Contains("sp%20ace=val%20ue");
        await Assert.That(result).Contains("sym%26bol=x%2Fy%3F");
        await Assert.That(result).Contains("unicode-%E2%9C%93=%F0%9F%99%82");

        await Assert.That(CountPairs(result)).IsEqualTo(3);
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
