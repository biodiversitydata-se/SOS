using System;
using System.ComponentModel;
using System.Text.Json;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Server;
using Microsoft.Extensions.Logging;
using SOS.Export.Managers.Interfaces;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Export;
using SOS.Lib.Models.DataCite;
using SOS.Lib.Models.Search;
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
        private readonly string _doiContainer;
        private readonly ILogger<ExportToDoiJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="dataCiteService"></param>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public CreateDoiJob(IObservationManager observationManager, IDataCiteService dataCiteService, 
            BlobStorageConfiguration configuration, ILogger<ExportToDoiJob> logger)
        {
            _observationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
            _dataCiteService = dataCiteService ?? throw new ArgumentNullException(nameof(dataCiteService));
            _doiContainer = configuration?.Containers["doi"] ?? throw new ArgumentNullException(nameof(configuration));

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [DisplayName("Create a DwC-A file using passed filter and give it a DOI")]
        public async Task<bool> RunAsync(ExportFilter filter, IJobCancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Start creating DOI job");

                _logger.LogDebug("Start creating DOI draft");

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
                                Description = JsonSerializer.Serialize(filter, new JsonSerializerOptions{ IgnoreNullValues = true }),
                                DescriptionType = DescriptionType.Other
                            }
                        },
                        PublicationYear = DateTime.Now.Year, 
                        Titles = new[] { new DOITitle{ Title = "User created DOI"}  },
                        Publisher = "Artdatabanken",
                        Subjects = new[]
                        {
                            new DOISubject
                            {
                                Subject = "Biological sciences"
                            }
                        },
                        Types = new DOITypes()
                        {
                            ResourceTypeGeneral = "Dataset",
                            ResourceType = "Observations"
                        }
                    },
                    Type = "dois"
                };

                metaData = await _dataCiteService.CreateDoiDraftAsync(metaData);
                _logger.LogDebug("Finish creating DOI draft");

                var success = false;

                if (metaData != null)
                {
                    _logger.LogDebug($"Start creating DOI file ({metaData.Id})");
                    success = await _observationManager.ExportAndStoreAsync(filter, _doiContainer, metaData.Id,
                        cancellationToken);
                    _logger.LogDebug($"Finish creating DOI file ({metaData.Id})");

                    if (success)
                    {
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