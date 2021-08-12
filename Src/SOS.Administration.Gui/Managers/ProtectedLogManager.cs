using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Administration.Gui.Managers.Interfaces;
using SOS.Administration.Gui.Models;
using SOS.Lib.Models.Log;
using SOS.Lib.Repositories.Processed.Interfaces;

namespace SOS.Administration.Gui.Managers
{
    public class ProtectedLogManager : IProtectedLogManager
    {
        private readonly IProtectedLogRepository _protectedLogRepository;
        private readonly IProcessedObservationRepository _processedObservationRepository;
        private readonly ILogger<ProtectedLogManager> _logger;

        /// <summary>
        /// Get data for observations and populate log row
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        private async Task<ProtectedLogDto> PopulateLogAsync(ProtectedLog log)
        {
            var observations =
                await _processedObservationRepository.GetObservationsAsync(log.OccurenceIds,
                    new[]
                    {
                        "event.startDate",
                        "location.decimalLatitude",
                        "location.decimalLongitude",
                        "location.county",
                        "location.municipality",
                        "location.province",
                        "location.locality",
                        "taxon.id",
                        "taxon.vernacularName",
                        "taxon.scientificName",
                        "taxon.attributes.protectionLevel"
                    }, true);

            return new ProtectedLogDto
            {
                ApplicationIdentifier = log.ApplicationIdentifier,
                Ip = log.Ip,
                IssueDate = log.IssueDate,
                Observations = observations.Select(o => new ProtectedLogObservationDto
                {
                    IssueDate = o.Event?.StartDate,
                    County = o.Location?.County?.Name,
                    Latitude = o.Location?.DecimalLatitude,
                    Locality = o.Location?.Locality,
                    Longitude = o.Location?.DecimalLongitude,
                    Municipality = o.Location?.Municipality?.Name,
                    Province = o.Location?.Province?.Name,
                    TaxonCommonName = o.Taxon?.VernacularName,
                    TaxonId = o.Taxon?.Id,
                    TaxonProtectionLevel = o.Taxon?.Attributes?.ProtectionLevel?.Id,
                    TaxonScientificName = o.Taxon?.ScientificName
                }),
                User = log.User,
                UserId = log.UserId
            };
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="protectedLogRepository"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="logger"></param>
        public ProtectedLogManager(IProtectedLogRepository protectedLogRepository, 
            IProcessedObservationRepository processedObservationRepository, 
            ILogger<ProtectedLogManager> logger)
        {
            _protectedLogRepository =
                protectedLogRepository ?? throw new ArgumentNullException(nameof(protectedLogRepository));
            _processedObservationRepository = processedObservationRepository ??
                                                       throw new ArgumentNullException(
                                                           nameof(processedObservationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ProtectedLogDto>> SearchAsync(DateTime from, DateTime to)
        {
            var protectedLogs = await _protectedLogRepository.SearchAsync(from, to);

            var populateTasks = protectedLogs
                .Select(PopulateLogAsync)
                .ToArray();
            await Task.WhenAll(populateTasks);

            return populateTasks.Select(t => t.Result);
        }
    }
}
