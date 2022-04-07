using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Configuration.Process;

namespace SOS.Harvest.Managers
{
    public class ProcessManager : IProcessManager
    {
        private readonly SemaphoreSlim _threadHandler;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processConfiguration"></param>
        public ProcessManager(ProcessConfiguration processConfiguration)
        {
            var noOfThreads = processConfiguration?.NoOfThreads ?? throw new ArgumentNullException(nameof(processConfiguration));
            _threadHandler = new SemaphoreSlim(noOfThreads, noOfThreads);
        }

        /// <inheritdoc />
        public int Release()
        {
            return _threadHandler.Release(); ;
        }

        /// <inheritdoc />
        public async Task WaitAsync()
        {
            await _threadHandler.WaitAsync();
        }
    }
}
