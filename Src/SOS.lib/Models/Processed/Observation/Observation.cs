using System;
using System.Collections.Generic;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Swagger;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// Observation diffuse status.
    /// </summary>
    public enum DiffusionStatus
    {
        /// <summary>
        /// Observation is not diffused.
        /// </summary>
        NotDiffused = 0,
        /// <summary>
        /// Observation is diffused by the SOS system and the non diffused original exists in the protected index.
        /// </summary>
        DiffusedBySystem,
        /// <summary>
        /// Observation is diffused by provider. No original data exists in the system.
        /// </summary>
        DiffusedByProvider
    }

    /// <summary>
    ///     Information about a species observation.
    /// </summary>
    public class Observation : IEntity<string>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public Observation()
        {
            Created = DateTime.Now;
            DiffusionStatus = DiffusionStatus.NotDiffused;
        }

        /// <summary>
        ///     List of defects found in harvest.
        /// </summary>
        [SwaggerExclude]
        public IDictionary<string, string> Defects { get; set; }

        /// <summary>
        /// Observation diffuse status.
        /// </summary>
        public DiffusionStatus DiffusionStatus { get; set; }

        /// <summary>
        ///     The category of information pertaining to an event (an
        ///     action that occurs at a place and during a period of time).
        /// </summary>
        public Event Event { get; set; }

        /// <summary>
        ///     Geological information, such as stratigraphy, that qualifies a region or place.
        /// </summary>
        public GeologicalContext GeologicalContext { get; set; }

        /// <summary>
        ///     The category of information pertaining to taxonomic
        ///     determinations (the assignment of a scientific name).
        /// </summary>
        public Identification Identification { get; set; }

        /// <summary>
        ///     A spatial region or named place. For Darwin Core,
        ///     a set of terms describing a place, whether named or not.
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        ///     A physical result of a sampling (or subsampling) event. In biological collections,
        ///     the material sample is typically collected, and either preserved or destructively processed.
        /// </summary>
        /// <example>
        ///     A whole organism preserved in a collection. A part of an organism isolated for some purpose.
        ///     A soil sample. A marine microbial sample.
        /// </example>
        public MaterialSample MaterialSample { get; set; }

        /// <summary>
        ///     The category of information pertaining to evidence of
        ///     an occurrence in nature, in a collection, or in a
        ///     dataset (specimen, observation, etc.).
        /// </summary>
        public Occurrence Occurrence { get; set; }

        /// <summary>
        ///     A particular organism or defined group of organisms considered to be taxonomically homogeneous.
        /// </summary>
        public Organism Organism { get; set; }

        /// <summary>
        ///     The category of information pertaining to taxonomic names,
        ///     taxon name usages, or taxon concepts.
        /// </summary>
        public Taxon Taxon { get; set; }

        #region Record level

        /// <summary>
        ///     Information about who can access the resource or
        ///     an indication of its security status.
        ///     Access Rights may include information regarding
        ///     access or restrictions based on privacy, security,
        ///     or other policies.
        /// </summary>
        /// <remarks>
        ///     This field uses a controlled vocabulary.
        public VocabularyValue AccessRights { get; set; }

        /// <summary>
        ///     The specific nature of the data record -
        ///     a subtype of the dcterms:type.
        ///     Recommended best practice is to use a controlled
        ///     vocabulary such as the Darwin Core Type Vocabulary
        ///     (http://rs.tdwg.org/dwc/terms/type-vocabulary/index.htm).
        /// </summary>
        /// <remarks>
        ///     This field uses a controlled vocabulary.
        /// </remarks>
        public VocabularyValue BasisOfRecord { get; set; }

        /// <summary>
        ///     A bibliographic reference for the resource as a statement
        ///     indicating how this record should be cited (attributed)
        ///     when used.
        ///     Recommended practice is to include sufficient
        ///     bibliographic detail to identify the resource as
        ///     unambiguously as possible.
        /// </summary>
        public string BibliographicCitation { get; set; }

        /// <summary>
        ///     The name, acronym, coden, or initialism identifying the
        ///     collection or data set from which the record was derived.
        /// </summary>
        public string CollectionCode { get; set; }

        /// <summary>
        ///     An identifier for the collection or dataset from which
        ///     the record was derived.
        ///     For physical specimens, the recommended best practice is
        ///     to use the identifier in a collections registry such as
        ///     the Biodiversity Collections Index
        ///     (http://www.biodiversitycollectionsindex.org/).
        /// </summary>
        public string CollectionId { get; set; }
       
        /// <summary>
        ///     Actions taken to make the shared data less specific or
        ///     complete than in its original form.
        ///     Suggests that alternative data of higher quality
        ///     may be available on request.
        /// </summary>
        public string DataGeneralizations { get; set; }

        /// <summary>
        ///     Data provider id.
        /// </summary>
        public int DataProviderId { get; set; }

        /// <summary>
        ///     An identifier for the set of data.
        ///     May be a global unique identifier or an identifier
        ///     specific to a collection or institution.
        /// </summary>
        public string DatasetId { get; set; }

        /// <summary>
        ///     The name identifying the data set
        ///     from which the record was derived.
        /// </summary>
        public string DatasetName { get; set; }

        /// <summary>
        ///     A list of additional measurements, facts, characteristics, or assertions about the record.
        ///     Meant to provide a mechanism for structured content.
        ///     Recommended best practice is to use a key:value encoding schema for a data interchange format such as JSON.
        /// </summary>
        public string DynamicProperties { get; set; }

        /// <summary>
        ///     Unique id.
        /// </summary>
        /// <remarks>
        /// Omit to automatically generate an id on insert (best performance).
        /// </remarks>
        public string Id { get; set; }

        /// <summary>
        ///     Additional information that exists, but that has
        ///     not been shared in the given record.
        /// </summary>
        public string InformationWithheld { get; set; }

        /// <summary>
        /// An identifier for the institution having custody of the object(s) or information referred to in the record.
        /// </summary>
        public string InstitutionId { get; set; }

        /// <summary>
        ///     The name (or acronym) in use by the institution having custody of
        ///     the object(s) or information referred to in the record.
        /// </summary>
        /// <remarks>
        ///     This field uses a controlled vocabulary.
        /// </remarks>
        public VocabularyValue InstitutionCode { get; set; }

        /// <summary>
        ///     A language of the resource.
        ///     Recommended best practice is to use a controlled
        ///     vocabulary such as RFC 4646 [RFC4646].
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        ///     A legal document giving official permission to do something with the resource.
        /// </summary>
        /// <example>
        ///     http://creativecommons.org/publicdomain/zero/1.0/legalcode,
        ///     http://creativecommons.org/licenses/by/4.0/legalcode
        /// </example>
        public string License { get; set; }

        /// <summary>
        ///     The most recent date-time on which the resource was changed (UTC).
        ///     For Darwin Core, recommended best practice is to use an
        ///     encoding scheme, such as ISO 8601:2004(E).
        /// </summary>

        public DateTime? Modified { get; set; }

        /// <summary>
        ///     The name (or acronym) in use by the institution having
        ///     ownership of the object(s) or information referred
        ///     to in the record.
        /// </summary>
        public string OwnerInstitutionCode { get; set; }

        /// <summary>
        ///     Private Collection.
        /// </summary>
        public string PrivateCollection { get; set; }

        /// <summary>
        /// Indicates whether the observation is protected.
        /// This property is deprecated and replaced by the Sensitive property.
        /// </summary>
        [Obsolete("Replaced by Sensitive")]
        public bool Protected { get; set; }

        /// <summary>
        /// Indicates whether the observation is sensitive and therefore protected.
        /// </summary>
        public bool Sensitive { get; set; }

        /// <summary>
        ///     Public Collection.
        /// </summary>
        public string PublicCollection { get; set; }

        /// <summary>
        ///     A related resource that is referenced, cited,
        ///     or otherwise pointed to by the described resource.
        /// </summary>
        public string References { get; set; }

        /// <summary>
        ///     A person or organization owning or
        ///     managing rights over the resource.
        /// </summary>
        public string RightsHolder { get; set; }

        /// <summary>
        ///     Species collection label.
        /// </summary>
        public string SpeciesCollectionLabel { get; set; }

        /// <summary>
        ///     The nature or genre of the resource.
        ///     For Darwin Core, recommended best practice is
        ///     to use the name of the class that defines the
        ///     root of the record.
        /// </summary>
        /// <remarks>
        ///     This field uses a controlled vocabulary.
        /// </remarks>
        public VocabularyValue Type { get; set; }

        /// <summary>
        ///     Properties only used by Artportalen.
        /// </summary>
        public ArtportalenInternal ArtportalenInternal { get; set; }

        #endregion Record level

        /// <summary>
        ///     Measurement or facts associated with the observation.
        /// </summary>
        public ICollection<ExtendedMeasurementOrFact> MeasurementOrFacts { get; set; }

        /// <summary>
        ///     Projects from Artportalen associated with the observation.
        /// </summary>
        public IEnumerable<Project> Projects { get; set; }

        /// <summary>
        /// The date the observation was created (UTC).
        /// </summary>
        public DateTime Created { get; set; }

        public DataQuality DataQuality { get; set; }
    }
}