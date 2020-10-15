using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.Managers.Interfaces;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Models.DataCite;
using SOS.Lib.Models.Search;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;

namespace SOS.Export.Jobs
{
    /// <summary>
    ///     Artportalen harvest
    /// </summary>
    public class CreateDoiJob : ICreateDoiJob
    {
        private readonly IObservationManager _observationManager;
        private readonly IDataCiteService _dataCiteService;
        private readonly IDataProviderRepository _dataProviderRepository;
        private readonly string _doiContainer;
        private readonly DOIConfiguration _doiConfiguration;
        private readonly ILogger<ExportToDoiJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="dataCiteService"></param>
        /// <param name="dataProviderRepository"></param>
        /// <param name="configuration"></param>
        /// <param name="doiConfiguration"></param>
        /// <param name="logger"></param>
        public CreateDoiJob(IObservationManager observationManager, 
            IDataCiteService dataCiteService,
            IDataProviderRepository dataProviderRepository,
            BlobStorageConfiguration configuration,
            DOIConfiguration doiConfiguration,
            ILogger<ExportToDoiJob> logger)
        {
            _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
            _dataCiteService = dataCiteService ?? throw new ArgumentNullException(nameof(dataCiteService));
            _dataProviderRepository = dataProviderRepository ?? throw  new ArgumentNullException(nameof(dataProviderRepository));
            _doiContainer = configuration?.Containers["doi"] ?? throw new ArgumentNullException(nameof(configuration));
            _doiConfiguration = doiConfiguration ?? throw new ArgumentNullException(nameof(doiConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [DisplayName("Create a DwC-A file using passed filter and give it a DOI")]
        public async Task<bool> RunAsync(ExportFilter filter, string emailAddress, IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Start creating DOI job");

                var descriptions = _doiConfiguration.Descriptions as List<DOIDescription> ?? new List<DOIDescription>();
                descriptions.Add(new DOIDescription
                {
                    Description = JsonSerializer.Serialize(filter, new JsonSerializerOptions { IgnoreNullValues = true }),
                    DescriptionType = DescriptionType.Other
                });

                var dataProviders = (await _dataProviderRepository.GetAllAsync())?.Where(dp => dp.IsActive);
                if (filter.DataProviderIds?.Any() ?? false)
                {
                    dataProviders = dataProviders?.Where(dp => dp.IsActive && filter.DataProviderIds.Contains(dp.Id)).ToList();
                }

                var metaData = new DOIMetadata
                {
                    Attributes = new DOIAttributes
                    {
                        Contributors = dataProviders?.Select(dp => new DOIContributor
                        {
                            ContributorType = ContributorType.DataCollector,
                            Name = dp.Name,
                            NameType = NameType.Organizational
                        }),
                        Creators = new[]
                        {
                            _doiConfiguration.Creator
                        },
                        Descriptions = descriptions,
                        Formats = _doiConfiguration.Formats,
                        PublicationYear = DateTime.Now.Year,
                        Titles = new[] { new DOITitle { Title = $"Occurrence records download on {DateTime.Now.ToString("yyyy-MM-dd")}" } },
                        Publisher = _doiConfiguration.Publisher,
                        Subjects = _doiConfiguration.Subjects,
                        Types = _doiConfiguration.Types
                    },
                    Type = "dois"
                };

                _logger.LogDebug("Start creating DOI draft");
                metaData = await _dataCiteService.CreateDoiDraftAsync(metaData);
                _logger.LogDebug("Finish creating DOI draft");

                var success = false;

                if (metaData != null)
                {
                    _logger.LogDebug($"Start creating DOI file ({metaData.Id})");
                    success = await _observationManager.ExportAndStoreAsync(filter, _doiContainer, metaData.Id,
                        emailAddress, cancellationToken);
                    _logger.LogDebug($"Finish creating DOI file ({metaData.Id})");

                    if (success)
                    {
                        metaData.Attributes.Url = $"{_doiConfiguration.Url}/{metaData.Id}";
                        metaData.Attributes.Identifiers = new[]
                        {
                            new DOIIdentifier
                            {
                                Identifier = $"https://doi.org/{metaData.Id}",
                                IdentifierType = "DOI"
                            }
                        };

                        _logger.LogDebug($"Start publishing DOI ({metaData.Id})");
                        success = await _dataCiteService.PublishDoiAsync(metaData);
                        _logger.LogDebug($"Finish publishing DOI ({metaData.Id})");
                    }
                }
                _logger.LogInformation($"Finish creating DOI job. Success: {success}");

                return success ? true : throw new Exception("Creating DOI job failed");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Creating DOI job was cancelled.");
            }
            catch (Exception e)
            {
                _logger.LogInformation("Failed to create DOI.", e);
            }
            return false;
        }
    }
}