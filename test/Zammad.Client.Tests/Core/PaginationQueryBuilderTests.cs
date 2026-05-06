using System.Threading.Tasks;
using Zammad.Client.Core;

namespace Zammad.Client.Tests.Core;

public sealed class PaginationQueryBuilderTests
{
    [Test]
    public async Task AddPagination_Null_ReturnsSameBuilder()
    {
        var builder = new QueryBuilder();

        var result = builder.AddPagination(null);

        await Assert.That(result).IsSameReferenceAs(builder);
        await Assert.That(builder.Values).IsEmpty();
    }

    [Test]
    public async Task AddPagination_Adds_Page_And_PerPage()
    {
        var builder = new QueryBuilder();
        var pagination = new Pagination { Page = 3, PerPage = 20 };

        var result = builder.AddPagination(pagination);

        await Assert.That(result).IsSameReferenceAs(builder);
        await Assert.That(builder.Values.Count).IsEqualTo(2);
        await Assert.That(builder.Values["page"]).IsEqualTo(3.ToString());
        await Assert.That(builder.Values["per_page"]).IsEqualTo(20.ToString());
    }

    [Test]
    public async Task AddLimitOffsetPagination_Null_ReturnsSameBuilder()
    {
        var builder = new QueryBuilder();

        var result = builder.AddLimitOffsetPagination(null);

        await Assert.That(result).IsSameReferenceAs(builder);
        await Assert.That(builder.Values).IsEmpty();
    }

    [Test]
    [Arguments(1, 50, 50, 0)]
    [Arguments(2, 50, 50, 50)]
    [Arguments(4, 25, 25, 75)]
    public async Task AddLimitOffsetPagination_Adds_Limit_And_Offset(
        int page,
        int perPage,
        int expectedLimit,
        int expectedOffset
    )
    {
        var builder = new QueryBuilder();
        var pagination = new Pagination { Page = page, PerPage = perPage };

        var result = builder.AddLimitOffsetPagination(pagination);

        await Assert.That(result).IsSameReferenceAs(builder);
        await Assert.That(builder.Values["limit"]).IsEqualTo(expectedLimit.ToString());
        await Assert.That(builder.Values["offset"]).IsEqualTo(expectedOffset.ToString());
    }
}
