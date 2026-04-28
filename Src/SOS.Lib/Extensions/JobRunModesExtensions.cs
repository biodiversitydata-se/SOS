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
                case JobRunModes.IncrementalActiveInstance:
                    return "Incremental";
                default:
                    return "Full";
            }
        }
    }
}

