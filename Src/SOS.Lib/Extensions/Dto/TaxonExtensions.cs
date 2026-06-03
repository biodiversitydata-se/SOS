using SOS.Lib.Models.Search.Result;
using SOS.Lib.Dtos;

namespace SOS.Lib.Extensions.Dto;

public static class TaxonExtensions
{
    extension(IEnumerable<TaxonAggregationItem> taxonAggregationItems)
    {
        public IEnumerable<TaxonAggregationItemDto> ToTaxonAggregationItemDtos()
        {
            return taxonAggregationItems.Select(item => item.ToTaxonAggregationItemDto());
        }
    }

    extension(TaxonAggregationItem taxonAggregationItem)
    {
        public TaxonAggregationItemDto ToTaxonAggregationItemDto()
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
}
