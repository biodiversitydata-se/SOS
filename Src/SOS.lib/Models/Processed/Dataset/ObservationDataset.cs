using Nest;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Processed.Dataset
{
    public class ObservationDataset : IEntity<string>
    {
        /// <summary>
        ///     Unique id.
        /// </summary>
        /// <remarks>
        /// Omit to automatically generate an id on insert (best performance).
        /// </remarks>
        public string Id { get; set; }

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
        /// Unique id for the project within which the dataset was collected.
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// Name of the project within which the dataset was collected. Can sometimes be the same as the name of the dataset.
        /// </summary>
        public string ProjectCode { get; set; }

        /// <summary>
        /// Assigner
        /// </summary>
        public Organisation Assigner { get; set; }

        /// <summary>
        /// Creator
        /// </summary>
        public Organisation Creator { get; set; }

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
        public PurposeEnum? Purpose { get; set; }

        /// <summary>
        /// Short description of the dataset or the context for collection of the data included in the dataset. The structure and content of the description is governed by the requirements from the respective metadata catalogues.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The methodology that is used for the data collection, e.g. as described by one or more monitoring manuals. Can contain a range of methods for the same and/or different parts of the data collection.
        /// </summary>
        public List<MethodologyModel> Methodology { get; set; }

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
        public AccessRightsEnum? AccessRights { get; set; }

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

        public class Organisation
        {
            /// <summary>
            /// The name of an organisation.
            /// </summary>
            public string OrganisationCode { get; set; }

            /// <summary>
            /// The id-number of an organisation.
            /// </summary>
            public string OrganisationID { get; set; }
        }

        /// <summary>
        /// The methodology that is used for the data collection, e.g. as described by one or more monitoring manuals.
        /// Can contain a range of methods for the same and/or different parts of the data collection.
        /// </summary>
        public class MethodologyModel
        {
            /// <summary>
            /// The title or name of a methodology, e.g. a monitoring manual.
            /// </summary>
            public string MethodologyName { get; set; }

            /// <summary>
            /// Short description of a methodology, e.g. a monitoring manual.
            /// </summary>
            public string MethodologyDescription { get; set; }

            /// <summary>
            /// Persistent link to description of a methodology, e.g. a monitoring manual.
            /// </summary>
            public string MethodologyLink { get; set; }

            /// <summary>
            /// Persistent link to published species list for the dataset.
            /// </summary>
            public string SpeciesList { get; set; }
        }

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
            BiogeografiskUppföljning = 2
        }
    }
}