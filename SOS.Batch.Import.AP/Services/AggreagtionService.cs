using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SOS.Batch.Import.AP.Factories.Interfaces;

namespace SOS.Batch.Import.AP.Services
{
    public class AggregationService : BackgroundService
    {
        private readonly ISightingFactory _sightingFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sightingFactory"></param>
        public AggregationService(ISightingFactory sightingFactory)
        {
            _sightingFactory = sightingFactory ?? throw new ArgumentNullException(nameof(sightingFactory));
        }

        /// <summary>
        /// Start 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected sealed override async Task ExecuteAsync(CancellationToken token)
        {
            if (!token.IsCancellationRequested)
            {
                await _sightingFactory.AggregateAsync();
            }
        }

        /// <summary>
        /// Stop
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override Task StopAsync(CancellationToken token)
        {
           return Task.FromCanceled(token);
        }
    }
}
