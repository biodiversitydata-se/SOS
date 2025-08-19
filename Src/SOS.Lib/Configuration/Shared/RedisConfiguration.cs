namespace SOS.Lib.Configuration.Shared
{
    /// <summary>
    ///     Configuration parameters for a Redis
    /// </summary>
    public class RedisConfiguration 
    {
        /// <summary>
        /// Address for the service
        /// </summary>
        public string EndPoint { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// ServiceName
        /// </summary>
        public string ServiceName { get; set; }
    }
}