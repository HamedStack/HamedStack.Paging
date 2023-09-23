// ReSharper disable PossibleMultipleEnumeration

namespace HamedStack.Paging;

/// <summary>
/// Represents an asynchronous paged list of items created from an <see cref="IAsyncEnumerable{T}"/> source.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
[Serializable]
public class PagedListAsync<T> : IPagedList<T>
{
    /// <summary>
    /// Private constructor to prevent direct instantiation of <see cref="PagedListAsync{T}"/>.
    /// </summary>
    private PagedListAsync()
    {
    }

    /// <summary>
    /// Asynchronously creates a new instance of the <see cref="PagedListAsync{T}"/> class.
    /// </summary>
    /// <param name="source">The <see cref="IAsyncEnumerable{T}"/> source from which the asynchronous paged list is created.</param>
    /// <param name="pageNumber">The current page number (1-based index).</param>
    /// <param name="pageSize">The number of items to include on each page.</param>
    /// <returns>A <see cref="Task{T}"/> representing the asynchronous operation that returns an instance of <see cref="PagedListAsync{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="pageNumber"/> or <paramref name="pageSize"/> is less than or equal to 0.</exception>
    /// <remarks>
    /// This factory method asynchronously initializes an asynchronous paged list from an <see cref="IAsyncEnumerable{T}"/> source. It calculates various properties of the paged list, such as the total number of items, the number of pages, and asynchronously loads the items on the current page.
    /// </remarks>
    public static async Task<IPagedList<T>> CreateAsync(IAsyncEnumerable<T> source, int pageNumber, int pageSize)
    {
        if (source == null)
            throw new ArgumentNullException(nameof(source));

        if (pageNumber <= 0 || pageSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number and page size must be greater than zero.");

        var pagedList = new PagedListAsync<T>
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        
        await pagedList.LoadDataAsync(source);

        return pagedList;
    }
    
    /// <inheritdoc />
    public int FirstItemOnPage => (PageNumber - 1) * PageSize + 1;

    /// <inheritdoc />
    public bool HasNextPage => PageNumber < PageCount;

    /// <inheritdoc />
    public bool HasPreviousPage => PageNumber > 1;

    /// <inheritdoc />
    public bool IsFirstPage => PageNumber == 1;

    /// <inheritdoc />
    public bool IsLastPage => PageNumber == PageCount;

    /// <inheritdoc />
    public IList<T> Items { get; private set; } = new List<T>();

    /// <inheritdoc />
    public int LastItemOnPage => FirstItemOnPage + Items.Count - 1;

    /// <inheritdoc />
    public int PageCount { get; private set; }

    /// <inheritdoc />
    public int PageNumber { get; private set; }

    /// <inheritdoc />
    public int PageSize { get; private set; }

    /// <inheritdoc />
    public int TotalCount { get; private set; }

    /// <inheritdoc />
    public T this[int index]
    {
        get
        {
            if (index < 0 || index >= Items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }
            return Items[index];
        }
    }

    /// <summary>
    /// Asynchronously loads data from the provided <see cref="IAsyncEnumerable{T}"/> source and populates the paged list.
    /// </summary>
    /// <param name="source">The <see cref="IAsyncEnumerable{T}"/> source from which to load data.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="source"/> is null.</exception>
    /// <remarks>
    /// This method asynchronously loads data from the specified <paramref name="source"/> and updates the paged list's properties, including the total number of items and the items on the current page.
    /// </remarks>
    private async Task LoadDataAsync(IAsyncEnumerable<T> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        TotalCount = await source.CountAsync();
        PageCount = (int)Math.Ceiling(TotalCount / (double)PageSize);
        Items = await source.SkipAsync((PageNumber - 1) * PageSize).TakeAsync(PageSize).ToListAsync();
    }
}