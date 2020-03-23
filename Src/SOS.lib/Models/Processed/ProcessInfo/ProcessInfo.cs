using System;
using System.Collections.Generic;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Processed.ProcessInfo
{
    /// <summary>
    /// Process configuration
    /// </summary>
    public class ProcessInfo : IEntity<string>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="start"></param>
        public ProcessInfo(string id, DateTime start) 
        {
            Id = id;
            Start = start;
        }

        /// <summary>
        /// Item processed
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Harvest end date and time
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Id, equals updated instance (0 or 1)
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Information about providers
        /// </summary>
        public IEnumerable<ProviderInfo> ProvidersInfo { get; set; }

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
