using System;

namespace SOS.Lib.Models.Misc
{
    /// <summary>
    /// File object
    /// </summary>
    public class File
    {
        /// <summary>
        /// Name Of file
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Date time when file was created
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// File size in bytes
        /// </summary>
        public long Size { get; set; }
    }
}
