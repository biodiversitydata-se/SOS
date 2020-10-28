using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Managers.Interfaces;
using SOS.Process.Managers.Interfaces;

namespace SOS.Process.Jobs
{
    /// <summary>
    ///     Artportalen harvest
    /// </summary>
    public class CopyProviderDataJob : ICopyProviderDataJob
    {
        private readonly IDataProviderManager _dataProviderManager;
        private readonly IInstanceManager _instanceManager;
        private readonly ILogger<CopyProviderDataJob> _logger;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="instanceManager"></param>
        /// <param name="dataProviderManager"></param>
        /// <param name="logger"></param>
        public CopyProviderDataJob(
            IInstanceManager instanceManager,
            IDataProviderManager dataProviderManager,
            ILogger<CopyProviderDataJob> logger)
        {
            _instanceManager = instanceManager ?? throw new ArgumentNullException(nameof(instanceManager));
            _dataProviderManager = dataProviderManager ?? throw new ArgumentNullException(nameof(dataProviderManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [DisplayName("Copy provider data from active to inactive instance")]
        public async Task<bool> RunAsync(int dataProviderId)
        {
            var dataProvider = await _dataProviderManager.GetDataProviderByIdAsync(dataProviderId);
            if (dataProvider == null)
            {
                throw new Exception($"Data provider with Id={dataProviderId} is not found");
            }

            // Activate passed instance
            var success = await _instanceManager.CopyProviderDataAsync(dataProvider);
            return success ? true : throw new Exception("Copy provider data job failed");
        }
    }
}