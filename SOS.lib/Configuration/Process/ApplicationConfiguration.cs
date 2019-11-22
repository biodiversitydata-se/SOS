using SOS.Lib.Extensions;

namespace SOS.Lib.Configuration.Process
{
    /// <summary>
    /// Web server configuration
    /// </summary>
    public class ApplicationConfiguration
    {
        /// <summary>
        /// Name of property holding database name
        /// </summary>
        public string DatabaseNameProperty { get; set; }

        /// <summary>
        /// Application installation folder 
        /// </summary>
        public string Folder { get; set; }

        /// <summary>
        /// Indicates if it's a web application or not
        /// </summary>
        public bool IsWebApplication { get; set; }

        /// <summary>
        /// Name of process or application pool
        /// </summary>
        public string ProcessName { get; set; }

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
        public string UniqueSettingsFile => $"{ServerName.FirstAlfanumeric()}.{Folder.LastAlfanumeric()}.{SettingsFile}";
    }
}
