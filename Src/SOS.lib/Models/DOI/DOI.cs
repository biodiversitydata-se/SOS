using System;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Search;

namespace SOS.Lib.Models.DOI
{
    /// <summary>
    /// DOI related data
    /// </summary>
    public class DOI : IEntity<Guid>
    {
        /// <summary>
        /// Container
        /// </summary>
        public string Container { get; set; }

        /// <summary>
        /// Creation Date
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Created by
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Filter used to create DOI
        /// </summary>
        public ExportFilter Filter { get; set; }

        /// <summary>
        /// DOI id
        /// </summary>
        public Guid Id { get; set; }
    }
}
