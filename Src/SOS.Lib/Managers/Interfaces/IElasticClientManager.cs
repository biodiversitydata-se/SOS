using Elastic.Clients.Elasticsearch;

namespace SOS.Lib.Managers.Interfaces
{
    /// <summary>
    /// Manager of elastic clients
    /// </summary>
    public interface IElasticClientManager
    {
        /// <summary>
        /// Elastic search clients
        /// </summary>
        ElasticsearchClient[] Clients { get; }
    }
}
