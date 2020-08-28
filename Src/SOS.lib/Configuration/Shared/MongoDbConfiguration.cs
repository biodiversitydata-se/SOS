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
            MongoInternalIdentity identity = null;
            PasswordEvidence evidence = null;
            if (!(string.IsNullOrEmpty(DatabaseName) ||
                  string.IsNullOrEmpty(UserName) ||
                  string.IsNullOrEmpty(Password)))
            {
                identity = new MongoInternalIdentity(DatabaseName, UserName);
                evidence = new PasswordEvidence(Password);
            }

            var mongoSettings = new MongoClientSettings
            {
                UseTls = UseTls,
                SslSettings = UseTls
                    ? new SslSettings
                    {
                        EnabledSslProtocols = SslProtocols.Tls12
                    }
                    : null,
                Credential = identity != null && evidence != null
                    ? new MongoCredential("SCRAM-SHA-1", identity, evidence)
                    : null
            };

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