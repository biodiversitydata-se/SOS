using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Models.Processed.Sighting
{
    /// <summary>
    /// This class contains information about a species sighting
    /// </summary>
    public class ProcessedSighting : IEntity<ObjectId>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="provider"></param>
        public ProcessedSighting(DataProvider provider)
        {
            Provider = provider;
        }
        /// <summary>
        /// Information about who can access the resource or
        /// an indication of its security status.
        /// Access Rights may include information regarding
        /// access or restrictions based on privacy, security,
        /// or other policies.
        /// </summary>
        public string AccessRights { get; set; }

        /// <summary>
        /// The specific nature of the data record -
        /// a subtype of the dcterms:type.
        /// Recommended best practice is to use a controlled
        /// vocabulary such as the Darwin Core Type Vocabulary
        /// (http://rs.tdwg.org/dwc/terms/type-vocabulary/index.htm).
        /// In Species Gateway this property has the value
        /// HumanObservation.
        /// </summary>
        public string BasisOfRecord { get; set; }

        /// <summary>
        /// A bibliographic reference for the resource as a statement
        /// indicating how this record should be cited (attributed)
        /// when used.
        /// Recommended practice is to include sufficient
        /// bibliographic detail to identify the resource as
        /// unambiguously as possible.
        /// This property is currently not used.
        /// </summary>
        public string BibliographicCitation { get; set; }

        /// <summary>
        /// The name, acronym, coden, or initialism identifying the 
        /// collection or data set from which the record was derived.
        /// </summary>
        public string CollectionCode { get; set; }

        /// <summary>
        /// An identifier for the collection or dataset from which
        /// the record was derived.
        /// For physical specimens, the recommended best practice is
        /// to use the identifier in a collections registry such as
        /// the Biodiversity Collections Index
        /// (http://www.biodiversitycollectionsindex.org/).
        /// </summary>
        public string CollectionId { get; set; }

        /// <summary>
        /// Actions taken to make the shared data less specific or
        /// complete than in its original form.
        /// Suggests that alternative data of higher quality
        /// may be available on request.
        /// This property is currently not used.
        /// </summary>
        public string DataGeneralizations { get; set; }

        /// <summary>
        /// An identifier for the set of data.
        /// May be a global unique identifier or an identifier
        /// specific to a collection or institution.
        /// </summary>
        public string DatasetId { get; set; }

        /// <summary>
        /// The name identifying the data set
        /// from which the record was derived.
        /// </summary>
        public string DatasetName { get; set; }

        /// <summary>
        /// The category of information pertaining to an event (an 
        /// action that occurs at a place and during a period of time).
        /// </summary>
        public ProcessedEvent Event { get; set; }

       /// <summary>
        /// Mongodb id
        /// </summary>
        [BsonId]
        public ObjectId Id { get; set; }

        /// <summary>
        /// The category of information pertaining to taxonomic
        /// determinations (the assignment of a scientific name).
        /// </summary>
        public ProcessedIdentification Identification { get; set; }

        /// <summary>
        /// Additional information that exists, but that has
        /// not been shared in the given record.
        /// This property is currently not used.
        /// </summary>
        public string InformationWithheld { get; set; }

        /// <summary>
        /// The name (or acronym) in use by the institution
        /// having custody of the object(s) or information
        /// referred to in the record.
        /// Currently this property has the value ArtDatabanken.
        /// </summary>
        public Metadata Institution { get; set; }

        /// <summary>
        /// Internal flag used in validation. must be true to be stored in processed data
        /// </summary>
        [JsonIgnore]
        public bool IsInEconomicZoneOfSweden { get; set; }

        /// <summary>
        /// A language of the resource.
        /// Recommended best practice is to use a controlled
        /// vocabulary such as RFC 4646 [RFC4646].
        /// This property is currently not used.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// </summary>
        /// <example>
        /// http://creativecommons.org/publicdomain/zero/1.0/legalcode,
        /// http://creativecommons.org/licenses/by/4.0/legalcode
        /// </example>
        public string License { get; set; }

        /// <summary>
        /// A spatial region or named place. For Darwin Core,
        /// a set of terms describing a place, whether named or not.
        /// </summary>
        public ProcessedLocation Location { get; set; }

        /// <summary>
        /// A physical result of a sampling (or subsampling) event. In biological collections, the material sample is typically collected, and either preserved or destructively processed.
        /// </summary>
        /// <example>
        /// A whole organism preserved in a collection. A part of an organism isolated for some purpose. A soil sample. A marine microbial sample.
        /// </example>
        public ProcessedMaterialSample MaterialSample { get; set; }


        /// <summary>
        /// The most recent date-time on which the resource was changed.
        /// For Darwin Core, recommended best practice is to use an
        /// encoding scheme, such as ISO 8601:2004(E).
        /// </summary>
        public DateTime? Modified { get; set; }

        /// <summary>
        /// The category of information pertaining to evidence of
        /// an occurrence in nature, in a collection, or in a
        /// dataset (specimen, observation, etc.).
        /// </summary>
        public ProcessedOccurrence Occurrence { get; set; }

        /// <summary>
        /// The name (or acronym) in use by the institution having
        /// ownership of the object(s) or information referred
        /// to in the record.
        /// This property is currently not used.
        /// </summary>
        public string OwnerInstitutionCode { get; set; }

        /// <summary>
        /// Projects connected to sighting
        /// </summary>
        public IEnumerable<ProcessedProject> Projects { get; set; }

        /// <summary>
        /// Protection level
        /// </summary>
        public int ProtectionLevel { get; set; }

        /// <summary>
        /// Internal use. Provider of the data
        /// </summary>
        [JsonIgnore]
        public DataProvider Provider { get; set; }

        /// <summary>
        /// A related resource that is referenced, cited,
        /// or otherwise pointed to by the described resource.
        /// This property is currently not used.
        /// </summary>
        public string References { get; set; }

        // <summary>
        /// Name of the person that reported the species observation.
        /// </summary>
        public string ReportedBy { get; set; }

        /// <summary>
        /// Date and time when the species observation was reported.
        /// </summary>
        public DateTime? ReportedDate { get; set; }

        /// <summary>
        /// Information about rights held in and over the resource.
        /// Typically, rights information includes a statement
        /// about various property rights associated with the resource,
        /// including intellectual property rights.
        /// This property is currently not used.
        /// </summary>
        public string Rights { get; set; }

        /// <summary>
        /// A person or organization owning or
        /// managing rights over the resource.
        /// This property is currently not used.
        /// </summary>
        public string RightsHolder { get; set; }

        /// <summary>
        /// The category of information pertaining to taxonomic names,
        /// taxon name usages, or taxon concepts.
        /// </summary>
        public ProcessedTaxon Taxon { get; set; }

        /// <summary>
        /// The nature or genre of the resource.
        /// For Darwin Core, recommended best practice is
        /// to use the name of the class that defines the
        /// root of the record.
        /// This property is currently not used.
        /// </summary>
        public string Type { get; set; }
    }
}
