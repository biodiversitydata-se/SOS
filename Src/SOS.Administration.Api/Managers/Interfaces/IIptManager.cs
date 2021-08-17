using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Administration.Api.Models.Ipt;

namespace SOS.Administration.Api.Managers.Interfaces
{
    /// <summary>
    ///  Ipt manager
    /// </summary>
    public interface IIptManager
    {
        /// <summary>
        /// Get all resources from IPT
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<IptResource>> GetResourcesAsync();
    }
}
