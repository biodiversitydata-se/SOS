using System.Collections.Generic;

namespace SOS.Observations.Api.Configuration
{
    public class DevOpsReleases
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    /// <summary>
    /// Observations API configuration.
    /// </summary>
    public class DevOpsConfiguration
    {
       /// <summary>
       /// Base address
       /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        /// Personal access token to get access to Dev Ops API
        /// </summary>
        public string PersonalAccessToken { get; set; }

        /// <summary>
        /// Dev ops definitions
        /// </summary>
        public IEnumerable<DevOpsReleases> Releases { get; set; }
    }
}