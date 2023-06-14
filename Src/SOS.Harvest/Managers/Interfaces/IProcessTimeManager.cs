namespace SOS.Harvest.Managers.Interfaces
{
    public interface IProcessTimeManager
    {
        /// <summary>
        /// Get timer total time
        /// </summary>
        /// <param name="timerType"></param>
        /// <returns></returns>
        ProcessTimeManager.Timer? GetTimer(ProcessTimeManager.TimerTypes timerType);

        /// <summary>
        /// Get all timers
        /// </summary>
        /// <returns></returns>
        IDictionary<ProcessTimeManager.TimerTypes, ProcessTimeManager.Timer> GetTimers();

        /// <summary>
        /// Start timer
        /// </summary>
        /// <param name="timerType"></param>
        /// <returns></returns>
        Guid Start(ProcessTimeManager.TimerTypes timerType);

        /// <summary>
        /// Stop timer
        /// </summary>
        /// <param name="timerType"></param>
        /// <param name="id"></param>
        void Stop(ProcessTimeManager.TimerTypes timerType, Guid id);
    }
}
