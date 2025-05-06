using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Serialization;
using Elastic.Transport;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace SOS.Lib.Helpers
{
    /// <summary>
    /// Helper methods for ElasticSearch
    /// </summary>
    public static class ElasticSearchHelper
    {
        /// <summary>
        /// Get default settings
        /// </summary>
        /// <param name="uris"></param>
        /// <returns></returns>
        public static ElasticsearchClientSettings GetDefaultSettings(params Uri[] uris)
        {
            var connectionPool = new StaticNodePool(uris);
            var settings = new ElasticsearchClientSettings(connectionPool,
                    sourceSerializer: (defaultSerializer, settings) => new DefaultSourceSerializer(settings, o =>
                    {
                        // Remove JsonStringEnumConverter since we convert enum to byte
                        var stringEnumConverter = o.Converters.FirstOrDefault(c => c.GetType().Equals(typeof(JsonStringEnumConverter)));
                        if (stringEnumConverter != null)
                        {
                            o.Converters.Remove(stringEnumConverter);
                        }
                        o.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
                    }),
                    requestInvoker: new HttpRequestInvoker()
                ) 
                .ServerCertificateValidationCallback(CertificateValidations.AllowAll);

            return settings;
        }
    }
}
