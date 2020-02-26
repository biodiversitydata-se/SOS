namespace SOS.Lib.Configuration.Shared
{
    /// <summary>
    /// Email server configuration
    /// </summary>
    public class EmailConfiguration
    {
        /// <summary>
        /// Email sender
        /// </summary>
        public string EmailFrom { get; set; }
        
        /// <summary>
        /// Password
        /// </summary>
        public string SmtpPassword { get; set; }
        
        /// <summary>
        /// Smtp port
        /// </summary>
        public int SmtpPort { get; set; }
        
        /// <summary>
        /// Smtp server
        /// </summary>
        public string SmtpServer { get; set; }
        
        /// <summary>
        /// Smtp user name
        /// </summary>
        public string SmtpUsername { get; set; }

        /// <summary>
        /// True if ssl should be used
        /// </summary>
        public bool UseSsl { get; set; }
    }
}
