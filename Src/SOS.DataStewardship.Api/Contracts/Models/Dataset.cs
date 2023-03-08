using SOS.DataStewardship.Api.Contracts.Enums;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace SOS.DataStewardship.Api.Contracts.Models
{
    [SwaggerSchema("Metadata about a dataset.", Required = new[] { "Identifier" })]
    public class Dataset
    {        
        [Required]
        [SwaggerSchema("The dataset id-number within the metadata catalogue administered by the authority that is responsible for the information.")]
        public string Identifier { get; set; }
        
        [Required]
        [SwaggerSchema("States which Creative Commons license that is applied to the dataset.  Note that any attachments included in the dataset have their own separate licenses.")]
        public string License { get; set; }

        [Required]
        [SwaggerSchema("Name of the dataset. Can sometimes be the same as the name of the project.")]
        public string Title { get; set; }

        [SwaggerSchema("Project")]
        public List<Project> Project { get; set; }

        [Required]
        [SwaggerSchema("The program area within environmental monitoring that the dataset belongs to.")]        
        public ProgrammeArea? ProgrammeArea { get; set; }

        [Required]
        [SwaggerSchema("The organisation that orders the data collection.")]
        public Organisation Assigner { get; set; }

        [Required]
        [SwaggerSchema("The organisation responsible for the data collection. More than  one organisation can be given here.")]
        public List<Organisation> Creator { get; set; }

        [Required]
        [SwaggerSchema("The organisation that is responsible for the information in the dataset.")]
        public Organisation OwnerinstitutionCode { get; set; }

        [Required]
        [SwaggerSchema("The organisation that receives the dataset from the data provider. Responsible for making the dataset accessible. (Also corresponding to: data host)")]
        public Organisation Publisher { get; set; }

        [Required]
        [SwaggerSchema("Name of the data stewardship within which the dataset is handled.")]
        public string DataStewardship { get; set; }
        
        [Required]        
        [SwaggerSchema("The purpose of the data collection (e.g. national or regional environmental monitoring).")]
        public Purpose? Purpose { get; set; }

        [Required]
        [SwaggerSchema("Short description of the dataset or the context for collection of the data included in the dataset. The structure and content of the description is governed by the requirements from the respective metadata catalogues.")]
        public string Description { get; set; }

        [Required]
        [SwaggerSchema("The methodology that is used for the data collection, e.g. as described by one or more monitoring manuals. Can contain a range of methods for the same and/or different parts of the data collection.")]
        public List<Methodology> Methodology { get; set; }

        [Required]
        [SwaggerSchema("Start date for the dataset.")]
        public DateTime? StartDate { get; set; }

        [SwaggerSchema("End date for the dataset.")]
        public DateTime? EndDate { get; set; }

        [Required]
        [SwaggerSchema("The country in which the data in the dataset were collected in.")]
        public string Spatial { get; set; }

        [Required]
        [SwaggerSchema("Information about whether the whole, parts of, or nothing in the dataset is publically available.")]
        public AccessRights? AccessRights { get; set; }

        [SwaggerSchema("When the dataset is not public or access is restricted  (see the property accessRights), how and/or why is described here.")]
        public string DescriptionAccessRights { get; set; }

        [Required]
        [SwaggerSchema("The language that is used when writing metadata about the dataset.")]
        public string Metadatalanguage { get; set; }

        [Required]
        [SwaggerSchema("The language used in the dataset.")]
        public string Language { get; set; }

        [SwaggerSchema("A list of unique identites for surveys that is part of the dataset.")]
        public List<string> EventIds { get; set; }
    }
}
