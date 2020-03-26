using System.Linq;
using System.Security.Authentication;
using MongoDB.Driver;

namespace SOS.Lib.Configuration.Shared
{
    /// <summary>
    /// ElasticSearch configuration properties
    /// </summary>
    public class ElasticSearchConfiguration
    {
        /// <summary>
        /// Host
        /// </summary>
        public string[] Hosts { get; set; }
    }
}