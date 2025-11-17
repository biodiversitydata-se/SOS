using SOS.Lib.Enums;

namespace SOS.Lib.Extensions;
public static class JobRunModesExtensions
{
    extension(JobRunModes jobRunModes)
    {
        public string GetLoggerMode()
        {
            switch (jobRunModes)
            {
                case JobRunModes.Full:
                    return "Full";
                case JobRunModes.IncrementalActiveInstance:
                    return "Incremental";
                case JobRunModes.IncrementalInactiveInstance:
                    return "Full";
                default:
                    return "Full";
            }
        }
    }
}

