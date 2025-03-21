using Elastic.Clients.Elasticsearch;
using SOS.Lib.Managers.Interfaces;

namespace SOS.Observations.Api.IntegrationTests.Setup.Stubs;
internal class ElasticClientTestManager : IElasticClientManager
{
    private ElasticsearchClient[] _clients;
    public ElasticClientTestManager(ElasticsearchClient elasticClient)
    {
        _clients = new ElasticsearchClient[] { elasticClient };
    }

    public ElasticsearchClient[] Clients => _clients;
}