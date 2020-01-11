using System;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Models.Verbatim.Shared
{
    public class HarvestInfo : RunInfo, IEntity<string>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="provider"></param>
        /// <param name="start"></param>
        public HarvestInfo(string id, DataProvider provider, DateTime start) : base(provider)
        {
            Id = id;
            Start = start;
        }

        /// <summary>
        /// Id of data set
        /// </summary>
        public string Id { get; set; }
    }
}
