using System.Linq;
using System.Security.Authentication;
using MongoDB.Driver;

namespace SOS.Lib.Configuration.Shared
{
    /// <summary>
    ///     Cosmos configuration properties
    /// </summary>
    public class MongoDbConfiguration
    {
        /// <summary>
        /// Database where the user account exists
        /// </summary>
        public string AuthenticationDb { get; set; }

        /// <summary>
        ///     Host
        /// </summary>
        public MongoDbServer[] Hosts { get; set; }

        /// <summary>
        ///     Name of replica set
        /// </summary>
        public string ReplicaSetName { get; set; }

        /// <summary>
        ///     User name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        ///     Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     Name of data base
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        ///     True if ssl is used
        /// </summary>
        public bool UseTls { get; set; }

        /// <summary>
        ///     How many items to read in a time
        /// </summary>
        public int ReadBatchSize { get; set; }

        /// <summary>
        ///     How many items to write in a time
        /// </summary>
        public int WriteBatchSize { get; set; }

        /// <summary>
        ///     Get mongo db settings object.
        /// </summary>
        /// <returns></returns>
        public MongoClientSettings GetMongoDbSettings()
        {
            // Create settings object
            var mongoSettings = new MongoClientSettings
            {
                ReadPreference = ReadPreference.PrimaryPreferred,
                UseTls = UseTls,
                SslSettings = UseTls ? new SslSettings
                    {
                        EnabledSslProtocols = SslProtocols.Tls12
                    }
                    : null
            };

            // Add authentication if requested
            if (!(string.IsNullOrEmpty(AuthenticationDb) ||
                  string.IsNullOrEmpty(UserName) ||
                  string.IsNullOrEmpty(Password)))
            {
                mongoSettings.Credential = MongoCredential.CreateCredential(AuthenticationDb, UserName, Password);
            }

            // Set server/s and connection mode based on number of hosts 
            if (Hosts.Length == 1)
            {
                mongoSettings.Server =
                    Hosts.Select(h => new MongoServerAddress(h.Name, h.Port)).FirstOrDefault();
                mongoSettings.ConnectionMode = ConnectionMode.Standalone;
            }
            else
            {
                mongoSettings.Servers = Hosts.Select(h => new MongoServerAddress(h.Name, h.Port)).ToArray();
                mongoSettings.ConnectionMode = ConnectionMode.ReplicaSet;
                mongoSettings.ReplicaSetName = ReplicaSetName;
            }
            return mongoSettings;
        }
    }
}