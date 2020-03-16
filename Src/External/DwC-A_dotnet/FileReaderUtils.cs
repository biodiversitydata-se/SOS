using System;
using System.Linq;

namespace DwC_A
{
    internal static class FileReaderUtils
    {
        public static void ValidateLineEnds(string linesTerminatedBy)
        {
            if (new[] { "\n", "r", "\r\n" }.Contains(linesTerminatedBy) == false)
            {
                throw new NotSupportedException($"Only files terminated by '\n', '\r' or '\r\n' are supported.");
            }
        }

    }
}
