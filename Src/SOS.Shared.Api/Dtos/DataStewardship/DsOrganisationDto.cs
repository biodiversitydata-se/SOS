using System.ComponentModel.DataAnnotations;

namespace SOS.Shared.Api.Dtos.DataStewardship
{
    /// <summary>
    /// Organisation
    /// </summary>
    public class DsOrganisationDto
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
