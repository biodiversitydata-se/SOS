using System.ComponentModel.DataAnnotations;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    /// <summary>
    /// Organisation
    /// </summary>
    public class Organisation
    {
        /// <summary>
        /// The name of an organisation.
        /// </summary>
        [Required]
        public string OrganisationCode { get; set; }

        /// <summary>
        /// The id-number of an organisation.
        /// </summary>
        public string OrganisationID { get; set; }
    }
}
