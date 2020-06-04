namespace SOS.Lib.Configuration.Shared
{
    /// <summary>
    /// Configuration parameters for Identity Server.
    /// </summary>
    public class IdentityServerConfiguration 
    {
        /// <summary>
        /// Authority
        /// </summary>
        public string Authority { get; set; }

        /// <summary>
        /// Api name
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Required https meta data
        /// </summary>
        public bool RequireHttpsMetadata { get; set; }
    }
}
