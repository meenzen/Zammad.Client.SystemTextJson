using Xunit;
using Zammad.Client.Core;

namespace Zammad.Client.Tests.Core;

public sealed class PaginationTests
{
    [Fact]
    public void Defaults_AreCorrect()
    {
        var p = new Pagination();

        Assert.Equal(1, p.Page);
        Assert.Equal(Pagination.DefaultPerPage, p.PerPage);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(1000)]
    public void Page_Init_Assigns_WhenValid(int value)
    {
        var p = new Pagination { Page = value };

        Assert.Equal(value, p.Page);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Page_Init_Clamps_To_Min1(int value)
    {
        var p = new Pagination { Page = value };

        Assert.Equal(1, p.Page);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(Pagination.MaxPerPage)]
    public void PerPage_Init_Assigns_WhenWithinBounds(int value)
    {
        var p = new Pagination { PerPage = value };

        Assert.Equal(value, p.PerPage);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void PerPage_Init_Clamps_To_Min1(int value)
    {
        var p = new Pagination { PerPage = value };

        Assert.Equal(1, p.PerPage);
    }

    [Theory]
    [InlineData(Pagination.MaxPerPage + 1)]
    [InlineData(10_000)]
    [InlineData(int.MaxValue)]
    public void PerPage_Init_Clamps_To_Max(int value)
    {
        var p = new Pagination { PerPage = value };

        Assert.Equal(Pagination.MaxPerPage, p.PerPage);
    }

    [Fact]
    public void Next_Increments_Page_And_Preserves_PerPage()
    {
        var p = new Pagination { Page = 3, PerPage = 40 };

        var next = p.Next();

        Assert.Equal(4, next.Page);
        Assert.Equal(40, next.PerPage);
        Assert.Equal(3, p.Page); // immutability of original
    }

    [Fact]
    public void Previous_ReturnsNull_On_First_Page()
    {
        var p = new Pagination { Page = 1, PerPage = 25 };

        var prev = p.Previous();

        Assert.Null(prev);
    }

    [Fact]
    public void Previous_Decrements_Page_And_Preserves_PerPage()
    {
        var p = new Pagination { Page = 5, PerPage = 25 };

        var prev = p.Previous();

        Assert.NotNull(prev);
        Assert.Equal(4, prev.Page);
        Assert.Equal(25, prev.PerPage);
    }

    [Theory]
    [InlineData(1, 50, 0)]
    [InlineData(2, 50, 50)]
    [InlineData(3, 25, 50)]
    [InlineData(10, 100, 900)]
    public void GetOffset_Computes_Correctly(int page, int perPage, int expected)
    {
        var p = new Pagination { Page = page, PerPage = perPage };

        var offset = p.GetOffset();

        Assert.Equal(expected, offset);
    }
}
