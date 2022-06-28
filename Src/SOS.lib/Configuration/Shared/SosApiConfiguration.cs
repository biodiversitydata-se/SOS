using System.Collections.Generic;

namespace SOS.Lib.Configuration.Shared
{
    public class SosApiConfiguration
    {
        /// <summary>
        /// Observations API address.
        /// </summary>
        public IEnumerable<string> ObservationsApiAddresses { get; set; }

        public IEnumerable<string> ElasticSearchProxyAddresses { get; set; }
    }
}
