using System.Collections.Generic;

namespace SOS.Lib.Models.FileSender
{
    public class TransferRequest
    {
        /// <summary>
        /// Expiry date as a UNIX timestamp
        /// </summary>
        public int Expires { get; set; }

        /// <summary>
        /// Files 
        /// </summary>
        public IEnumerable<FileRequest> Files { get; set; }

        /// <summary>
        /// Choosen sender email address
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Message sent to recipients (may be empty)
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// I.E. email_download_complete, email_report_on_closing
        /// </summary>
        public IEnumerable<string> Options { get; set; }

        /// <summary>
        /// Email recipients
        /// </summary>
        public IEnumerable<string> Recipients { get; set; }

        /// <summary>
        /// Subject sent to recipients (may be empty)
        /// </summary>
        public string Subject { get; set; }
    }
}
