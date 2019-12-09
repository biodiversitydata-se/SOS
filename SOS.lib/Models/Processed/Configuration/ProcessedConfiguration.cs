﻿using System;
using System.Collections.Generic;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Lib.Models.Processed.Configuration
{
    /// <summary>
    /// Process configuration
    /// </summary>
    public class ProcessedConfiguration : IEntity<byte>
    {
        /// <summary>
        /// Process end date and time
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Active instance 0 or 1
        /// </summary>
        public byte ActiveInstance { get; set; }

        /// <summary>
        /// Information about harvest
        /// </summary>
        public IEnumerable<HarvestInfo> HarvestInfo { get; set; }

        /// <summary>
        /// Id of configuration (always 0)
        /// </summary>
        public byte Id { get; set; }

        /// <summary>
        /// Process start date and time
        /// </summary>
        public DateTime Start { get; set; }
    }
}
