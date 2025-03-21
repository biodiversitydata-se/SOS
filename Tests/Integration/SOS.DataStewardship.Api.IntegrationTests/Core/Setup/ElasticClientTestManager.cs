using Elastic.Clients.Elasticsearch;

namespace SOS.DataStewardship.Api.IntegrationTests.Core.Setup;

public class ElasticClientTestManager : IElasticClientManager
{
    private ElasticsearchClient[] _clients;
    public ElasticClientTestManager(ElasticsearchClient elasticClient)
    {
        _clients = new ElasticsearchClient[] { elasticClient };
    }

    public ElasticsearchClient[] Clients => _clients;
}