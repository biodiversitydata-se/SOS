using System;
using System.Collections.Generic;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.DataStewardship.Enums;
using SOS.Lib.Models.Processed.DataStewardship.Common;

namespace SOS.Lib.Models.Processed.DataStewardship.Dataset
{
    public class DwcVerbatimObservationDataset : IEntity<int>
    {        
        /// <summary>
        ///     MongoDb Id. // todo - should we use Id, RecordId or occurrenceID as Id field?
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The dataset id-number within the metadata catalogue administered by the authority that is responsible for the information.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Name of the dataset. Can sometimes be the same as the name of the project.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Name of the data stewardship within which the dataset is handled.
        /// </summary>
        public string DataStewardship { get; set; }

        /// <summary>
        /// Project
        /// </summary>
        public List<Project> Project { get; set; }

        /// <summary>
        /// The program area within environmental monitoring that the dataset belongs to.
        /// </summary>                
        public ProgrammeArea? ProgrammeArea { get; set; }

        /// <summary>
        /// Assigner
        /// </summary>
        public Organisation Assigner { get; set; }

        /// <summary>
        /// Creator
        /// </summary>
        public List<Organisation> Creator { get; set; }

        /// <summary>
        /// OwnerinstitutionCode
        /// </summary>
        public Organisation OwnerinstitutionCode { get; set; }

        /// <summary>
        /// Publisher
        /// </summary>
        public Organisation Publisher { get; set; }

        /// <summary>
        /// The purpose of the data collection (e.g. national or regional environmental monitoring).
        /// </summary>
        public Purpose? Purpose { get; set; }

        /// <summary>
        /// Short description of the dataset or the context for collection of the data included in the dataset. The structure and content of the description is governed by the requirements from the respective metadata catalogues.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The methodology that is used for the data collection, e.g. as described by one or more monitoring manuals. Can contain a range of methods for the same and/or different parts of the data collection.
        /// </summary>
        public List<Methodology> Methodology { get; set; }

        /// <summary>
        /// Start date for the dataset.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// End date for the dataset.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// The country in which the data in the dataset were collected in.
        /// </summary>
        public string Spatial { get; set; }

        /// <summary>
        /// Information about whether the whole, parts of, or nothing in the dataset is publically available.
        /// </summary>
        public AccessRights? AccessRights { get; set; }

        /// <summary>
        /// When the dataset is not public or access is restricted  (see the property accessRights), how and/or why is described here.
        /// </summary>
        public string DescriptionAccessRights { get; set; }

        /// <summary>
        /// The language that is used when writing metadata about the dataset.
        /// </summary>
        public string Metadatalanguage { get; set; }

        /// <summary>
        /// The language used in the dataset.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// A list of unique identites for surveys that is part of the dataset.
        /// </summary>
        public List<string> EventIds { get; set; }
    }

}