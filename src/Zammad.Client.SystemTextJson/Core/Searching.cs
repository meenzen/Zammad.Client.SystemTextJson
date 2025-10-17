namespace Zammad.Client.Core;

public enum OrderDirection
{
    Ascending,
    Descending,
}

public sealed class Sorting
{
    public required string SortBy { get; init; }
    public OrderDirection OrderBy { get; init; } = OrderDirection.Ascending;
}

public sealed class SearchQuery
{
    public required string Query { get; init; }
    public Sorting? Sorting { get; init; }
    public Pagination Pagination { get; init; } = new();
}

internal static class SearchQueryExtensions
{
    internal static QueryBuilder AddSorting(this QueryBuilder builder, Sorting? sorting)
    {
        if (sorting is null)
        {
            return builder;
        }

        builder.Add("sort_by", sorting.SortBy);
        builder.Add(
            "order_by",
            sorting.OrderBy switch
            {
                OrderDirection.Ascending => "asc",
                OrderDirection.Descending => "desc",
                _ => throw new LogicException($"Unknown {nameof(OrderDirection)} value: {sorting.OrderBy}"),
            }
        );

        return builder;
    }

    internal static QueryBuilder AddSearchQuery(this QueryBuilder builder, SearchQuery? query)
    {
        if (query is null)
        {
            return builder;
        }

        builder.Add("query", query.Query);
        builder.AddSorting(query.Sorting);
        builder.AddPagination(query.Pagination);

        return builder;
    }
}
