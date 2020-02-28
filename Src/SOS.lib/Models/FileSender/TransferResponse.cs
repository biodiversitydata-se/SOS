using System.Collections.Generic;

namespace SOS.Lib.Models.FileSender
{
    public class TransferResponse
    {
        /// <summary>
        /// Created time stamp
        /// </summary>
        public TimeStamp Created { get; set; }

        /// <summary>
        /// Expire time
        /// </summary>
        public TimeStamp Expires { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Expiry_date_extension { get; set; }

        /// <summary>
        /// Files 
        /// </summary>
        public IEnumerable<FileResponse> Files { get; set; }

        /// <summary>
        /// Transfer Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Choosen sender email address
        /// </summary>
        public string User_email { get; set; }

        /// <summary>
        /// Message sent to recipients (may be empty)
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Options
        /// </summary>
        public IEnumerable<string> Options { get; set; }

        /// <summary>
        /// Email recipients
        /// </summary>
        public IEnumerable<RecipientResponse> Recipients { get; set; }

        /// <summary>
        /// Subject sent to recipients (may be empty)
        /// </summary>
        public string Subject { get; set; }
    }
}
