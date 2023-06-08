using SOS.Observations.Api.Dtos.DataStewardship.Enums;
using System.ComponentModel.DataAnnotations;

namespace SOS.Observations.Api.Dtos.DataStewardship
{
    /// <summary>
    /// Project
    /// </summary>
    public class DsProjectDto
    {
        /// <summary>
        /// Unique id for the project within which the dataset was collected.
        /// </summary>            
        public string ProjectID { get; set; }

        /// <summary>
        /// Name of the project within which the dataset was collected. Can sometimes be the same as the name of the dataset.
        /// </summary>            
        [Required]
        public string ProjectCode { get; set; }

        /// <summary>
        /// Type of project that the dataset was collected within, e.g. delprogram, gemensamt delprogram.
        /// </summary>            
        [Required]
        public DsProjectType? ProjectType { get; set; }

        public override string ToString()
        {
            return $"ProjectID: {ProjectID}, ProjectCode: {ProjectCode}, ProjectType: {ProjectType}";
        }
    }
}