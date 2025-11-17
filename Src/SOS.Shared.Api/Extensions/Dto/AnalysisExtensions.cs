using SOS.Lib.Models.Analysis;
using SOS.Lib.Models.Search.Result;
using SOS.Shared.Api.Dtos.Search;

namespace SOS.Shared.Api.Extensions.Dto;

public static class AnalysisExtensions
{
    extension(PagedAggregationResult<UserAggregationResponse> result)
    {
        public PagedAggregationResultDto<UserAggregationResponseDto> ToDto()
        {
            if (result == null)
            {
                return null!;
            }

            return new PagedAggregationResultDto<UserAggregationResponseDto>
            {
                AfterKey = result.AfterKey,
                Records = result.Records?.Select(r => new UserAggregationResponseDto
                {
                    AggregationField = r.AggregationField,
                    Count = r.Count,
                    UniqueTaxon = r.UniqueTaxon,
                    OrganismQuantity = r.OrganismQuantity
                })
            };
        }
    }

    extension(IEnumerable<AggregationItem> result)
    {
        public IEnumerable<AggregationItemOrganismQuantityDto> ToDto()
        {
            if (result == null)
            {
                return null!;
            }

            return result.Select(item => new AggregationItemOrganismQuantityDto
            {
                AggregationKey = item.AggregationKey,
                DocCount = item.DocCount,
                OrganismQuantity = item.OrganismQuantity
            });
        }
    }
}