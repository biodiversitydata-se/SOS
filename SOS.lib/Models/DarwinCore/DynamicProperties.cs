using System;

namespace SOS.Lib.Models.DarwinCore
{
    /// <summary>
    /// This class contains fields not defined in Darwin Core.
    /// </summary>
    public class DynamicProperties
    {
        /// <summary>
        /// Not defined in Darwin Core.
        /// The Data provider Id.
        /// </summary>
        public int DataProviderId { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Information about date and time when the
        /// species observation started.
        /// </summary>
        public DateTime ObservationDateStart { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Information about date and time when the
        /// species observation ended.
        /// </summary>
        public DateTime? ObservationDateEnd { get; set; }

        /// <summary>
        ///
        /// Conservation related information about the taxon that
        /// the species observation is attached to.
        /// </summary>
        public DarwinCoreConservation Conservation { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// M value that is part of a linear reference system.
        /// The properties CoordinateX, CoordinateY, CoordinateZ,
        /// CoordinateM and CoordinateSystemWkt defines where the
        /// species observation was made.
        /// </summary>
        public string CoordinateM { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Coordinate system wkt (Well-known text)
        /// as defined by OGC (Open Geospatial Consortium).
        /// The properties CoordinateX, CoordinateY, CoordinateZ,
        /// CoordinateM and CoordinateSystemWkt defines where the
        /// species observation was made.
        /// </summary>
        public string CoordinateSystemWkt { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// East-west value of the coordinate.
        /// The properties CoordinateX, CoordinateY, CoordinateZ,
        /// CoordinateM and CoordinateSystemWkt defines where the
        /// species observation was made.
        /// Which values that are valid depends on which
        /// coordinate system that is used.
        /// </summary>
        public double CoordinateX { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// North-south value of the coordinate.
        /// The properties CoordinateX, CoordinateY, CoordinateZ,
        /// CoordinateM and CoordinateSystemWkt defines where the
        /// species observation was made.
        /// Which values that are valid depends on which
        /// coordinate system that is used.
        /// </summary>
        public double CoordinateY { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Altitude value of the coordinate.
        /// The properties CoordinateX, CoordinateY, CoordinateZ,
        /// CoordinateM and CoordinateSystemWkt defines where the
        /// species observation was made.
        /// </summary>
        public string CoordinateZ { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Taxon id (not GUID) value in Dyntaxa.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public int? DyntaxaTaxonID { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Information about date and time when the
        /// species observation ended.
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// SwedishSpeciesObservationService specific id
        /// for this species observation.
        /// The id is only used in communication with
        /// SwedishSpeciesObservationService and has no 
        /// meaning in other contexts.
        /// This id is currently not stable.
        /// The same observation may have another id tomorrow.
        /// In the future this id should be stable.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Id of individual
        /// </summary>
        public string IndividualID { get; set; }
        
        /// <summary>
        /// Not defined in Darwin Core.
        /// Indicates if this species occurrence is natural or
        /// if it is a result of human activity.
        /// </summary>
        public bool? IsNaturalOccurrence { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Indicates if this observation is a never found observation.
        /// "Never found observation" is an observation that says
        /// that the specified species was not found in a location
        /// deemed appropriate for the species.
        /// </summary>
        public bool? IsNeverFoundObservation { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Indicates if this observation is a 
        /// not rediscovered observation.
        /// "Not rediscovered observation" is an observation that says
        /// that the specified species was not found in a location
        /// where it has previously been observed.
        /// </summary>
        public bool? IsNotRediscoveredObservation { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Indicates if this observation is a positive observation.
        /// "Positive observation" is a normal observation indicating
        /// that a species has been seen at a specified location.
        /// </summary>
        public bool? IsPositiveObservation { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Web address that leads to more information about the
        /// location. The information should be accessible
        /// from the most commonly used web browsers.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string LocationURL { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Web address that leads to more information about the
        /// occurrence. The information should be accessible
        /// from the most commonly used web browsers.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string OccurrenceURL { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Common name of the organism group that observed species
        /// belongs to. Classification of species groups is the same as
        /// used in latest 'Red List of Swedish Species'.
        /// </summary>
        public string OrganismGroup { get; set; }

        /// <summary>
        /// Owner of the sighting
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Parish where the species observation where made.
        /// 'Socken/församling' in swedish.
        /// </summary>
        public string Parish { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Information about the project in which this
        /// species observation was made.
        /// </summary>
        public DarwinCoreProject Project { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Quantity of observed species, for example distribution area.
        /// Unit is specified in property QuantityUnit.
        /// </summary>
        public string Quantity { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Unit for quantity value of observed species.
        /// </summary>
        public string QuantityUnit { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Name of the person that reported the species observation.
        /// </summary>
        public string ReportedBy { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Date and time when the species observation was reported.
        /// </summary>
        public DateTime? ReportedDate { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Web address that leads to more information about the
        /// species observation. The information should be accessible
        /// from the most commonly used web browsers.
        /// </summary>
        public string SpeciesObservationURL { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Information about date and time when the
        /// species observation started.
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Substrate on which the species was observed.
        /// </summary>
        public string Substrate { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Status of the taxon concept.
        /// Examples of possible values are InvalidDueToSplit,
        /// InvalidDueToLump, InvalidDueToDelete, Unchanged,
        /// ValidAfterLump or ValidAfterSplit.
        /// </summary>
        public string TaxonConceptStatus { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Sort order of taxon according to Dyntaxa.
        /// This property is currently not used.
        /// </summary>
        public int TaxonSortOrder { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Web address that leads to more information about the
        /// taxon. The information should be accessible
        /// from the most commonly used web browsers.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string TaxonURL { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Indicates if the species observer himself is
        /// uncertain about the taxon determination.
        /// </summary>
        public bool UncertainDetermination { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// Information about current validation status
        /// for the species observation.
        /// </summary>
        public string ValidationStatus { get; set; }

        /// <summary>
        /// Not defined in Darwin Core.
        /// ScientificName as it appears in the original record.
        /// This property is currently not used.
        /// </summary>
        public string VerbatimScientificName { get; set; }
        
        public int? ActivityId { get; set; }
        public int? BirdNestActivityId { get; set; }
        public int DisturbanceRadius { get; set; }
        public int MaxAccuracyOrDisturbanceRadius { get; set; }
        public int? CountyIdByName { get; set; }
        public int? CountyPartIdByName { get; set; }
        public int? ProvinceIdByName { get; set; }
        public int? ProvincePartIdByName { get; set; }
        public int? CountyIdByCoordinate { get; set; }
        public int? CountyPartIdByCoordinate { get; set; }
        public int? ProvinceIdByCoordinate { get; set; }
        public int? ProvincePartIdByCoordinate { get; set; }
        public int? MunicipalityIdByCoordinate { get; set; }
        public int? ParishIdByCoordinate { get; set; }
        
        /// <summary>
        /// Not defined in Darwin Core.
        /// Name of the organization or person that
        /// owns the species observation.
        /// </summary>
        public string Owner { get; set; }
    }
}
