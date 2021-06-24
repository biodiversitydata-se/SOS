using System;
using System.Collections.Generic;

namespace SOS.Administration.Gui.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ProtectedLogDto
    {
        /// <summary>
        /// Identifier of application making the request
        /// </summary>
        public string ApplicationIdentifier { get; set; }

        /// <summary>
        /// IP request was made from
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Date request was made
        /// </summary>
        public DateTime IssueDate { get; set; }

        /// <summary>
        /// List of observations returned
        /// </summary>
        public IEnumerable<ProtectedLogObservationDto> Observations { get; set; }

        /// <summary>
        /// Person making the request
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Person making the request
        /// </summary>
        public string UserId { get; set; }
    }
}