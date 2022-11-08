using SOS.DataStewardship.Api.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Security.AccessControl;

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
        /// States which Creative Commons license that is applied to the dataset.  Note that any attachments included in the dataset have their own separate licenses.
        /// </summary>
        /// <value>States which Creative Commons license that is applied to the dataset.  Note that any attachments included in the dataset have their own separate licenses.</value>
        [Required]
        [DataMember(Name = "license")]
        public string License { get; set; }

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
        /// The organisation that orders the data collection.
        /// </summary>
        [Required]
        [DataMember(Name="assigner")]
        public Organisation Assigner { get; set; }

        /// <summary>
        /// The organisation responsible for the data collection. More than  one organisation can be given here.
        /// </summary>
        /// <value>The organisation responsible for the data collection. More than  one organisation can be given here.</value>
        [Required]
        [DataMember(Name = "creator")]
        public List<Organisation> Creator { get; set; }

        /// <summary>
        /// The organisation that is responsible for the information in the dataset.
        /// </summary>
        [Required]
        [DataMember(Name="ownerinstitutionCode")]
        public Organisation OwnerinstitutionCode { get; set; }

        /// <summary>
        /// The organisation that receives the dataset from the data provider. Responsible for making the dataset accessible. (Also corresponding to: data host)
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
        [Required]
        [DataMember(Name="purpose")]
        public Purpose? Purpose { get; set; }

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
        [Required]
        [DataMember(Name="accessRights")]
        public AccessRights? AccessRights { get; set; }

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
