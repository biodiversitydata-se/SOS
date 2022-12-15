namespace SOS.DataStewardship.Api.IntegrationTests.Setup;

public class ElasticClientTestManager : IElasticClientManager
{
    private IElasticClient[] _clients;
    public ElasticClientTestManager(ElasticClient elasticClient)
    {
        _clients= new ElasticClient[] { elasticClient };
    }

    public IElasticClient[] Clients => _clients;
}