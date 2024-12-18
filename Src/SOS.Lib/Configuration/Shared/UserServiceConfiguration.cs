﻿namespace SOS.Lib.Configuration.Shared
{
    public class UserServiceConfiguration : RestServiceConfiguration
    {
        /// <summary>
        /// Address for the User Admin 2 - User API service.
        /// </summary>
        public string UserAdmin2ApiBaseAddress { get; set; }

        /// <summary>
        /// SOS client id.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// SOS client secret.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Scope.
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Token URL.
        /// </summary>
        public string TokenUrl { get; set; }

        /// <summary>
        /// Token expiration buffer in seconds.
        /// </summary>
        public int TokenExpirationBufferInSeconds { get; set; }
        
        /// <summary>
        /// Identity provider.
        /// </summary>
        public IdentityServerConfiguration IdentityProvider { get; set; }
    }
}