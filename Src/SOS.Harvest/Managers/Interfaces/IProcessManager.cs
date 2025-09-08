namespace SOS.Harvest.Managers.Interfaces
{
    public interface IProcessManager
    {
        /// <summary>
        /// Release Thread
        /// </summary>
        /// <returns></returns>
        int Release(string context);

        /// <summary>
        /// Wait for thread to finish
        /// </summary>
        /// <returns></returns>
        Task<bool> WaitAsync(string context);
    }
}
