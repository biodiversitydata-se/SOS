using SOS.Lib.Configuration.Shared;
using SOS.Lib.Managers.Interfaces;
using IElasticClient = Nest.IElasticClient;

namespace SOS.Lib.Managers
{
    /// <summary>
    /// Elastic client manager
    /// </summary>
    public class ElasticClientManager : IElasticClientManager
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="elasticSearchConfiguration"></param>        
        public ElasticClientManager(ElasticSearchConfiguration elasticSearchConfiguration)
        {
            Clients = elasticSearchConfiguration.GetClients();
        }

        /// <inheritdoc />
        public IElasticClient[] Clients { get; }
    }
}
