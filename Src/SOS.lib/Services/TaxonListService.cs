using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.Services
{
    /// <summary>
    /// Taxon list service
    /// </summary>
    public class TaxonListService : ITaxonListService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly TaxonListServiceConfiguration _taxonListServiceConfiguration;
        private readonly ILogger<TaxonListService> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClientService"></param>
        /// <param name="taxonListServiceConfiguration"></param>
        /// <param name="logger"></param>
        public TaxonListService(
            IHttpClientService httpClientService,
            TaxonListServiceConfiguration taxonListServiceConfiguration,
            ILogger<TaxonListService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _taxonListServiceConfiguration = taxonListServiceConfiguration ??
                                             throw new ArgumentNullException(nameof(taxonListServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<ConservationList>> GetDefinitionsAsync()
        {
            try
            {
                var response = await _httpClientService.GetDataAsync<ConservationListsResult>(
                    new Uri($"{ _taxonListServiceConfiguration.BaseAddress }/definitions"));

                return response.ConservationLists;
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get conservation lists definition", e);
            }

            return null;
        }

        public async Task<List<NatureConservationListTaxa>> GetTaxaAsync(IEnumerable<int> conservationListIds)
        {
            try
            {
                var response = await _httpClientService.PostDataAsync<NatureConservationListTaxaResult>(
                    new Uri($"{ _taxonListServiceConfiguration.BaseAddress }/taxa"),
                        new { conservationListIds = conservationListIds, outputFields = new[] { "id", "scientificname", "swedishname" } });

                return response.NatureConservationListTaxa;
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get conservation lists taxa", e);
            }

            return null;
        }
    }
}
