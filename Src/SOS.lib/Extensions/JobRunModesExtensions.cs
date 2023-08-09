using SOS.Lib.Enums;

namespace SOS.Lib.Extensions;
public static class JobRunModesExtensions
{
    public static string GetLoggerMode(this JobRunModes jobRunModes)
    {
        switch (jobRunModes)
        {
            case JobRunModes.Full:
                return "Full";
            case JobRunModes.IncrementalActiveInstance:
            case JobRunModes.IncrementalInactiveInstance:
                return "Incremental";
            default:
                return "Full";
        }
    }
}

