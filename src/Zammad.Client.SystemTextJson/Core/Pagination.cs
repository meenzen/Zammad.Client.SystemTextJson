namespace Zammad.Client.Core;

public sealed class Pagination
{
    public const int DefaultPerPage = 50;
    public const int MaxPerPage = 100;

    private readonly int _page = 1;
    private readonly int _perPage = DefaultPerPage;

    /// <summary>
    /// Page number.
    /// </summary>
    /// <remarks>
    /// Page number starts at 1.
    /// </remarks>
    public int Page
    {
        get => _page;
        init
        {
            if (value < 1)
            {
                value = 1;
            }

            _page = value;
        }
    }

    public int PerPage
    {
        get => _perPage;
        init
        {
            if (value < 1)
            {
                value = 1;
            }

            if (value > MaxPerPage)
            {
                value = MaxPerPage;
            }

            _perPage = value;
        }
    }
}

public static class PaginationExtensions
{
    /// <summary>
    /// Get the next page.
    /// </summary>
    public static Pagination Next(this Pagination pagination) =>
        new() { Page = pagination.Page + 1, PerPage = pagination.PerPage };

    /// <summary>
    /// Get the previous page.
    /// </summary>
    /// <returns>null if the page is the first page.</returns>
    public static Pagination? Previous(this Pagination pagination)
    {
        if (pagination.Page <= 1)
        {
            return null;
        }

        return new Pagination { Page = pagination.Page - 1, PerPage = pagination.PerPage };
    }
}

internal static class PaginationQueryBuilder
{
    internal static QueryBuilder AddPagination(this QueryBuilder builder, Pagination? pagination)
    {
        if (pagination is null)
        {
            return builder;
        }

        return builder.Add("page", pagination.Page).Add("per_page", pagination.PerPage);
    }

    internal static int GetOffset(this Pagination pagination) => (pagination.Page - 1) * pagination.PerPage;

    internal static QueryBuilder AddLimitOffsetPagination(this QueryBuilder builder, Pagination? pagination)
    {
        if (pagination is null)
        {
            return builder;
        }

        return builder.Add("limit", pagination.PerPage).Add("offset", pagination.GetOffset());
    }
}
