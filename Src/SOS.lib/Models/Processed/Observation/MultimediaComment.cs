using System;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    ///     Multimedia comment
    /// </summary>
    public class MultimediaComment
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
        public DateTime? Created { get; set; }
        
    }
}