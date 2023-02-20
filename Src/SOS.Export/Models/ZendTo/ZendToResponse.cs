using System.Collections.Generic;

namespace SOS.Export.Models.ZendTo
{
    public class ZendToFile
    {
        /// <summary>
        /// File check sum
        /// </summary>
        public string Checksum { get; set; }

        /// <summary>
        /// File name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// True if ile can't be downloaded
        /// </summary>
        public bool Nodownloads { get; set; }

        /// <summary>
        /// File size
        /// </summary>
        public int Size { get; set; }
    }

    public class ZendToRecipient
    {
        /// <summary>
        /// Name of recipient
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Email to recipient
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Recipient download link
        /// </summary>
        public string Link { get; set; }
    }

    public class ZendToResponse
    {
        /// <summary>
        /// File info
        /// </summary>
        public IEnumerable<ZendToFile> Files { get; set; }

        /// <summary>
        /// Days file remains in ZendTo
        /// </summary>
        public double LifetimeDays { get; set; }

        /// <summary>
        /// True if file only can be fetched one time
        /// </summary>
        public bool OneTime { get; set; }

        /// <summary>
        /// Recipients 
        /// </summary>
        public IEnumerable<ZendToRecipient> Recipients { get; set; }

        /// <summary>
        /// Response text
        /// </summary>
        public string Response { get; set; }

        /// <summary>
        /// Send status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// True if request was successful
        /// </summary>
        public bool Success => Status?.Equals("OK", System.StringComparison.CurrentCultureIgnoreCase) ?? false;
    }
}