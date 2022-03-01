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
            _threadHandler = new SemaphoreSlim(processConfiguration?.NoOfThreads ?? throw new ArgumentNullException(nameof(processConfiguration)));
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
