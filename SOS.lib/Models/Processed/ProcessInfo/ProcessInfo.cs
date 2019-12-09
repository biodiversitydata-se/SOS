using System;
using System.Collections.Generic;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Processed.ProcessInfo
{
    /// <summary>
    /// Process configuration
    /// </summary>
    public class ProcessInfo : IEntity<byte>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        public ProcessInfo(byte id)
        {
            Id = id;
            VerbatimInfo = new List<VerbatimInfo>();
        }

        /// <summary>
        /// Process end date and time
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Information about harvest
        /// </summary>
        public IEnumerable<VerbatimInfo> VerbatimInfo { get; set; }

        /// <summary>
        /// Id, equals updated instance (0 or 1)
        /// </summary>
        public byte Id { get; set; }

        /// <summary>
        /// Process start date and time
        /// </summary>
        public DateTime Start { get; set; }

    }
}
