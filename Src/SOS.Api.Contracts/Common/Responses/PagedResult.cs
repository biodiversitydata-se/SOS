namespace SOS.Api.Contracts.Common.Responses;
public class PagedResult<T>
{
    public int Skip { get; init; }

    public int Take { get; init; }

    public long TotalCount { get; init; }

    public IReadOnlyList<T> Records { get; init; } = Array.Empty<T>();
}


