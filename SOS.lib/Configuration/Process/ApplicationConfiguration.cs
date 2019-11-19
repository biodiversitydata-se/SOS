using SOS.Lib.Extensions;

namespace SOS.Lib.Configuration.Process
{
    /// <summary>
    /// Web server configuration
    /// </summary>
    public class ApplicationConfiguration
    {
        /// <summary>
        /// Application pool
        /// </summary>
        public string ApplicationPool { get; set; }

        /// <summary>
        /// Application installation folder 
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// Name of server
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Name of app settings file
        /// </summary>
        public string SettingsFile { get; set; }

        /// <summary>
        /// Make a unique settings file name
        /// </summary>
        public string UniqueSettingsFile => $"{ServerName.ToAlfanumeric()}.{Folder.ToAlfanumeric()}.{SettingsFile}";
    }
}
