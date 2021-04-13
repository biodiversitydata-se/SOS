using System;
using System.Collections.Generic;
using Nest;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    ///     Occurrence information about a species observation.
    /// </summary>
    public class Occurrence
    {
        /// <summary>
        ///     Activity property.
        /// </summary>
        /// <remarks>
        ///     This field uses a controlled vocabulary.
        /// </remarks>
        [Object]
        public VocabularyValue
            Activity { get; set; }

        /// <summary>
        ///     A list (concatenated and separated) of identifiers
        ///     (publication, global unique identifier, URI) of
        ///     media associated with the Occurrence.
        /// </summary>
        public string AssociatedMedia { get; set; }

        /// <summary>
        ///     A list (concatenated and separated) of identifiers of
        ///     other Occurrence records and their associations to
        ///     this Occurrence.
        /// </summary>
        public string AssociatedOccurrences { get; set; }

        /// <summary>
        ///     A list (concatenated and separated) of identifiers
        ///     (publication, bibliographic reference, global unique
        ///     identifier, URI) of literature associated with
        ///     the Occurrence.
        /// </summary>
        public string AssociatedReferences { get; set; }

        /// <summary>
        ///     A list (concatenated and separated) of identifiers of
        ///     other Occurrence records and their associations to
        ///     this Occurrence.
        /// </summary>
        public string AssociatedSequences { get; set; }

        /// <summary>
        ///     A list (concatenated and separated) of identifiers or
        ///     names of taxa and their associations with the Occurrence.
        /// </summary>
        public string AssociatedTaxa { get; set; }

        /// <summary>
        ///     A description of the behavior shown by the subject at
        ///     the time the Occurrence was recorded.
        /// </summary>
        /// <remarks>
        ///     This field uses a controlled vocabulary.
        /// </remarks>
        [Object]
        public VocabularyValue Behavior { get; set; }

        /// <summary>
        ///     Biotope.
        /// </summary>
        /// <remarks>
        ///     This field uses a controlled vocabulary.
        /// </remarks>
        [Object]
        public VocabularyValue Biotope { get; set; }

        /// <summary>
        ///     Description of biotope.
        /// </summary>
        public string BiotopeDescription { get; set; }

        /// <summary>
        ///     Bird nest activity.
        /// </summary>
        public int? BirdNestActivityId { get; set; }

        /// <summary>
        ///     An identifier (preferably unique) for the record
        ///     within the data set or collection.
        /// </summary>
        public string CatalogNumber { get; set; }
        
        /// <summary>
        /// Catalog Id.
        /// </summary>
        public int CatalogId { get; set; }

        /// <summary>
        ///     The current state of a specimen with respect to the
        ///     collection identified in collectionCode or collectionID.
        ///     Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        public string Disposition { get; set; }

        /// <summary>
        ///     The process by which the biological individual(s)
        ///     represented in the Occurrence became established at the
        ///     location.
        ///     Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        /// <remarks>
        ///     This field uses a controlled vocabulary.
        /// </remarks>
        [Object]
        public VocabularyValue EstablishmentMeans { get; set; }

        /// <summary>
        ///     The number of individuals represented present
        ///     at the time of the Occurrence.
        /// </summary>
        public string IndividualCount { get; set; }

        /// <summary>
        ///     An identifier for an individual or named group of
        ///     individual organisms represented in the Occurrence.
        ///     Meant to accommodate resampling of the same individual
        ///     or group for monitoring purposes. May be a global unique
        ///     identifier or an identifier specific to a data set.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string IndividualID { get; set; }

        /// <summary>
        ///     Indicates if this species occurrence is natural or
        ///     if it is a result of human activity.
        /// </summary>
        public bool? IsNaturalOccurrence { get; set; }

        /// <summary>
        ///     Indicates if this observation is a never found observation.
        ///     "Never found observation" is an observation that says
        ///     that the specified species was not found in a location
        ///     deemed appropriate for the species.
        /// </summary>
        public bool? IsNeverFoundObservation { get; set; }

        /// <summary>
        ///     Indicates if this observation is a
        ///     not rediscovered observation.
        ///     "Not rediscovered observation" is an observation that says
        ///     that the specified species was not found in a location
        ///     where it has previously been observed.
        /// </summary>
        public bool? IsNotRediscoveredObservation { get; set; }

        /// <summary>
        ///     Indicates if this observation is a positive observation.
        ///     "Positive observation" is a normal observation indicating
        ///     that a species has been seen at a specified location.
        /// </summary>
        public bool? IsPositiveObservation { get; set; }

        /// <summary>
        ///     The age class or life stage of the biological individual(s)
        ///     at the time the Occurrence was recorded.
        ///     Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        /// <remarks>
        ///     This field uses a controlled vocabulary.
        /// </remarks>
        [Object]
        public VocabularyValue LifeStage { get; set; }

        /// <summary>
        ///     Media linked to the observation
        /// </summary>
        [Nested]
        public ICollection<Multimedia> Media { get; set; }

        /// <summary>
        /// An identifier for the Occurrence (as opposed to a particular digital record of the occurrence).
        /// In the absence of a persistent global unique identifier, construct one from a combination of
        /// identifiers in the record that will most closely make the occurrenceID globally unique.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        [Keyword]
        public string OccurrenceId { get; set; }

        /// <summary>
        ///     Comments or notes about the Occurrence.
        /// </summary>
        public string OccurrenceRemarks { get; set; }

        /// <summary>
        ///     A statement about the presence or absence of a Taxon at a
        ///     Location.
        ///     Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        /// <remarks>
        ///     This field uses a controlled vocabulary.
        /// </remarks>
        [Object]
        public VocabularyValue OccurrenceStatus { get; set; }

        /// <summary>
        ///     A list (concatenated and separated) of previous or
        ///     alternate fully qualified catalog numbers or other
        ///     human-used identifiers for the same Occurrence,
        ///     whether in the current or any other data set or collection.
        /// </summary>
        public string OtherCatalogNumbers { get; set; }

        /// <summary>
        ///     A number or enumeration value for the quantity of organisms.
        ///     A dwc:organismQuantity must have a corresponding dwc:organismQuantityType.
        /// </summary>
        /// <example>
        ///     27 (organismQuantity) with individuals (organismQuantityType).
        ///     12.5 (organismQuantity) with %biomass (organismQuantityType).
        ///     r (organismQuantity) with BraunBlanquetScale (organismQuantityType).
        /// </example>
        public string OrganismQuantity { get; set; }

        /// <summary>
        ///     The quantity of organisms as integer. Necessary because we want to be able to do Range-querys against quantities. 
        /// </summary>
        /// <remarks>Not defined in DwC</remarks>
        public int? OrganismQuantityInt { get; set; }

        /// <summary>
        ///     The type of quantification system used for the quantity of organisms.
        ///     A dwc:organismQuantityType must have a corresponding dwc:organismQuantity.
        /// </summary>
        /// <example>
        ///     27 (organismQuantity) with individuals (organismQuantityType).
        ///     12.5 (organismQuantity) with %biomass (organismQuantityType).
        ///     r (organismQuantity) with BraunBlanquetScale (organismQuantityType).
        /// </example>
        /// <remarks>
        ///     This field uses a controlled vocabulary.
        /// </remarks>
        public VocabularyValue OrganismQuantityUnit { get; set; }

        /// <summary>
        ///     A list (concatenated and separated) of preparations
        ///     and preservation methods for a specimen.
        /// </summary>
        public string Preparations { get; set; }

        /// <summary>
        ///     A list (concatenated and separated) of previous
        ///     assignments of names to the Occurrence.
        /// </summary>
        public string PreviousIdentifications { get; set; }

        /// <summary>
        /// Information about how protected information about a species is in Sweden.
        /// This is a value between 1 to 5.
        /// 1 indicates public access and 5 is the highest used security level.
        /// </summary>
        public int ProtectionLevel { get; set; }

        /// <summary>
        ///     A list (concatenated and separated) of names of people,
        ///     groups, or organizations responsible for recording the
        ///     original Occurrence. The primary collector or observer,
        ///     especially one who applies a personal identifier
        ///     (recordNumber), should be listed first.
        /// </summary>
        public string RecordedBy { get; set; }
        
        /// <summary>
        ///     An identifier given to the Occurrence at the time it was
        ///     recorded. Often serves as a link between field notes and
        ///     an Occurrence record, such as a specimen collector's number.
        /// </summary>
        public string RecordNumber { get; set; }

        /// <summary>
        ///     Name of the person that reported the species observation.
        /// </summary>
        [Keyword]
        public string ReportedBy { get; set; }

        /// <summary>
        ///     Date and time when the species observation was reported (UTC).
        /// </summary>
        [Date]
        public DateTime? ReportedDate { get; set; }

        /// <summary>
        ///     The reproductive condition of the biological individual(s) represented in the Occurrence.
        /// </summary>
        /// <remarks>
        ///     This field uses a controlled vocabulary.
        /// </remarks>
        [Object]
        public VocabularyValue ReproductiveCondition { get; set; }

        /// <summary>
        ///     The sex of the biological individual(s) represented in
        ///     the Occurrence.
        ///     Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        /// <remarks>
        ///     This field uses a controlled vocabulary.
        /// </remarks>
        [Object]
        public VocabularyValue Sex { get; set; }

        /// <summary>
        /// Substrate.
        /// </summary>
        [Object]
        public Substrate Substrate { get; set; }

        /// <summary>
        ///     URL to occurrence.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        ///     The reported length.
        /// </summary>
        public int? Length { get; set; }

        /// <summary>
        ///     The reported weight.
        /// </summary>
        public int? Weight { get; set; }
    }
}