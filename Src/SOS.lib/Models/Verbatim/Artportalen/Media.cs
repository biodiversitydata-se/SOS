using System;
using System.Collections.Generic;

namespace SOS.Lib.Models.Verbatim.Artportalen
{
    public class Media
    {
        /// <summary>
        /// Media comments
        /// </summary>
        public ICollection<MediaComment> Comments { get; set; }

        /// <summary>
        ///    Copyright text
        /// </summary>
        public string CopyrightText { get; set; }

        /// <summary>
        ///     Id of media.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Type of file
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// Uri to file
        /// </summary>
        public string FileUri { get; set; }

        /// <summary>
        /// Rights holder
        /// </summary>
        public string RightsHolder { get; set; }

        /// <summary>
        /// Media upload time
        /// </summary>
        public DateTime? UploadDateTime { get; set; }
    }
}
