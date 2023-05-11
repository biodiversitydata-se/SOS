using System.Collections.Concurrent;
using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Configuration.Process;

namespace SOS.Harvest.Managers
{
    public class ProcessTimeManager : IProcessTimeManager
    {
        private readonly bool _enabled;
        private readonly ConcurrentDictionary<TimerTypes, Timer> _timers;

        public class Timer
        {
            private long _sessionCount;
            private long _totalDuration;
            private readonly ConcurrentDictionary<Guid, DateTime> _timerSession;

            /// <summary>
            /// Constructor
            /// </summary>
            public Timer()
            {
                _sessionCount = 0;
                _totalDuration = 0;
                _timerSession = new ConcurrentDictionary<Guid, DateTime>();
            }

            public void EndSession(Guid id)
            {
                if (_timerSession.TryRemove(id, out var startTime))
                {
                    Interlocked.Add(ref _totalDuration, (long)(DateTime.Now - startTime).TotalMilliseconds);
                }
            }

            public Guid StartSession()
            {
                var id = Guid.NewGuid();

                while (!_timerSession.TryAdd(id, DateTime.Now))
                {
                    Thread.Sleep(10);
                }
                Interlocked.Increment(ref _sessionCount);
                return id;
            }

            public TimeSpan AverageDuration => TimeSpan.FromMilliseconds(_sessionCount == 0 ? 0 :_totalDuration / _sessionCount);

            public long SessionCount => _sessionCount;

            public TimeSpan TotalDuration => TimeSpan.FromMilliseconds(_totalDuration);
        }

        public enum TimerTypes
        {
            Diffuse,
            CoordinateConversion,
            CsvWrite,
            DwCCreation,
            ElasticsearchDelete,
            ElasticSearchRead,
            ElasticSearchWrite,
            MongoDbRead,
            MongoDbWrite,
            ProcessObservation,
            ProcessOverall,
            ValidateObservations,
            ValidateIndex
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="processConfiguration"></param>
        public ProcessTimeManager(ProcessConfiguration processConfiguration)
        {
            _enabled = processConfiguration?.EnableTimeManager ?? throw new ArgumentNullException(nameof(processConfiguration));
            _timers = new ConcurrentDictionary<TimerTypes, Timer>();
        }

        /// <inheritdoc />
        public Timer GetTimer(TimerTypes timerType)
        {
            if (_timers.TryGetValue(timerType, out var timer))
            {
                return timer;
            }

            return null;
        }

        /// <summary>
        /// Get all timers
        /// </summary>
        /// <returns></returns>
        public IDictionary<TimerTypes, Timer> GetTimers()
        {
            return _timers;
        }

        /// <inheritdoc />
        public Guid Start(TimerTypes timerType)
        {
            if (!_enabled)
            {
                return Guid.Empty;
            }

            if (!_timers.TryGetValue(timerType, out var timer))
            {
                timer = new Timer();
                _timers.TryAdd(timerType, timer);
            }

            return timer.StartSession();
        }

        /// <inheritdoc />
        public void Stop(TimerTypes timerType, Guid id)
        {
            if (!_enabled)
            {
                return;
            }

            if (!_timers.TryGetValue(timerType, out var timer))
            {
                return;
            }

            timer.EndSession(id);
        }
    }
}
