using System.Collections.Generic;

namespace SOS.Lib.Models.Email
{
    /// <summary>
    /// 
    /// </summary>
    public class EmailMessage
    {
        /// <summary>
        /// Email message
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Email subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Email subject
        /// </summary>
        public IEnumerable<string> To { get; set; }
    }
}
