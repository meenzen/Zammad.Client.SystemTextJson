using Xunit;
using Zammad.Client.Core;

namespace Zammad.Client.Tests.Core;

public sealed class PaginationQueryBuilderTests
{
    [Fact]
    public void AddPagination_Null_ReturnsSameBuilder()
    {
        var builder = new QueryBuilder();

        var result = builder.AddPagination(null);

        Assert.Same(builder, result);
        Assert.Empty(builder.Values);
    }

    [Fact]
    public void AddPagination_Adds_Page_And_PerPage()
    {
        var builder = new QueryBuilder();
        var pagination = new Pagination { Page = 3, PerPage = 20 };

        var result = builder.AddPagination(pagination);

        Assert.Same(builder, result);
        Assert.Equal(2, builder.Values.Count);
        Assert.Equal(3.ToString(), builder.Values["page"]);
        Assert.Equal(20.ToString(), builder.Values["per_page"]);
    }

    [Fact]
    public void AddLimitOffsetPagination_Null_ReturnsSameBuilder()
    {
        var builder = new QueryBuilder();

        var result = builder.AddLimitOffsetPagination(null);

        Assert.Same(builder, result);
        Assert.Empty(builder.Values);
    }

    [Theory]
    [InlineData(1, 50, 50, 0)]
    [InlineData(2, 50, 50, 50)]
    [InlineData(4, 25, 25, 75)]
    public void AddLimitOffsetPagination_Adds_Limit_And_Offset(
        int page,
        int perPage,
        int expectedLimit,
        int expectedOffset
    )
    {
        var builder = new QueryBuilder();
        var pagination = new Pagination { Page = page, PerPage = perPage };

        var result = builder.AddLimitOffsetPagination(pagination);

        Assert.Same(builder, result);
        Assert.Equal(expectedLimit.ToString(), builder.Values["limit"]);
        Assert.Equal(expectedOffset.ToString(), builder.Values["offset"]);
    }
}
