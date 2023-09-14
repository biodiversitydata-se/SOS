using SOS.Harvest.Harvesters.Interfaces;
using SOS.Lib.Enums;

namespace SOS.Harvest.Managers.Interfaces
{
    public interface IObservationHarvesterManager
    {
        /// <summary>
        /// Get harvester for data provider
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <returns></returns>
        IObservationHarvester GetHarvester(DataProviderType dataProvider);
    }
}
