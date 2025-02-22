namespace JobBot.Pagination;

public class PaginationResult<T>
{
    public required int TotalCount { get; set; }
    public required T[] Results { get; set; }
}
