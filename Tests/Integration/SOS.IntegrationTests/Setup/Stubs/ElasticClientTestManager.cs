using Nest;
using SOS.Lib.Managers.Interfaces;

namespace SOS.IntegrationTests.Setup.Stubs;
internal class ElasticClientTestManager : IElasticClientManager
{
    private IElasticClient[] _clients;
    public ElasticClientTestManager(IElasticClient elasticClient)
    {
        _clients = new IElasticClient[] { elasticClient };
    }

    public IElasticClient[] Clients => _clients;
}