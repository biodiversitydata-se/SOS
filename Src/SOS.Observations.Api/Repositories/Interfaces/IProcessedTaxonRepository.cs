using System.Collections.Generic;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Observations.Api.Repositories.Interfaces
{
    /// <summary>
    /// </summary>
    public interface IProcessedTaxonRepository : IProcessRepositoryBase<Observation, string>
    {

        Task<Result<IEnumerable<GeoGridTileTaxaCell>>> GetCompleteGeoTileTaxaAggregationAsync(
            SearchFilter filter,
            int zoom);

        Task<Result<GeoGridTileTaxonPageResult>> GetPageGeoTileTaxaAggregationAsync(
            SearchFilter filter,
            int zoom,
            string geoTilePage,
            int? taxonIdPage);

        /// <summary>
        /// Aggregate observations by taxon. Sort by observation count descending.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="sumUnderlyingTaxa"></param>
        /// <param name="noOfLatestHits"></param>
        /// <returns></returns>
        Task<Result<PagedResult<TaxonAggregationItem>>> GetTaxonAggregationAsync(
            SearchFilter filter,
            int? skip,
            int? take,
            bool sumUnderlyingTaxa = false,
            int noOfLatestHits = 1);        

        /// <summary>
        /// Get taxon sum aggregation. Including underlying taxa and province count.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<Dictionary<int, TaxonSumAggregationItem>> GetTaxonSumAggregationAsync(SearchFilter filter);

        /// <summary>
        /// Get indication if taxa exists in specified area
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IEnumerable<TaxonAggregationItem>> GetTaxonExistsIndicationAsync(
            SearchFilter filter);
    }
}