using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;

namespace SOS.Import.Services
{
    public class SersObservationService : ISersObservationService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly ILogger<SersObservationService> _logger;
        private readonly SersServiceConfiguration _sersServiceConfiguration;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="sersServiceConfiguration"></param>
        public SersObservationService(
            IHttpClientService httpClientService,
            SersServiceConfiguration sersServiceConfiguration,
            ILogger<SersObservationService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _sersServiceConfiguration = sersServiceConfiguration ??
                                        throw new ArgumentNullException(nameof(sersServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<XDocument> GetAsync(long changeId)
        {
            try
            {
                var xmlStream = await _httpClientService.GetFileStreamAsync(
                    new Uri($"{_sersServiceConfiguration.BaseAddress}/api/v1/SersSpeciesObservation/?token={_sersServiceConfiguration.Token}" +
                            $"&changedFrom={ new DateTime(_sersServiceConfiguration.StartHarvestYear, 1, 1).ToString("yyyy-MM-dd") }" +
                            $"&isChangedFromSpecified=false" +
                            $"&changedTo={ DateTime.Now.ToString("yyyy-MM-dd") } " +
                            $"&isChangedToSpecified=false" +
                            $"&changeId={changeId}" +
                            $"&isChangedIdSpecified=true" +
                            $"&maxReturnedChanges={_sersServiceConfiguration.MaxReturnedChangesInOnePage}"),
                    new Dictionary<string, string>(new[]
                        {
                            new KeyValuePair<string, string>("Accept", _sersServiceConfiguration.AcceptHeaderContentType),
                        }
                    )
                );

                xmlStream.Seek(0, SeekOrigin.Begin);

                var xDocument = await XDocument.LoadAsync(xmlStream, LoadOptions.None, CancellationToken.None);

                return xDocument;
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get data from SERS", e);
                return null;
            }
        }
    }
}