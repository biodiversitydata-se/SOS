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
    public class NorsObservationService : INorsObservationService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly ILogger<NorsObservationService> _logger;
        private readonly NorsServiceConfiguration _norsServiceConfiguration;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="norsServiceConfiguration"></param>
        public NorsObservationService(
            IHttpClientService httpClientService,
            NorsServiceConfiguration norsServiceConfiguration,
            ILogger<NorsObservationService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _norsServiceConfiguration = norsServiceConfiguration ??
                                        throw new ArgumentNullException(nameof(norsServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<XDocument> GetAsync(long changeId)
        {
            try
            {
                var xmlStream = await _httpClientService.GetFileStreamAsync(
                    new Uri($"{_norsServiceConfiguration.BaseAddress}/api/v1/NorsSpeciesObservation/?token={_norsServiceConfiguration.Token}" +
                            $"&changedFrom=1900-01-01" +
                            $"&isChangedFromSpecified=false" +
                            $"&changedTo=1900-01-01" +
                            $"&isChangedToSpecified=false" +
                            $"&changeId={changeId}" +
                            $"&isChangedIdSpecified=true" +
                            $"&maxReturnedChanges={_norsServiceConfiguration.MaxReturnedChangesInOnePage}"),
                    new Dictionary<string, string>(new[]
                        {
                            new KeyValuePair<string, string>("Accept", _norsServiceConfiguration.AcceptHeaderContentType),
                        }
                    )
                );

                xmlStream.Seek(0, SeekOrigin.Begin);

                var xDocument = await XDocument.LoadAsync(xmlStream, LoadOptions.None, CancellationToken.None);

                return xDocument;
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get data from NORS", e);
                return null;
            }
        }
    }
}