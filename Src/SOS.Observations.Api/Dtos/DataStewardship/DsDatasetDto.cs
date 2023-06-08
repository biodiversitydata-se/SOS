using SOS.Observations.Api.Dtos.DataStewardship.Enums;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SOS.Observations.Api.Dtos.DataStewardship
{
    /// <summary>
    /// Metadata about a dataset.
    /// </summary>
    [SwaggerSchema("Metadata about a dataset.", Required = new[] { "Identifier" })]
    public class DsDatasetDto
    {
        /// <summary>
        /// The dataset id-number within the metadata catalogue administered by the authority that is responsible for the information.
        /// </summary>
        [Required]
        [SwaggerSchema("The dataset id-number within the metadata catalogue administered by the authority that is responsible for the information.")]
        public string Identifier { get; set; }

        /// <summary>
        /// States which Creative Commons license that is applied to the dataset. Note that any attachments included in the dataset have their own separate licenses.
        /// </summary>
        [Required]
        [SwaggerSchema("States which Creative Commons license that is applied to the dataset. Note that any attachments included in the dataset have their own separate licenses.")]
        public string License { get; set; }

        /// <summary>
        /// Name of the dataset. Can sometimes be the same as the name of the project.
        /// </summary>
        [Required]
        [SwaggerSchema("Name of the dataset. Can sometimes be the same as the name of the project.")]
        public string Title { get; set; }

        /// <summary>
        /// Projects
        /// </summary>
        [SwaggerSchema("Project")]
        public IEnumerable<DsProjectDto> Projects { get; set; }

        /// <summary>
        /// The program area within environmental monitoring that the dataset belongs to.
        /// </summary>
        [Required]
        [SwaggerSchema("The program area within environmental monitoring that the dataset belongs to.")]
        public DsProgrammeArea? ProgrammeArea { get; set; }

        /// <summary>
        /// The organisation that orders the data collection.
        /// </summary>
        [Required]
        [SwaggerSchema("The organisation that orders the data collection.")]
        public DsOrganisationDto Assigner { get; set; }

        /// <summary>
        /// The organisation responsible for the data collection. More than  one organisation can be given here.
        /// </summary>
        [Required]
        [SwaggerSchema("The organisation responsible for the data collection. More than  one organisation can be given here.")]
        public IEnumerable<DsOrganisationDto> Creator { get; set; }

        /// <summary>
        /// The organisation that is responsible for the information in the dataset.
        /// </summary>
        [Required]
        [SwaggerSchema("The organisation that is responsible for the information in the dataset.")]
        public DsOrganisationDto OwnerinstitutionCode { get; set; }

        /// <summary>
        /// The organisation that receives the dataset from the data provider. Responsible for making the dataset accessible. (Also corresponding to: data host)
        /// </summary>
        [Required]
        [SwaggerSchema("The organisation that receives the dataset from the data provider. Responsible for making the dataset accessible. (Also corresponding to: data host)")]
        public DsOrganisationDto Publisher { get; set; }

        /// <summary>
        /// Name of the data stewardship within which the dataset is handled.
        /// </summary>
        [Required]
        [SwaggerSchema("Name of the data stewardship within which the dataset is handled.")]
        public string DataStewardship { get; set; }

        /// <summary>
        /// The purpose of the data collection (e.g. national or regional environmental monitoring).
        /// </summary>
        [Required]
        [SwaggerSchema("The purpose of the data collection (e.g. national or regional environmental monitoring).")]
        public DsPurpose? Purpose { get; set; }

        /// <summary>
        /// Short description of the dataset or the context for collection of the data included in the dataset. 
        /// The structure and content of the description is governed by the requirements from the respective metadata catalogues.
        /// </summary>
        [Required]
        [SwaggerSchema("Short description of the dataset or the context for collection of the data included in the dataset. The structure and content of the description is governed by the requirements from the respective metadata catalogues.")]
        public string Description { get; set; }

        /// <summary>
        /// The methodology that is used for the data collection, e.g. as described by one or more monitoring manuals. 
        /// Can contain a range of methods for the same and/or different parts of the data collection.
        /// </summary>
        [Required]
        [SwaggerSchema("The methodology that is used for the data collection, e.g. as described by one or more monitoring manuals. Can contain a range of methods for the same and/or different parts of the data collection.")]
        public IEnumerable<DsMethodologyDto> Methodology { get; set; }

        /// <summary>
        /// Start date for the dataset.
        /// </summary>
        [Required]
        [SwaggerSchema("Start date for the dataset.")]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date for the dataset.
        /// </summary>
        [SwaggerSchema("End date for the dataset.")]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The country in which the data in the dataset were collected in.
        /// </summary>
        [Required]
        [SwaggerSchema("The country in which the data in the dataset were collected in.")]
        public string Spatial { get; set; }

        /// <summary>
        /// Information about whether the whole, parts of, or nothing in the dataset is publically available.
        /// </summary>
        [Required]
        [SwaggerSchema("Information about whether the whole, parts of, or nothing in the dataset is publically available.")]
        public DsAccessRights? AccessRights { get; set; }

        /// <summary>
        /// "When the dataset is not public or access is restricted  (see the property accessRights), how and/or why is described here.
        /// </summary>
        [SwaggerSchema("When the dataset is not public or access is restricted  (see the property accessRights), how and/or why is described here.")]
        public string DescriptionAccessRights { get; set; }

        /// <summary>
        /// The language that is used when writing metadata about the dataset.
        /// </summary>
        [Required]
        [SwaggerSchema("The language that is used when writing metadata about the dataset.")]
        public string Metadatalanguage { get; set; }

        /// <summary>
        /// The language used in the dataset.
        /// </summary>
        [Required]
        [SwaggerSchema("The language used in the dataset.")]
        public string Language { get; set; }

        /// <summary>
        /// A list of unique identites for surveys that is part of the dataset.
        /// </summary>
        [SwaggerSchema("A list of unique identites for surveys that is part of the dataset.")]
        public IEnumerable<string> EventIds { get; set; }
    }
}
