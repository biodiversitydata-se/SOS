using Elastic.Clients.Elasticsearch;
using Elastic.Transport.Products.Elasticsearch;
using System;

namespace SOS.Lib.Extensions
{
    public static class ElasticSearchResponseExtensions
    {
        private static void ThrowException(ElasticsearchServerError elasticsearchServerError, string debugInformation)
        {
            if (elasticsearchServerError != null)
            {
                if (elasticsearchServerError.Error?.Reason?.Contains("The request was canceled due to the configured HttpClient.Timeout", StringComparison.CurrentCultureIgnoreCase) ?? false)
                {
                    throw new TimeoutException(elasticsearchServerError.Error.Reason);
                }

                throw new Exception(elasticsearchServerError.Error?.Reason);
            }

            throw new InvalidOperationException(debugInformation);
        }

        public static void ThrowIfInvalid<T>(this SearchResponse<T> response) where T : class
        {
            if (!response?.IsValidResponse ?? true)
            {
                ThrowException(response.ElasticsearchServerError, response.DebugInformation);
            }
        }

        public static void ThrowIfInvalid(this CountResponse response)
        {
            if (!response.IsValidResponse)
            {
                ThrowException(response.ElasticsearchServerError, response.DebugInformation);
            }
        }

        public static void ThrowIfInvalid(this OpenPointInTimeResponse response)
        {
            if (!response?.IsValidResponse ?? true)
            {
                ThrowException(response.ElasticsearchServerError, response.DebugInformation);
            }
        }
    }

}
