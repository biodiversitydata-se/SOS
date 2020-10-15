using System.Collections.Generic;
using SOS.Lib.Models.DataCite;

namespace SOS.Lib.Configuration.Export
{
    public class DOIConfiguration
    {
        /// <summary>
        /// DOI creator
        /// </summary>
        public DOICreator Creator { get; set; }

        /// <summary>
        /// Description of the DOI
        /// </summary>
        public IEnumerable<DOIDescription> Descriptions { get; set; }

        /// <summary>
        /// DOI Formats
        /// </summary>
        public IEnumerable<string> Formats { get; set; }

        /// <summary>
        /// Publisher
        /// </summary>
        public string Publisher { get; set; }

        /// <summary>
        /// DOI subjects
        /// </summary>
        public IEnumerable<DOISubject> Subjects { get; set; }

        /// <summary>
        /// Types of DOI
        /// </summary>
        public DOITypes Types { get; set; }

        /// <summary>
        /// URL to landing page
        /// </summary>
        public string Url { get; set; }
    }
}
