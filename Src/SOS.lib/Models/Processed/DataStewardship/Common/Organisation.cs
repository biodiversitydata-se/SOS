using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
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
