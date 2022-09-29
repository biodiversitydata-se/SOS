using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace SOS.DataStewardship.Api.Models
{ 
    /// <summary>
    /// Metadata about a dataset.
    /// </summary>
    [DataContract]
    public class Dataset
    { 
        /// <summary>
        /// The dataset id-number within the metadata catalogue administered by the authority that is responsible for the information.
        /// </summary>
        [Required]
        [DataMember(Name="identifier")]
        public string Identifier { get; set; }

        /// <summary>
        /// Name of the dataset. Can sometimes be the same as the name of the project.
        /// </summary>
        [Required]
        [DataMember(Name="title")]
        public string Title { get; set; }

        /// <summary>
        /// Unique id for the project within which the dataset was collected.
        /// </summary>
        [DataMember(Name="projectID")]
        public string ProjectID { get; set; }

        /// <summary>
        /// Name of the project within which the dataset was collected. Can sometimes be the same as the name of the dataset.
        /// </summary>
        [DataMember(Name="projectCode")]
        public string ProjectCode { get; set; }

        /// <summary>
        /// Assigner
        /// </summary>
        [Required]
        [DataMember(Name="assigner")]
        public Organisation Assigner { get; set; }

        /// <summary>
        /// Creator
        /// </summary>
        [Required]
        [DataMember(Name="creator")]
        public Organisation Creator { get; set; }

        /// <summary>
        /// OwnerinstitutionCode
        /// </summary>
        [Required]
        [DataMember(Name="ownerinstitutionCode")]
        public Organisation OwnerinstitutionCode { get; set; }

        /// <summary>
        /// Publisher
        /// </summary>
        [Required]
        [DataMember(Name="publisher")]
        public Organisation Publisher { get; set; }

        /// <summary>
        /// Name of the data stewardship within which the dataset is handled.
        /// </summary>
        [Required]
        [DataMember(Name="dataStewardship")]
        public string DataStewardship { get; set; }

        /// <summary>
        /// The purpose of the data collection (e.g. national or regional environmental monitoring).
        /// </summary>
        public enum PurposeEnum
        {
            /// <summary>
            /// Nationell miljöövervakning
            /// </summary>
            [EnumMember(Value = "nationell miljöövervakning")]
            NationellMiljöövervakning = 0,
            /// <summary>
            /// Regional miljöövervakning
            /// </summary>
            [EnumMember(Value = "regional miljöövervakning")]
            RegionalMiljöövervakning = 1,
            /// <summary>
            /// Biogeografisk uppföljning
            /// </summary>
            [EnumMember(Value = "biogeografisk uppföljning")]
            biogeografiskUppföljning = 2
        }

        /// <summary>
        /// The purpose of the data collection (e.g. national or regional environmental monitoring).
        /// </summary>
        [Required]
        [DataMember(Name="purpose")]
        public PurposeEnum? Purpose { get; set; }

        /// <summary>
        /// Short description of the dataset or the context for collection of the data included in the dataset. The structure and content of the description is governed by the requirements from the respective metadata catalogues.
        /// </summary>
        [Required]
        [DataMember(Name="description")]
        public string Description { get; set; }

        /// <summary>
        /// The methodology that is used for the data collection, e.g. as described by one or more monitoring manuals. Can contain a range of methods for the same and/or different parts of the data collection.
        /// </summary>
        [Required]
        [DataMember(Name="methodology")]
        public List<Methodology> Methodology { get; set; }

        /// <summary>
        /// Start date for the dataset.
        /// </summary>
        [Required]
        [DataMember(Name="startDate")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date for the dataset.
        /// </summary>
        [DataMember(Name="endDate")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The country in which the data in the dataset were collected in.
        /// </summary>
        [Required]
        [DataMember(Name="spatial")]
        public string Spatial { get; set; }

        /// <summary>
        /// Information about whether the whole, parts of, or nothing in the dataset is publically available.
        /// </summary>
        public enum AccessRightsEnum
        {
            /// <summary>
            /// publik
            /// </summary>
            [EnumMember(Value = "publik")]
            Publik = 0,
            /// <summary>
            /// begränsad
            /// </summary>
            [EnumMember(Value = "begränsad")]
            Begränsad = 1,
            /// <summary>
            /// ej offentlig
            /// </summary>
            [EnumMember(Value = "ej offentlig")]
            EjOffentlig = 2
        }

        /// <summary>
        /// Information about whether the whole, parts of, or nothing in the dataset is publically available.
        /// </summary>
        [Required]
        [DataMember(Name="accessRights")]
        public AccessRightsEnum? AccessRights { get; set; }

        /// <summary>
        /// The language that is used when writing metadata about the dataset.
        /// </summary>
        [Required]
        [DataMember(Name="metadatalanguage")]
        public string Metadatalanguage { get; set; }

        /// <summary>
        /// The language used in the dataset.
        /// </summary>
        [Required]
        [DataMember(Name="language")]
        public string Language { get; set; }

        /// <summary>
        /// A list of unique identites for surveys that is part of the dataset.
        /// </summary>
        [DataMember(Name="events")]
        public List<string> Events { get; set; }
    }
}
