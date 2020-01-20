using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Import.Models.TaxonAttributeService;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Constants;

namespace SOS.Import.Services
{
    /// <summary>
    /// Species data service
    /// </summary>
    public class TaxonAttributeService : ITaxonAttributeService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly TaxonAttributeServiceConfiguration _taxonAttributeServiceConfiguration;
        private readonly ILogger<TaxonAttributeService> _logger;

        /// <summary>
        /// Constructor
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
            _taxonAttributeServiceConfiguration = taxonAttributeServiceConfiguration ?? throw new ArgumentNullException(nameof(taxonAttributeServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Create header data dictionary
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetHeaderData()
        {
            return new Dictionary<string, string>
            {
                {"currentUser", "SOS_ServiceUser" },
                {"culture", Cultures.sv_SE}
            };
        }

        /// <inheritdoc />
        public async Task<int> GetCurrentRedlistPeriodIdAsync()
        {
            try
            {
                var redlistPeriod = await _httpClientService.GetDataAsync<dynamic>(
                    new Uri($"{_taxonAttributeServiceConfiguration.BaseAddress}/Periods/CurrentRedlistPeriod"), GetHeaderData());

                return redlistPeriod.id;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get current redlist period id");
                return -1;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<TaxonAttributeModel>> GetTaxonAttributesAsync(IEnumerable<int> taxonIds, IEnumerable<int> factorIds, IEnumerable<int> periodIds)
        {
            try
            {
                return await _httpClientService.PostDataAsync<IEnumerable<TaxonAttributeModel>>(
                    new Uri($"{_taxonAttributeServiceConfiguration.BaseAddress}/Taxa"), new { taxonIds, factorIds, periodIds, qualityIds = new[] { 1, 2, 3 } }, GetHeaderData());
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to get taxon attributes");
                return null;
            }
        }
    }
}
