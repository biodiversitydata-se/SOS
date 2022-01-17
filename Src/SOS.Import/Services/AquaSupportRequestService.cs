using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Services.Interfaces;

namespace SOS.Import.Services
{
    public class AquaSupportRequestService : IAquaSupportRequestService
    {
        private readonly IHttpClientService _httpClientService;
        private readonly ILogger<AquaSupportRequestService> _logger;
        private readonly SemaphoreSlim _semaphore;

        private DateTime? _lastRequestTime;
        private int _maxReturnedChanges;
        private const int TimeBetweenCalls = 2000;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="httpClientService"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AquaSupportRequestService(
            IHttpClientService httpClientService,
            ILogger<AquaSupportRequestService> logger)
        {
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _maxReturnedChanges = 10000;
            _semaphore = new SemaphoreSlim(1);
        }

        public async Task<XDocument> GetAsync(string baseUrl, DateTime startDate, DateTime endDate, long changeId)
        {
            try
            {
                await _semaphore.WaitAsync();
                
                var timeSinceLastCall = (DateTime.Now - (_lastRequestTime ?? DateTime.MinValue)).Milliseconds;
                if (timeSinceLastCall < TimeBetweenCalls)
                {
                    var sleepTime = TimeBetweenCalls - timeSinceLastCall;
                    Thread.Sleep(sleepTime);
                }

                _lastRequestTime = DateTime.Now;
                var xmlStream = await _httpClientService.GetFileStreamAsync(
                    new Uri($"{baseUrl}" +
                            $"&changedFrom={startDate.ToString("yyyy-MM-dd")}" +
                            $"&isChangedFromSpecified=true" +
                            $"&changedTo={endDate.ToString("yyyy-MM-dd")}" +
                            $"&isChangedToSpecified=true" +
                            $"&changeId={changeId}" +
                            $"&isChangedIdSpecified=true" +
                            $"&maxReturnedChanges={_maxReturnedChanges}"),
                    new Dictionary<string, string>(new[]
                        {
                            new KeyValuePair<string, string>("Accept", "application/xml"),
                        }
                    )
                );

                // If get stream failed, try to decrease max returned observations and try again
                if (xmlStream == null && _maxReturnedChanges > 1000)
                {
                    _maxReturnedChanges -= 1000;
                    return await GetAsync(baseUrl, startDate, endDate, changeId);
                }

                xmlStream.Seek(0, SeekOrigin.Begin);

                var xDocument = await XDocument.LoadAsync(xmlStream, LoadOptions.None, CancellationToken.None);

                return xDocument;
            }
            catch (Exception e)
            {
                _logger.LogError("Failed to get data from AquaSupport data", e);
                throw;
            }
            finally
            {
                // Release semaphore in order to let next thread start getting data 
                _semaphore.Release();
            }
        }
    }
}
