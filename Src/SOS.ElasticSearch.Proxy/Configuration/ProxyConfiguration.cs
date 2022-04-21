namespace SOS.ElasticSearch.Proxy.Configuration
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

        /// <summary>
        /// Fields to exclude in Elasticsearch query.
        /// </summary>
        public List<string>? ExcludeFields { get; set; }

        /// <summary>
        /// The exclude query.
        /// </summary>
        public string? ExcludeFieldsQuery
        {
            get
            {
                if (_excludeFieldsQuery == null)
                {
                    if (ExcludeFields == null || ExcludeFields.Count == 0)
                    {
                        _excludeFieldsQuery = "";
                    }
                    else
                    {
                        string fields = string.Join(", ", ExcludeFields.Select(m => $"\"{m}\""));
                        _excludeFieldsQuery = $"\"_source\": {{ \"excludes\": [{fields}]}},";
                    }
                }

                return _excludeFieldsQuery;
            }
        }
        private string? _excludeFieldsQuery;
    }
}