using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class Organisation
    {
        /// <summary>
        /// The name of an organisation.
        /// </summary>
        [Required]
        [DataMember(Name = "organisationCode")]
        public string OrganisationCode { get; set; }

        /// <summary>
        /// The id-number of an organisation.
        /// </summary>
        [DataMember(Name = "organisationID")]
        public string OrganisationID { get; set; }
    }
}
