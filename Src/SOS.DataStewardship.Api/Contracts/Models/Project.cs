using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using SOS.DataStewardship.Api.Contracts.Enums;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract]
    public class Project
    {
        /// <summary>
        /// Unique id for the project within which the dataset was collected.
        /// </summary>            
        [DataMember(Name = "projectID")]
        public string ProjectID { get; set; }

        /// <summary>
        /// Name of the project within which the dataset was collected. Can sometimes be the same as the name of the dataset.
        /// </summary>            
        [Required]
        [DataMember(Name = "projectCode")]
        public string ProjectCode { get; set; }

        /// <summary>
        /// Type of project that the dataset was collected within, e.g. delprogram, gemensamt delprogram.
        /// </summary>            
        [Required]
        [DataMember(Name = "projectType")]
        public ProjectType? ProjectType { get; set; }

        public override string ToString()
        {
            return $"ProjectID: {ProjectID}, ProjectCode: {ProjectCode}, ProjectType: {ProjectType}";
        }
    }
}