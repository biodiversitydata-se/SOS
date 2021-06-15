using System;
using System.Linq;
using Elasticsearch.Net;
using Nest;

namespace SOS.Lib.Configuration.Shared
{
    /// <summary>
    ///     ElasticSearch configuration properties
    /// </summary>
    public class ElasticSearchConfiguration
    {
        private string _indexPrefix;

        /// <summary>
        /// Enable debug mode if true
        /// </summary>
        public bool DebugMode { get; set; }

        /// <summary>
        ///     How many items to read in a time when scrolling
        /// </summary>
        public int ReadBatchSize { get; set; }

        /// <summary>
        ///     How many items to write in a time
        /// </summary>
        public int WriteBatchSize { get; set; }

        /// <summary>
        ///     Host
        /// </summary>
        public string[] Hosts { get; set; }

        /// <summary>
        /// Max number of aggregation buckets.
        /// </summary>
        public int MaxNrAggregationBuckets { get; set; } = 65535;

        /// <summary>
        /// Number of shards
        /// </summary>
        public int NumberOfShards { get; set; } = 6;

        /// <summary>
        /// Number of replicas
        /// </summary>
        public int NumberOfReplicas { get; set; }

        /// <summary>
        ///     Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        ///     User name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Scope required for access to protected observations
        /// </summary>
        public string ProtectedScope { get; set; }

        /// <summary>
        ///     dev, st or at. prod is empty
        /// </summary>
        public string IndexPrefix
        {
            get
            {
                // If prefix is "sos-local", add the machine name to the index
                // to let developers have their own environment when running locally.
                if (_indexPrefix == "sos-local")
                {
                    return $"sos-local-{Environment.MachineName}";
                }

                return _indexPrefix;
            }
            set => _indexPrefix = value;
        }

       
        /// <summary>
        /// Get client created with cuurent configuration
        /// </summary>
        /// <returns></returns>
        public ElasticClient GetClient()
        {
            var uris = Hosts.Select(u => new Uri(u));

            var connectionPool = new StaticConnectionPool(uris);
            var settings = new ConnectionSettings(connectionPool);

            if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password))
            {
                settings.BasicAuthentication(UserName, Password);
                settings.SniffOnStartup(false);
                settings.SniffOnConnectionFault(false);
                settings.ServerCertificateValidationCallback(CertificateValidations.AllowAll);
            }

            //  .ServerCertificateValidationCallback(CertificateValidations.AuthorityIsRoot(cert));
            if (DebugMode)
            {
                settings.DisableDirectStreaming().EnableDebugMode().PrettyJson();
            }

            return new ElasticClient(settings);
        }
    }
}