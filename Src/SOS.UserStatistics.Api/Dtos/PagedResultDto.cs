namespace SOS.UserStatistics.Api.Dtos;

public class PagedResultDto<T>
{
    public int Skip { get; set; }
    public int Take { get; set; }
    public long TotalCount { get; set; }
    public IEnumerable<T> Records { get; set; }
}
