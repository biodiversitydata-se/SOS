using SOS.Lib.Models.Search.Result;
using SOS.Shared.Api.Dtos;

namespace SOS.Shared.Api.Extensions.Dto;

public static class TaxonExtensions
{
    public static IEnumerable<TaxonAggregationItemDto> ToTaxonAggregationItemDtos(this IEnumerable<TaxonAggregationItem> taxonAggregationItems)
    {
        return taxonAggregationItems.Select(item => item.ToTaxonAggregationItemDto());
    }

    public static TaxonAggregationItemDto ToTaxonAggregationItemDto(this TaxonAggregationItem taxonAggregationItem)
    {
        return new TaxonAggregationItemDto
        {
            FirstSighting = taxonAggregationItem.FirstSighting,
            LastSighting = taxonAggregationItem.LastSighting,
            TaxonId = taxonAggregationItem.TaxonId,
            ObservationCount = taxonAggregationItem.ObservationCount
        };
    }
}
