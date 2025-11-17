using Elastic.Clients.Elasticsearch;
using Elastic.Transport.Products.Elasticsearch;
using System;

namespace SOS.Lib.Extensions;

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

    extension<T>(SearchResponse<T> response) where T : class
    {
        public void ThrowIfInvalid()
        {
            if (!response?.IsValidResponse ?? true)
            {
                ThrowException(response.ElasticsearchServerError, response.DebugInformation);
            }
        }
    }

    extension(CountResponse response)
    {
        public void ThrowIfInvalid()
        {
            if (!response.IsValidResponse)
            {
                ThrowException(response.ElasticsearchServerError, response.DebugInformation);
            }
        }
    }

    extension(OpenPointInTimeResponse response)
    {
        public void ThrowIfInvalid()
        {
            if (!response?.IsValidResponse ?? true)
            {
                ThrowException(response.ElasticsearchServerError, response.DebugInformation);
            }
        }
    }
}
