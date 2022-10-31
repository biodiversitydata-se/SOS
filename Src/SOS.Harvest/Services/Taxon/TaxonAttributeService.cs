﻿using Microsoft.Extensions.Logging;
using SOS.Harvest.Services.Taxon.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Constants;
using SOS.Lib.Models.TaxonAttributeService;
using SOS.Lib.Services.Interfaces;

namespace SOS.Harvest.Services.Taxon
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class TaxonAttributeService : ITaxonAttributeService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly ILogger<TaxonAttributeService> _logger;
        private readonly TaxonAttributeServiceConfiguration _taxonAttributeServiceConfiguration;

        private async Task<TaxonAttributeResponse> GetTaxonAttributesAsync(IEnumerable<int> taxonIds,
            IEnumerable<int> factorIds, IEnumerable<int> periodIds, int attempt)
        {
            try
            {
                return await _httpClientService.PostDataAsync<TaxonAttributeResponse>(
                    new Uri($"{_taxonAttributeServiceConfiguration.BaseAddress}/TaxonAttributes?pageSize={int.MaxValue}&page=1"),
                    new { taxonIds, factorIds, periodIds, qualityIds = new[] { 1, 2, 3 } }, GetHeaderData());
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to get taxon attributes attempt: {attempt}");

                if (attempt > 2)
                {
                    return null!;
                }

                Thread.Sleep(attempt * 500);
                return await GetTaxonAttributesAsync(taxonIds, factorIds, periodIds, ++attempt);
            }
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="httpClientService"></param>
        /// <param name="taxonAttributeServiceConfiguration"></param>
        /// <param name="logger"></param>
        public TaxonAttributeService(
            IHttpClientService httpClientService,
            TaxonAttributeServiceConfiguration taxonAttributeServiceConfiguration,
            ILogger<TaxonAttributeService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _taxonAttributeServiceConfiguration = taxonAttributeServiceConfiguration ??
                                                  throw new ArgumentNullException(
                                                      nameof(taxonAttributeServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<int> GetCurrentRedlistPeriodIdAsync()
        {
            try
            {
                var redlistPeriod = await _httpClientService.GetDataAsync<RedlistPeriod>(
                    new Uri($"{_taxonAttributeServiceConfiguration.BaseAddress}/Periods/CurrentRedlistPeriod"),
                    GetHeaderData());

                return redlistPeriod.Id;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get current redlist period id");
                return -1;
            }
        }

        /// <inheritdoc />
        public async Task<TaxonAttributeResponse> GetTaxonAttributesAsync(IEnumerable<int> taxonIds,
            IEnumerable<int> factorIds, IEnumerable<int> periodIds)
        {
            return await GetTaxonAttributesAsync(taxonIds, factorIds, periodIds, 1);
        }

        /// <summary>
        ///     Create header data dictionary
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetHeaderData()
        {
            return new Dictionary<string, string>
            {
                {"currentUser", "SOS_ServiceUser"},
                {"culture", Cultures.sv_SE}
            };
        }
    }
}