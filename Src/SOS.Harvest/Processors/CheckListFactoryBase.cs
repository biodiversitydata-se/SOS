using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Processors
{
    /// <summary>
    /// Base class for observation factories
    /// </summary>
    public class CheckListFactoryBase : FactoryBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected CheckListFactoryBase(DataProvider dataProvider, IProcessTimeManager processTimeManager) : base(dataProvider, processTimeManager)
        {
           
        }
    }
}