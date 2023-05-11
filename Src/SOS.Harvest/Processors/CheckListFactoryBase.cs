using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Processors
{
    /// <summary>
    /// Base class for observation factories
    /// </summary>
    public class ChecklistFactoryBase : FactoryBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected ChecklistFactoryBase(DataProvider dataProvider, IProcessTimeManager processTimeManager, ProcessConfiguration processConfiguration) : base(dataProvider, processTimeManager, processConfiguration)
        {
           
        }
    }
}