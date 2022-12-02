using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Processors
{
    /// <summary>
    /// Base class for dataset factories
    /// </summary>
    public class DatasetFactoryBase : FactoryBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected DatasetFactoryBase(DataProvider dataProvider, IProcessTimeManager processTimeManager, ProcessConfiguration processConfiguration) : base(dataProvider, processTimeManager, processConfiguration)
        {
           
        }
    }
}