using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace SOS.DataStewardship.Api.Models
{ 
    /// <summary>
    /// The methodology that is used for the data collection, e.g. as described by one or more monitoring manuals. Can contain a range of methods for the same and/or different parts of the data collection.
    /// </summary>
    [DataContract]
    public class Methodology
    { 
        /// <summary>
        /// The title or name of a methodology, e.g. a monitoring manual.
        /// </summary>
        [Required]
        [DataMember(Name="methodologyName")]
        public string MethodologyName { get; set; }

        /// <summary>
        /// Short description of a methodology, e.g. a monitoring manual.
        /// </summary>
        [Required]
        [DataMember(Name="methodologyDescription")]
        public string MethodologyDescription { get; set; }

        /// <summary>
        /// Persistent link to description of a methodology, e.g. a monitoring manual.
        /// </summary>
        [Required]
        [DataMember(Name="methodologyLink")]
        public string MethodologyLink { get; set; }

        /// <summary>
        /// Persistent link to published species list for the dataset.
        /// </summary>
        [Required]
        [DataMember(Name="speciesList")]
        public string SpeciesList { get; set; }
    }
}
