using System;
using System.Linq;
using Elasticsearch.Net;
using Nest;
using SOS.Lib.Configuration.Shared;

namespace SOS.Lib.Extensions
{
    public static class ElasticsearchExtensions
    {
        public static ElasticClient ToClient(this ElasticSearchConfiguration configuration)
        {
            /*
                  var uris = new Uri[]
              {
                new Uri("<a class="c-link" href="https://54.236.165.201:9201/" target="_blank" rel="noopener noreferrer">https://xxx.xxx.xxx.xxx:9201</a>"),
                new Uri("<a class="c-link" href="https://34.232.31.152:9201/" target="_blank" rel="noopener noreferrer">https://xxx.xxx.xxx.xxx:9201</a>"),
                new Uri("<a class="c-link" href="https://3.233.56.221:9201/" target="_blank" rel="noopener noreferrer">https://xxx.xxx.xxx.xxx:9201</a>")

             var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            X509Certificate cert = null;
            foreach (var certificate in store.Certificates)
            {
                cert = certificate;
            }

            };
             */


            var uris = configuration.Hosts.Select(u => new Uri(u));
            
            var connectionPool = new StaticConnectionPool(uris);
            var settings = new ConnectionSettings(connectionPool);

            if (!string.IsNullOrEmpty(configuration.UserName) && !string.IsNullOrEmpty(configuration.Password))
            {
                settings.BasicAuthentication(configuration.UserName, configuration.Password);
            }
               
              //  .ServerCertificateValidationCallback(CertificateValidations.AuthorityIsRoot(cert));
            //.DisableDirectStreaming().EnableDebugMode().PrettyJson() // Uncomment this line when debugging ES-query. Req and Resp is in result.DebugInformation in ProcessedObservationRepository.cs.

            var client = new ElasticClient(settings);
            return client;
        }
    }
}
