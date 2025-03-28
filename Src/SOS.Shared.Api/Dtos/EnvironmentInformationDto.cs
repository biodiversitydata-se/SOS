﻿namespace SOS.Shared.Api.Dtos
{
    /// <summary>
    ///     Environment information
    /// </summary>
    public class EnvironmentInformationDto
    {
        /// <summary>
        ///     Type of environment (Local, Dev, ST, AT, Prod)
        /// </summary>
        public string EnvironmentType { get; set; }

        /// <summary>
        ///     Name of the server hosting the application/service
        /// </summary>
        public string HostingServerName { get; set; }

        /// <summary>
        ///     Server operating system.
        /// </summary>
        public string OsPlatform { get; set; }

        /// <summary>
        ///     .Net version.
        /// </summary>
        public string AspDotnetVersion { get; set; }

        /// <summary>
        ///     Current Culture.
        /// </summary>
        public string CurrentCulture { get; set; }
    }
}