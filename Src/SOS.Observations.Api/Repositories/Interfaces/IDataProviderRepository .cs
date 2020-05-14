using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Observations.Api.Repositories.Interfaces
{
    /// <summary>
    /// Data provider repository interface
    /// </summary>
    public interface IDataProviderRepository : IBaseRepository<DataProvider, int>
    {
        
    }
}