using Microsoft.Extensions.Logging;
using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Configuration.Process;

namespace SOS.Harvest.Processors
{
    public abstract class ProcessorBase<TClass>
    {
        protected readonly IProcessManager ProcessManager;
        protected readonly IProcessTimeManager TimeManager;
        protected readonly ProcessConfiguration ProcessConfiguration;
        protected readonly ILogger<TClass> Logger;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processManager"></param>
        /// <param name="processTimeManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected ProcessorBase(
            IProcessManager processManager,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration,
            ILogger<TClass> logger)
        {
            ProcessManager = processManager ?? throw new ArgumentNullException(nameof(processManager));
            TimeManager = processTimeManager ?? throw new ArgumentNullException(nameof(processTimeManager));
            ProcessConfiguration = processConfiguration ?? throw new ArgumentNullException(nameof(processConfiguration));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
    }
}