namespace SOS.DataStewardship.Api.IntegrationTests.Setup;

public class ElasticClientTestManager : IElasticClientManager
{
    private IElasticClient[] _clients;
    public ElasticClientTestManager(IElasticClient elasticClient)
    {
        _clients = new IElasticClient[] { elasticClient };
    }

    public IElasticClient[] Clients => _clients;
}