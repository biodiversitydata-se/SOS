using System;
using System.Collections.Generic;
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
        /// Elastic clusters
        /// </summary>
        public IEnumerable<Cluster> Clusters { get; set; }

        /// <summary>
        /// Enable debug mode if true
        /// </summary>
        public bool DebugMode { get; set; }

        /// <summary>
        /// Index settings
        /// </summary>
        public IEnumerable<ElasticSearchIndexConfiguration> IndexSettings { get; set; }

        /// <summary>
        ///     Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Request timeout in seconds
        /// </summary>
        public int? RequestTimeout { get; set; }

        /// <summary>
        ///     User name
        /// </summary>
        public string UserName { get; set; }

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
                else if (_indexPrefix == "sos-integrationtests")
                {
                    return $"sos-integrationtests-{Environment.MachineName}";
                }

                return _indexPrefix;
            }
            set => _indexPrefix = value;
        }

       
        /// <summary>
        /// Get client created with current configuration
        /// </summary>
        /// <returns></returns>
        public IElasticClient[] GetClients()
        {
            var clients = new List<IElasticClient>();
            foreach (var cluster in Clusters)
            {
                var uris = cluster.Hosts.Select(u => new Uri(u));

                var connectionPool = new StaticConnectionPool(uris);
                var settings = new ConnectionSettings(connectionPool)
                    .EnableApiVersioningHeader()
                    .EnableHttpCompression(true)
                    .RequestTimeout(TimeSpan.FromSeconds(RequestTimeout ?? 60))
                    .SniffOnStartup(true)
                    .SniffOnConnectionFault(true)
                    .SniffLifeSpan(new TimeSpan(0, 30, 0))
                    .ServerCertificateValidationCallback(CertificateValidations.AllowAll);
             
                if (!string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password))
                {
                    settings.BasicAuthentication(UserName, Password);
                }

                if (DebugMode)
                {
                    settings.EnableDebugMode();
                }
                clients.Add(new ElasticClient(settings));
            }

            return clients.ToArray();
        }

        /// <summary>
        /// Cluster class
        /// </summary>
        public class Cluster
        {
            /// <summary>
            ///     Host
            /// </summary>
            public string[] Hosts { get; set; }
        }
    }
}