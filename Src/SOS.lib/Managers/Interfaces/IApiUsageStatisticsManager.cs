using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.Validation;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Managers.Interfaces
{
    public interface IApiUsageStatisticsManager
    {
        Task<bool> HarvestStatisticsAsync();
    }
}
