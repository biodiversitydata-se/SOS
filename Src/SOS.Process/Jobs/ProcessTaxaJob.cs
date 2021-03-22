using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Enums;
using SOS.Lib.Jobs.Process;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.ProcessInfo;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;
using SOS.Process.Processors.Taxon.Interfaces;

namespace SOS.Process.Jobs
{
    public class ProcessTaxaJob : ProcessJobBase, IProcessTaxaJob
    {
        private readonly ITaxonProcessor _taxonProcessor;
        private readonly ILogger<ProcessTaxaJob> _logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="taxonProcessor"></param>
        /// <param name="harvestInfoRepository"></param>
        /// <param name="processInfoRepository"></param>
        /// <param name="logger"></param>
        public ProcessTaxaJob(
            ITaxonProcessor taxonProcessor,
            IHarvestInfoRepository harvestInfoRepository,
            IProcessInfoRepository processInfoRepository,
            ILogger<ProcessTaxaJob> logger) : base(harvestInfoRepository, processInfoRepository)
        {
            _taxonProcessor = taxonProcessor ??
                              throw new ArgumentNullException(nameof(taxonProcessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        [DisplayName("Process taxa")]
        public async Task<bool> RunAsync()
        {
            var start = DateTime.Now;

            var taxaCount = await _taxonProcessor.ProcessTaxaAsync();
            var success = taxaCount >= 0;
            _logger.LogDebug("Start updating process info for taxa");

            await SaveProcessInfo(new ProcessInfo(nameof(Taxon), start)
            {
                PublicCount = taxaCount,
                End = DateTime.Now,
                Status = success ? RunStatus.Success : RunStatus.Failed
            });
           
            _logger.LogDebug("Finish updating process info for taxa");

            return success ? true : throw new Exception("Process taxa job failed");
        }
    }
}