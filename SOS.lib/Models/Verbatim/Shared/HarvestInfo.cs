using System;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Verbatim.Shared
{
    public class HarvestInfo : IEntity<string>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="provider"></param>
        public HarvestInfo(string id, DataProvider provider)
        {
            Id = id;
            DataProvider = provider;
        }

        /// <summary>
        /// Id of data provider
        /// </summary>
        public DataProvider DataProvider { get; set; }

        /// <summary>
        /// Harvest end date and time
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Id of data set
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Harvest start date and time
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Number of items
        /// </summary>
        public int Count { get; set; }
    }
}
