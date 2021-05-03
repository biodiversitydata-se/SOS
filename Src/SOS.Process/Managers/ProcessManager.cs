using System;
using System.Threading;
using System.Threading.Tasks;
using SOS.Lib.Configuration.Process;
using SOS.Process.Managers.Interfaces;

namespace SOS.Process.Managers
{
    public class ProcessManager : IProcessManager
    {
        private readonly SemaphoreSlim _threadHandler;

        public ProcessManager(ProcessConfiguration processConfiguration)
        {
            _threadHandler = new SemaphoreSlim(processConfiguration?.NoOfThreads ?? throw new ArgumentNullException(nameof(processConfiguration)));
        }

        /// <inheritdoc />
        public int Release()
        {
            return _threadHandler.Release();
        }

        /// <inheritdoc />
        public async Task WaitAsync()
        {
            await _threadHandler.WaitAsync();
        }
    }
}
