namespace SOS.Lib.Configuration.Shared
{
    /// <summary>
    ///     Configuration for distributed cache
    /// </summary>
    public class DistributedCacheConfiguration 
    {
        /// <summary>
        /// Channel prefix
        /// </summary>
        public string ChannelPrefix { get; set; }

        /// <summary>
        /// Connection string
        /// </summary>
        public string ConnectionString => $"{Server}:{Port},User={UserName},Password={Password},ChannelPrefix={ChannelPrefix}";

        /// <summary>
        ///     Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Server port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        ///     Address for the service
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        ///     User name
        /// </summary>
        public string UserName { get; set; }
    }
}