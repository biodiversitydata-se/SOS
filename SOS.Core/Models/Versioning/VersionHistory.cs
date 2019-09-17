using System;

namespace SOS.Core.Models.Versioning
{
    public class VersionHistory
    {
        public int Version { get; set; }
        public DateTime UtcDate { get; set; }
        public bool IsDeleted { get; set; }
        public string Diff { get; set; }
        public string Type { get; set; }

        public VersionHistory(string diff)
        {
            UtcDate = DateTime.UtcNow;
            Diff = diff;
        }
    }
}
