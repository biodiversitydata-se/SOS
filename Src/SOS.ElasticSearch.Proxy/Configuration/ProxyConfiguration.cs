﻿namespace SOS.ElasticSearch.Proxy.Configuration
{
    public class ProxyConfiguration
    {
        /// <summary>
        /// Average size of observation, used to calculate number of observations returned
        /// </summary>
        public int AverageObservationSize { get; set;}

        /// <summary>
        /// Log the request.
        /// </summary>
        public bool LogRequest { get; set; }

        /// <summary>
        /// Log the response.
        /// </summary>
        public bool LogResponse { get; set; }

        /// <summary>
        /// Max number of characters in log response.
        /// </summary>
        public int LogResponseMaxCharacters { get; set; } = 4096;

        /// <summary>
        /// Log performance.
        /// </summary>
        public bool LogPerformance { get; set; }

        /// <summary>
        /// If True, exclude fields should be added to the JSON query sent to Elasticsearch.
        /// </summary>
        public bool ExcludeFieldsInElasticsearchQuery { get; set; }
    }
}