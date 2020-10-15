using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Export;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Models.DataCite;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;

namespace SOS.Export.Jobs
{
    /// <summary>
    ///     Artportalen harvest
    /// </summary>
    public class ExportToDoiJob : IExportToDoiJob
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly IDataCiteService _dataCiteService;
        private readonly IDataProviderRepository _dataProviderRepository;
        private readonly string _doiContainer;
        private readonly string _exportContainer;
        private readonly DOIConfiguration _doiConfiguration;
        private readonly ILogger<ExportToDoiJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="blobStorageService"></param>
        /// <param name="dataCiteService"></param>
        /// <param name="dataProviderRepository"></param>
        /// <param name="configuration"></param>
        /// <param name="doiConfiguration"></param>
        /// <param name="logger"></param>
        public ExportToDoiJob(IBlobStorageService blobStorageService, 
            IDataCiteService dataCiteService,
            IDataProviderRepository dataProviderRepository,
            BlobStorageConfiguration configuration,
            DOIConfiguration doiConfiguration,
            ILogger<ExportToDoiJob> logger)
        {
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _dataCiteService = dataCiteService ?? throw new ArgumentNullException(nameof(dataCiteService));
            _dataProviderRepository = dataProviderRepository ?? throw new ArgumentNullException(nameof(dataProviderRepository));
            _doiContainer = configuration?.Containers["doi"] ?? throw new ArgumentNullException(nameof(configuration));
            _exportContainer = configuration.Containers["export"];
            _doiConfiguration = doiConfiguration ?? throw new ArgumentNullException(nameof(doiConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [DisplayName("Copy export file to DOI repository and give it a DOI")]
        public async Task<bool> RunAsync(string fileName, IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Start export to DOI job");

                //Get file suffix
                var fileSuffix = Regex.Match(fileName, @"\.[0-9a-z]+$").Value;

                var dataProviders = (await _dataProviderRepository.GetAllAsync())?.Where(dp => dp.IsActive);
                
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
                        Descriptions = _doiConfiguration.Descriptions,
                        Formats = _doiConfiguration.Formats,
                        PublicationYear = DateTime.Now.Year,
                        Suffix = $"{fileName.Replace(fileSuffix, "")}-{Guid.NewGuid()}",
                        Titles = new[] { new DOITitle { Title = $"Occurrence records on {DateTime.Now.ToString("yyyy-MM-dd")}" } },
                        Publisher = _doiConfiguration.Publisher,
                        Subjects = _doiConfiguration.Subjects,
                        Types = _doiConfiguration.Types
                    },
                    Type = "dois"
                };

                _logger.LogDebug($"Start creating DOI draft");
                metaData = await _dataCiteService.CreateDoiDraftAsync(metaData);
                _logger.LogDebug($"Finish creating DOI draft");

                var success = false;

                if (metaData != null)
                {
                    var targetFileName = $"{metaData.Id}{fileSuffix}";

                    _logger.LogDebug($"Start copy file ({fileName}) from {_exportContainer} to {_doiContainer}/{targetFileName}");

                    success = await _blobStorageService.CopyFileAsync(_exportContainer, fileName, _doiContainer,
                        targetFileName);
                    _logger.LogDebug($"Finish copy file ({fileName}) from {_exportContainer} to {_doiContainer}/{targetFileName}");

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
                _logger.LogInformation($"End export to DOI job. Success: {success}");

                return success ? true : throw new Exception("Export to DOI job failed");
            }
            catch (JobAbortedException)
            {
                _logger.LogInformation("Export to DOI job was cancelled.");
            }
            catch (Exception e)
            {
                _logger.LogInformation("Failed to create DOI from export.", e);
            }
            return false;
        }
    }
}