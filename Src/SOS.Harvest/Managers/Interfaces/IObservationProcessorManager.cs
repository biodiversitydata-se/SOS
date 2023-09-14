using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Enums;

namespace SOS.Harvest.Managers.Interfaces
{
    public interface IObservationProcessorManager
    {
        /// <summary>
        /// Get processor for data provider
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <returns></returns>
        IObservationProcessor GetProcessor(DataProviderType dataProvider);
    }
}
