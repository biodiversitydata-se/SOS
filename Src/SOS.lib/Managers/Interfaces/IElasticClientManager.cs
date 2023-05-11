using Nest;

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
        IElasticClient[] Clients { get; }
    }
}
