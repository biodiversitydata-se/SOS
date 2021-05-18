using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SOS.Import.Harvesters.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.TaxonListService;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Services.Interfaces;

namespace SOS.Import.Harvesters
{
    /// <summary>
    ///     Class for harvest taxon lists.
    /// </summary>
    public class TaxonListHarvester : ITaxonListHarvester
    {
        private readonly ITaxonListService _taxonListService;
        private readonly ITaxonListRepository _taxonListRepository;
        private readonly SosApiConfiguration _sosApiConfiguration;
        private readonly ILogger<TaxonListHarvester> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="taxonListService"></param>
        /// <param name="taxonListRepository"></param>
        /// <param name="sosApiConfiguration"></param>
        /// <param name="logger"></param>
        public TaxonListHarvester(
            ITaxonListService taxonListService,
            ITaxonListRepository taxonListRepository,
            SosApiConfiguration sosApiConfiguration,
            ILogger<TaxonListHarvester> logger)
        {
            _taxonListService = taxonListService ?? throw new ArgumentNullException(nameof(taxonListService));
            _taxonListRepository = taxonListRepository ?? throw new ArgumentNullException(nameof(taxonListRepository));
            _sosApiConfiguration = sosApiConfiguration ?? throw new ArgumentNullException(nameof(sosApiConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task ClearTaxonListsCache()
        {
            try
            {
                var client = new HttpClient();
                foreach (var observationsApiAddress in _sosApiConfiguration.ObservationsApiAddresses)
                {
                    var requestUri = $"{observationsApiAddress}Caches?cache={nameof(Cache.TaxonLists)}";
                    var response = await client.DeleteAsync(requestUri);
                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"Taxon lists cache cleared ({observationsApiAddress})");
                    }
                    else
                    {
                        _logger.LogInformation($"Failed to clear taxon lists cache ({observationsApiAddress})");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to clear taxon lists cache");
            }
        }

        public async Task<HarvestInfo> HarvestTaxonListsAsync()
        {
            var harvestInfo = new HarvestInfo(DateTime.Now) { Id = nameof(TaxonList) };
            try
            {
                _logger.LogDebug("Start getting taxon lists");

                var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var filePath = Path.Combine(assemblyPath, @"Resources\TaxonLists.json");
                var taxonLists =
                    JsonConvert.DeserializeObject<List<TaxonList>>(await File.ReadAllTextAsync(filePath));
                var taxonListByTaxonListServiceId = taxonLists.ToDictionary(m => m.TaxonListServiceId, m => m);
                var conservationLists =
                    await _taxonListService.GetTaxaAsync(taxonLists.Select(m => m.TaxonListServiceId));
                foreach (var conservationList in conservationLists)
                {
                    taxonListByTaxonListServiceId[conservationList.ListId].Taxa =
                        conservationList.TaxonInformation.Select(ConvertToTaxonListTaxonInformation);
                }

                // Check that data seems to be correct
                bool dataIsOk = conservationLists.Count != 0;
                if (dataIsOk)
                {
                    foreach (var list in conservationLists)
                    {
                        if (list.TaxonInformation == null || list.TaxonInformation.Count == 0)
                        {
                            dataIsOk = false;
                            break;
                        }
                    }
                }

                if (!dataIsOk)
                {
                    harvestInfo.Status = RunStatus.Failed;
                    return harvestInfo;
                }

                await _taxonListRepository.DeleteCollectionAsync();
                await _taxonListRepository.AddCollectionAsync();
                await _taxonListRepository.AddManyAsync(taxonLists);
                await ClearTaxonListsCache();

                // Update harvest info
                harvestInfo.End = DateTime.Now;
                harvestInfo.Status = RunStatus.Success;
                harvestInfo.Count = taxonLists?.Count() ?? 0;

                _logger.LogDebug("Adding taxon lists succeeded");
                return harvestInfo;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed harvest of taxon lists");
                harvestInfo.Status = RunStatus.Failed;
            }

            return harvestInfo;
        }

        private TaxonListTaxonInformation ConvertToTaxonListTaxonInformation(TaxonInformation taxonInformation)
        {
            return new TaxonListTaxonInformation()
            {
                Id = taxonInformation.Id,
                ScientificName = taxonInformation.ScientificName,
                SwedishName = taxonInformation.SwedishName
            };
        }
    }
}