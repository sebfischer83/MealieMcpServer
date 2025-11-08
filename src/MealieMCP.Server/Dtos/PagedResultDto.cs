using System.Collections.Generic;

namespace MealieMCP.Server.Dtos;

/// <summary>
/// Represents a paginated result returned by the Mealie API.
/// </summary>
/// <typeparam name="T">Type of the payload items.</typeparam>
public class PagedResultDto<T>
{
    public PagedResultDto(
        IReadOnlyList<T> items,
        int page,
        int perPage,
        int total,
        int totalPages,
        string? next,
        string? previous)
    {
        Items = items;
        Page = page;
        PerPage = perPage;
        Total = total;
        TotalPages = totalPages;
        Next = next;
        Previous = previous;
    }

    /// <summary>
    /// Items contained in the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; }

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int Page { get; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PerPage { get; }

    /// <summary>
    /// Total number of items across all pages.
    /// </summary>
    public int Total { get; }

    /// <summary>
    /// Total number of pages available.
    /// </summary>
    public int TotalPages { get; }

    /// <summary>
    /// Link to the next page if available.
    /// </summary>
    public string? Next { get; }

    /// <summary>
    /// Link to the previous page if available.
    /// </summary>
    public string? Previous { get; }
}
