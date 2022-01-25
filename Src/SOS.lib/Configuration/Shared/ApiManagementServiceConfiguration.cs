namespace SOS.Lib.Configuration.Shared
{
    /// <summary>
    /// Api management service configuration
    /// </summary>
    public class ApiManagementServiceConfiguration
    {
        /// <summary>
        /// Service base address 
        /// </summary>
        public string BaseAddress { get; set; }

        /// <summary>
        /// Resource group
        /// </summary>
        public string ResourceGroup { get; set; }

        /// <summary>
        /// Service
        /// </summary>
        public string Service { get; set; }

        /// <summary>
        /// Key used to sign
        /// </summary>
        public string SigningKey { get; set; }

        /// <summary>
        /// Subscription id
        /// </summary>
        public string SubscriptionId { get; set; }
    }
}