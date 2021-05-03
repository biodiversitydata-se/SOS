using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Services.Interfaces;

namespace SOS.Import.Services
{
    public class KulObservationService : IKulObservationService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly KulServiceConfiguration _kulServiceConfiguration;
        private readonly ILogger<KulObservationService> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="speciesObservationChangeServiceClient"></param>
        /// <param name="kulServiceConfiguration"></param>
        /// <param name="logger"></param>
        public KulObservationService(
            IHttpClientService httpClientService,
            KulServiceConfiguration kulServiceConfiguration,
            ILogger<KulObservationService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _kulServiceConfiguration = kulServiceConfiguration ??
                                       throw new ArgumentNullException(nameof(kulServiceConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<XDocument> GetAsync(long changeId)
        {
            try
            {
                var xmlStream = await _httpClientService.GetFileStreamAsync(
                    new Uri($"{_kulServiceConfiguration.BaseAddress}/api/v1/KulSpeciesObservation/?token={_kulServiceConfiguration.Token}" +
                            $"&changedFrom={ new DateTime(_kulServiceConfiguration.StartHarvestYear,1,1).ToString("yyyy-MM-dd")  }" +
                            $"&isChangedFromSpecified=false" +
                            $"&changedTo={ DateTime.Now.ToString("yyyy-MM-dd") }" +
                            $"&isChangedToSpecified=false" +
                            $"&changeId={changeId}" +
                            $"&isChangedIdSpecified=true" +
                            $"&maxReturnedChanges={_kulServiceConfiguration.MaxReturnedChangesInOnePage}"),
                    new Dictionary<string, string>(new[]
                        {
                            new KeyValuePair<string, string>("Accept", _kulServiceConfiguration.AcceptHeaderContentType),
                        }
                        )
                    );

                xmlStream.Seek(0, SeekOrigin.Begin);

                var xDocument = await XDocument.LoadAsync(xmlStream, LoadOptions.None, CancellationToken.None);

                return xDocument;
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get data from KUL", e);
               throw;
            }
        }
    }
}