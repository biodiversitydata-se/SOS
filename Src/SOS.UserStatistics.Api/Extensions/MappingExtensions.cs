namespace SOS.UserStatistics.Api.Extensions;

internal static class MappingExtensions
{
    public static PagedResultDto<TRecordDto> ToPagedResultDto<TRecord, TRecordDto>(
           this PagedResult<TRecord> pagedResult,
           IEnumerable<TRecordDto> records)
    {
        return new PagedResultDto<TRecordDto>
        {
            Records = records,
            Skip = pagedResult.Skip,
            Take = pagedResult.Take,
            TotalCount = pagedResult.TotalCount,
        };
    }
}
