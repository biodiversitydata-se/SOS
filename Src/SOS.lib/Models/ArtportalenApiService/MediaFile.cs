using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Lib.Models.ArtportalenApiService
{
    public class MediaFile
    {
        /// <summary>
        /// Gets the media file's comments
        /// </summary>
        public IList<MediaFileComment> Comments { get; set; }

        /// <summary>
        /// Gets the media file's created date and time
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets the description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets the file name
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets the url where the normal, size adjusted file can be downloaded
        /// </summary>
        public string FileUrl { get; set; }

        /// <summary>
        /// Gets the height (in pixels).
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Gets the id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets the media file type name
        /// </summary>
        public string MediaFileTypeName { get; set; }

        /// <summary>
        /// Gets the url where the original, unmodified file can be downloaded.
        /// </summary>
        public string OriginalFileUrl { get; set; }

        /// <summary>
        /// Gets the width (in pixels) of the original file.
        /// </summary>
        public int OriginalWidth { get; set; }

        /// <summary>
        /// Gets the height (in pixels) of the original file.
        /// </summary>
        public int OriginalHeight { get; set; }

        /// <summary>
        /// Gets the url where a thumbnail of the file can be downloaded
        /// </summary>
        public string ThumbnailFileUrl { get; set; }

        /// <summary>
        /// Gets the user's name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets the number of views the media file has had
        /// </summary>
        public int Views { get; set; }

        /// <summary>
        /// Gets the width (in pixels)
        /// </summary>
        public int Width { get; set; }
    }

    public class MediaFileComment
    {
        /// <summary>
        /// Gets the comment's created date and time
        /// </summary>
        public virtual DateTime Created { get; set; }

        /// <summary>
        /// Gets the comment
        /// </summary>
        public virtual string Comment { get; set; }

        /// <summary>
        /// Gets the id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets the user's name
        /// </summary>
        public virtual string UserName { get; set; }
    }
}
