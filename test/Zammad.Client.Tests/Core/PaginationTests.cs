using Zammad.Client.Core;

namespace Zammad.Client.Tests.Core;

public sealed class PaginationTests
{
    [Test]
    public async Task Defaults_AreCorrect()
    {
        var p = new Pagination();

        await Assert.That(p.Page).IsEqualTo(1);
        await Assert.That(p.PerPage).IsEqualTo(Pagination.DefaultPerPage);
    }

    [Test]
    [Arguments(1)]
    [Arguments(5)]
    [Arguments(1000)]
    public async Task Page_Init_Assigns_WhenValid(int value)
    {
        var p = new Pagination { Page = value };

        await Assert.That(p.Page).IsEqualTo(value);
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(int.MinValue)]
    public async Task Page_Init_Clamps_To_Min1(int value)
    {
        var p = new Pagination { Page = value };

        await Assert.That(p.Page).IsEqualTo(1);
    }

    [Test]
    [Arguments(1)]
    [Arguments(10)]
    [Arguments(Pagination.MaxPerPage)]
    public async Task PerPage_Init_Assigns_WhenWithinBounds(int value)
    {
        var p = new Pagination { PerPage = value };

        await Assert.That(p.PerPage).IsEqualTo(value);
    }

    [Test]
    [Arguments(0)]
    [Arguments(-1)]
    [Arguments(int.MinValue)]
    public async Task PerPage_Init_Clamps_To_Min1(int value)
    {
        var p = new Pagination { PerPage = value };

        await Assert.That(p.PerPage).IsEqualTo(1);
    }

    [Test]
    [Arguments(Pagination.MaxPerPage + 1)]
    [Arguments(10_000)]
    [Arguments(int.MaxValue)]
    public async Task PerPage_Init_Clamps_To_Max(int value)
    {
        var p = new Pagination { PerPage = value };

        await Assert.That(p.PerPage).IsEqualTo(Pagination.MaxPerPage);
    }

    [Test]
    public async Task Next_Increments_Page_And_Preserves_PerPage()
    {
        var p = new Pagination { Page = 3, PerPage = 40 };

        var next = p.Next();

        await Assert.That(next.Page).IsEqualTo(4);
        await Assert.That(next.PerPage).IsEqualTo(40);
        await Assert.That(p.Page).IsEqualTo(3); // immutability of original
    }

    [Test]
    public async Task Previous_ReturnsNull_On_First_Page()
    {
        var p = new Pagination { Page = 1, PerPage = 25 };

        var prev = p.Previous();

        await Assert.That(prev).IsNull();
    }

    [Test]
    public async Task Previous_Decrements_Page_And_Preserves_PerPage()
    {
        var p = new Pagination { Page = 5, PerPage = 25 };

        var prev = p.Previous();

        await Assert.That(prev).IsNotNull();
        await Assert.That(prev.Page).IsEqualTo(4);
        await Assert.That(prev.PerPage).IsEqualTo(25);
    }

    [Test]
    [Arguments(1, 50, 0)]
    [Arguments(2, 50, 50)]
    [Arguments(3, 25, 50)]
    [Arguments(10, 100, 900)]
    public async Task GetOffset_Computes_Correctly(int page, int perPage, int expected)
    {
        var p = new Pagination { Page = page, PerPage = perPage };

        var offset = p.GetOffset();

        await Assert.That(offset).IsEqualTo(expected);
    }
}
