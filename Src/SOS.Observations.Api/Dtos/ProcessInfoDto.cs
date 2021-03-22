using System;
using System.Collections.Generic;

namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    /// Dto
    /// </summary>
    public class ProcessInfoDto
    {
        /// <summary>
        ///     Item processed
        /// </summary>
        public int PublicCount { get; set; }

        /// <summary>
        /// Protected observations count
        /// </summary>
        public int ProtectedCount { get; set; }

        /// <summary>
        ///     Harvest end date and time
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        ///     Provider information about meta data
        /// </summary>
        public IEnumerable<ProcessInfoDto> MetadataInfo { get; set; }

        /// <summary>
        ///     Information about providers
        /// </summary>
        public IEnumerable<ProviderInfoDto> ProvidersInfo { get; set; }

        /// <summary>
        ///     Harvest start date and time
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        ///     Running status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        ///     Id, equals updated instance (0 or 1)
        /// </summary>
        public string Id { get; set; }
    }
}
