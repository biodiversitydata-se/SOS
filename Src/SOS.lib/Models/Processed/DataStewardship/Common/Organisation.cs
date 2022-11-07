using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Lib.Models.Processed.DataStewardship.Common
{
    public class Organisation
    {
        /// <summary>
        /// The name of an organisation.
        /// </summary>
        public string OrganisationCode { get; set; }

        /// <summary>
        /// The id-number of an organisation.
        /// </summary>
        public string OrganisationID { get; set; }
    }
}
