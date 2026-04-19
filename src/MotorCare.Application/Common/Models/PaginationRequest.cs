namespace MotorCare.Application.Common.Models;

public sealed record PaginationRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public int SafePageNumber => PageNumber < 1 ? 1 : PageNumber;
    public int SafePageSize => PageSize switch
    {
        < 1 => 20,
        > 100 => 100,
        _ => PageSize
    };

    public int Skip => (SafePageNumber - 1) * SafePageSize;

    public static PaginationRequest Of(int pageNumber, int pageSize)
        => new() { PageNumber = pageNumber, PageSize = pageSize };
}
