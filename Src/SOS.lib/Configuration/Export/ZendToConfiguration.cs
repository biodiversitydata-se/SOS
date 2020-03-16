namespace SOS.Lib.Configuration.Export
{
    public class ZendToConfiguration
    {
        /// <summary>
        /// Email subject
        /// </summary>
        public string EmailSubject { get; set; }

        /// <summary>
        /// Email message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Account password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// "SenderName" "SenderEmail" has dropped off a file for you
        /// </summary>
        public string SenderEmail { get; set; }

        /// <summary>
        /// "SenderName" "SenderEmail" has dropped off a file for you
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// Sender organization 
        /// </summary>
        public string SenderOrganization { get; set; }

        /// <summary>
        /// Account user name
        /// </summary>
        public string UserName { get; set; }
    }
}
