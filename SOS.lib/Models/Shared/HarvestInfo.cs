using System;
using SOS.Lib.Enums;

namespace SOS.Lib.Models.Shared.Shared
{
    public class RunInfo 
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="provider"></param>
        public RunInfo(DataProvider provider)
        {
            DataProvider = provider;
        }

        /// <summary>
        /// Number of items
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Id of data provider
        /// </summary>
        public DataProvider DataProvider { get; }

        /// <summary>
        /// Harvest end date and time
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Harvest start date and time
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Running status
        /// </summary>
        public RunStatus Status { get; set; }
    }
}
