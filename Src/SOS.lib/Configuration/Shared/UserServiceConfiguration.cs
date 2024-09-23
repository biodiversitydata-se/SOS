namespace SOS.Lib.Configuration.Shared
{
    public class UserServiceConfiguration : RestServiceConfiguration
    {
        /// <summary>
        /// Address for the User Admin 2 - User API service.
        /// </summary>
        public string UserAdmin2ApiBaseAddress { get; set; }

        /// <summary>
        /// Decides whether User Admin 2 - User API should be used.
        /// </summary>
        public bool UseUserAdmin2Api { get; set; }

        /// <summary>
        /// SOS client id.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// SOS client secret.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Token URL.
        /// </summary>
        public string TokenUrl { get; set; }        
    }
}