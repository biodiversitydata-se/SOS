using System;
using System.Collections.Generic;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Lib.Models.Processed.ProcessInfo
{
    /// <summary>
    /// Information about verbatim
    /// </summary>
    public class VerbatimInfo : HarvestInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="provider"></param>
        /// <param name="start"></param>
        public VerbatimInfo(string id, DataProvider provider, DateTime start): base(id, provider, start)
        {
            
        }

        /// <summary>
        /// Information about metadata
        /// </summary>
        public IEnumerable<HarvestInfo> Metadata { get; set; }


    }
}
