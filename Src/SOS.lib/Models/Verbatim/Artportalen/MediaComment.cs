using System;

namespace SOS.Lib.Models.Verbatim.Artportalen
{
    public class MediaComment
    {
        /// <summary>
        /// Media comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// User making the comment
        /// </summary>
        public string CommentBy { get; set; }

        /// <summary>
        /// Media comment cration time
        /// </summary>
        public DateTime? CommentCreated { get; set; }
    }
}
