namespace SOS.Lib.Models.DarwinCore
{
    /// <summary>
    ///     This class contains occurrence information about a species
    ///     observation in Darwin Core 1.5 compatible format.
    ///     Further information about the properties can
    ///     be found at http://rs.tdwg.org/dwc/terms/.
    /// </summary>
    public class DarwinCoreOccurrence
    {
        /// <summary>
        ///     Darwin Core term name: associatedMedia.
        ///     A list (concatenated and separated) of identifiers
        ///     (publication, global unique identifier, URI) of
        ///     media associated with the Occurrence.
        /// </summary>
        public string AssociatedMedia { get; set; }

        /// <summary>
        ///     Darwin Core term name: associatedOccurrences.
        ///     A list (concatenated and separated) of identifiers of
        ///     other Occurrence records and their associations to
        ///     this Occurrence.
        /// </summary>
        public string AssociatedOccurrences { get; set; }

        /// <summary>
        ///     Darwin Core term name: associatedReferences.
        ///     A list (concatenated and separated) of identifiers
        ///     (publication, bibliographic reference, global unique
        ///     identifier, URI) of literature associated with
        ///     the Occurrence.
        /// </summary>
        public string AssociatedReferences { get; set; }

        /// <summary>
        ///     Darwin Core term name: associatedSequences.
        ///     A list (concatenated and separated) of identifiers of
        ///     other Occurrence records and their associations to
        ///     this Occurrence.
        /// </summary>
        public string AssociatedSequences { get; set; }

        /// <summary>
        ///     Darwin Core term name: associatedTaxa.
        ///     A list (concatenated and separated) of identifiers or
        ///     names of taxa and their associations with the Occurrence.
        /// </summary>
        public string AssociatedTaxa { get; set; }

        /// <summary>
        ///     Darwin Core term name: behavior.
        ///     A description of the behavior shown by the subject at
        ///     the time the Occurrence was recorded.
        ///     Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        public string Behavior { get; set; }

        /// <summary>
        ///     Darwin Core term name: catalogNumber.
        ///     An identifier (preferably unique) for the record
        ///     within the data set or collection.
        /// </summary>
        public string CatalogNumber { get; set; }

        /// <summary>
        ///     Darwin Core term name: disposition.
        ///     The current state of a specimen with respect to the
        ///     collection identified in collectionCode or collectionID.
        ///     Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        public string Disposition { get; set; }

        /// <summary>
        ///     Darwin Core term name: establishmentMeans.
        ///     The process by which the biological individual(s)
        ///     represented in the Occurrence became established at the
        ///     location.
        ///     Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        public string EstablishmentMeans { get; set; }

        /// <summary>
        ///     Darwin Core term name: individualCount.
        ///     The number of individuals represented present
        ///     at the time of the Occurrence.
        /// </summary>
        public string IndividualCount { get; set; }

        /// <summary>
        ///     Darwin Core term name: individualID.
        ///     An identifier for an individual or named group of
        ///     individual organisms represented in the Occurrence.
        ///     Meant to accommodate resampling of the same individual
        ///     or group for monitoring purposes. May be a global unique
        ///     identifier or an identifier specific to a data set.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        //public string IndividualID { get; set; }

        /// <summary>
        ///     Darwin Core term name: lifeStage.
        ///     The age class or life stage of the biological individual(s)
        ///     at the time the Occurrence was recorded.
        ///     Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        public string LifeStage { get; set; }

        /// <summary>
        ///     Darwin Core term name: occurrenceID.
        ///     An identifier for the Occurrence (as opposed to a
        ///     particular digital record of the occurrence).
        ///     In the absence of a persistent global unique identifier,
        ///     construct one from a combination of identifiers in
        ///     the record that will most closely make the occurrenceID
        ///     globally unique.
        ///     The format LSID (Life Science Identifiers) is used as GUID
        ///     (Globally unique identifier) for species observations.
        ///     Currently known GUIDs:
        ///     Species Gateway (Artportalen) 1,
        ///     urn:lsid:artportalen.se:Sighting:{reporting system}.{id}
        ///     where {reporting system} is one of Bird, Bugs, Fish,
        ///     MarineInvertebrates, PlantAndMushroom or Vertebrate.
        ///     Species Gateway (Artportalen) 2,
        ///     urn:lsid:artportalen.se:Sighting:{id}
        ///     Red list database: urn:lsid:artdata.slu.se:SpeciesObservation:{id}
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string OccurrenceID { get; set; }

        /// <summary>
        ///     Darwin Core term name: occurrenceRemarks.
        ///     Comments or notes about the Occurrence.
        /// </summary>
        public string OccurrenceRemarks { get; set; }

        /// <summary>
        ///     Darwin Core term name: occurrenceStatus.
        ///     A statement about the presence or absence of a Taxon at a
        ///     Location.
        ///     Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        public string OccurrenceStatus { get; set; }

        /// <summary>
        ///     Darwin Core term name: otherCatalogNumbers.
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
        ///     The type of quantification system used for the quantity of organisms.
        ///     A dwc:organismQuantityType must have a corresponding dwc:organismQuantity.
        /// </summary>
        /// <example>
        ///     27 (organismQuantity) with individuals (organismQuantityType).
        ///     12.5 (organismQuantity) with %biomass (organismQuantityType).
        ///     r (organismQuantity) with BraunBlanquetScale (organismQuantityType).
        /// </example>
        public string OrganismQuantityType { get; set; }

        /// <summary>
        ///     Darwin Core term name: preparations.
        ///     A list (concatenated and separated) of preparations
        ///     and preservation methods for a specimen.
        /// </summary>
        public string Preparations { get; set; }

        /// <summary>
        ///     Darwin Core term name: previousIdentifications.
        ///     A list (concatenated and separated) of previous
        ///     assignments of names to the Occurrence.
        /// </summary>
        public string PreviousIdentifications { get; set; }

        /// <summary>
        ///     Darwin Core term name: recordedBy.
        ///     A list (concatenated and separated) of names of people,
        ///     groups, or organizations responsible for recording the
        ///     original Occurrence. The primary collector or observer,
        ///     especially one who applies a personal identifier
        ///     (recordNumber), should be listed first.
        /// </summary>
        public string RecordedBy { get; set; }

        /// <summary>
        ///     Darwin Core term name: recordNumber.
        ///     An identifier given to the Occurrence at the time it was
        ///     recorded. Often serves as a link between field notes and
        ///     an Occurrence record, such as a specimen collector's number.
        /// </summary>
        public string RecordNumber { get; set; }

        /// <summary>
        ///     Darwin Core term name: reproductiveCondition.
        ///     The reproductive condition of the biological individual(s)
        ///     represented in the Occurrence.
        ///     Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        public string ReproductiveCondition { get; set; }

        /// <summary>
        ///     Darwin Core term name: sex.
        ///     The sex of the biological individual(s) represented in
        ///     the Occurrence.
        ///     Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        public string Sex { get; set; }
    }
}