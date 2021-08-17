using System;

namespace SOS.Administration.Api.Models.Ipt
{
    public class IptResource
    {
        /// <summary>
        /// Name of resource
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Resource last modified
        /// </summary>
        public DateTime LastModified { get; set; }

        /// <summary>
        /// Name of organization
        /// </summary>
        public string Organization { get; set; }

        /// <summary>
        /// Resource last publication 
        /// </summary>
        public DateTime LastPublication { get; set; }

        /// <summary>
        /// Next publication of resource 
        /// </summary>
        public DateTime? NextPublication { get; set; }

        /// <summary>
        /// Number of records in resource
        /// </summary>
        public int Records { get; set; }

        /// <summary>
        /// SubType of resource
        /// </summary>
        public string SubType { get; set; }

        /// <summary>
        /// Type of resource
        /// </summary>
        public string Type { get; set; }
    }
}
