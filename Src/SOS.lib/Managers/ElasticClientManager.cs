using SOS.Lib.Configuration.Shared;
using SOS.Lib.Managers.Interfaces;
using IElasticClient = Nest.IElasticClient;

namespace SOS.Lib.Managers
{
    /// <inheritdoc />
    public class ElasticClientManager : IElasticClientManager
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elasticSearchConfiguration"></param>
        /// <param name="debugMode"></param>
        public ElasticClientManager(ElasticSearchConfiguration elasticSearchConfiguration, bool debugMode = false)
        {
            Clients = elasticSearchConfiguration.GetClients(debugMode);
        }

        /// <inheritdoc />
        public IElasticClient[] Clients { get; }
    }
}
