namespace SOS.Api.Contracts.Areas.Responses;

public sealed class AreaWithGeometryPagedResult
{
    public IReadOnlyList<AreaWithGeometryDto> Records { get; set; } = [];
    public int Skip { get; set; }
    public int Take { get; set; }
    public long TotalCount { get; set; }
}
