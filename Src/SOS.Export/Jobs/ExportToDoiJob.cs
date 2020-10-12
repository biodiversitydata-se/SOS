using System;
using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Models.DataCite;
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
        private readonly string _doiContainer;
        private readonly string _exportContainer;
        private readonly ILogger<ExportToDoiJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="blobStorageService"></param>
        /// <param name="dataCiteService"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public ExportToDoiJob(IBlobStorageService blobStorageService, IDataCiteService dataCiteService, 
            BlobStorageConfiguration configuration, ILogger<ExportToDoiJob> logger)
        {
            _blobStorageService = blobStorageService ?? throw new ArgumentNullException(nameof(blobStorageService));
            _dataCiteService = dataCiteService ?? throw new ArgumentNullException(nameof(dataCiteService));
            _doiContainer = configuration?.Containers["doi"] ?? throw new ArgumentNullException(nameof(configuration));
            _exportContainer = configuration.Containers["export"];

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

                var metaData = new DOIMetadata
                {
                    Attributes = new DOIAttributes
                    {
                        Creators = new[]
                        {
                            new DOICreator
                            {
                                Name = "Artdatabanken",
                                NameType = CreatorNameType.Organizational
                            }
                        },
                        Descriptions = new[] {
                            new DOIDescription
                            {
                                Description = fileName,
                                DescriptionType = DescriptionType.Other
                            }
                        },
                        PublicationYear = DateTime.Now.Year, 
                        Titles = new[] { new DOITitle{ Title = $"{fileName} {DateTime.Now.ToLocalTime().ToShortDateString()}" }  },
                        Publisher = "Artdatabanken",
                        Subjects = new[]
                        {
                            new DOISubject
                            {
                                Subject = "Biological sciences"
                            }
                        },
                        Suffix = $"{fileName.Replace(fileSuffix, "")}-{Guid.NewGuid()}",
                        Types = new DOITypes()
                        {
                            ResourceTypeGeneral = "Dataset",
                            ResourceType ="Observations"
                        }
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