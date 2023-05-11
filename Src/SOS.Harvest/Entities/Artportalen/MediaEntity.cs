namespace SOS.Harvest.Entities.Artportalen
{
    public class MediaEntity 
    {
        /// <summary>
        /// Media comment
        /// </summary>
        public string? Comment { get; set; }

        /// <summary>
        /// User making the comment
        /// </summary>
        public string? CommentBy { get; set; }

        /// <summary>
        /// Media comment cration time
        /// </summary>
        public DateTime? CommentCreated { get; set; }

        /// <summary>
        ///    Copyright text
        /// </summary>
        public string? CopyrightText { get; set; }

        /// <summary>
        ///     Id of media.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Type of file
        /// </summary>
        public string? FileType { get; set; }

        /// <summary>
        /// Uri to file
        /// </summary>
        public string? FileUri { get; set; }

        /// <summary>
        /// Rights holder
        /// </summary>
        public string? RightsHolder { get; set; }

        /// <summary>
        /// Id of sighting
        /// </summary>
        public int SightingId { get; set; }

        /// <summary>
        /// Media upload time
        /// </summary>
        public DateTime? UploadDateTime { get; set; }
    }
}