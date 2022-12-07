using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Processors
{
    /// <summary>
    /// Base class for event factories
    /// </summary>
    public class EventFactoryBase : FactoryBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected EventFactoryBase(DataProvider dataProvider, IProcessTimeManager processTimeManager, ProcessConfiguration processConfiguration) : base(dataProvider, processTimeManager, processConfiguration)
        {

        }
    }
}