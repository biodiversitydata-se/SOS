using System;
using System.Collections.Generic;

namespace SOS.Lib.Models.DarwinCore
{
    /// <summary>
    ///     Dynamic properties.
    /// </summary>
    public class DynamicProperties
    {
        /// <summary>
        ///     The Data provider Id.
        /// </summary>
        public int DataProviderId { get; set; }

        /// <summary>
        ///     Information about date and time when the
        ///     species observation started.
        /// </summary>
        public DateTime ObservationDateStart { get; set; }

        /// <summary>
        ///     Information about date and time when the
        ///     species observation ended.
        /// </summary>
        public DateTime? ObservationDateEnd { get; set; }

        /// <summary>
        ///     Conservation related information about the taxon that
        ///     the species observation is attached to.
        /// </summary>
        public DarwinCoreConservation Conservation { get; set; }

        /// <summary>
        ///     M value that is part of a linear reference system.
        ///     The properties CoordinateX, CoordinateY, CoordinateZ,
        ///     CoordinateM and CoordinateSystemWkt defines where the
        ///     species observation was made.
        /// </summary>
        public string CoordinateM { get; set; }

        /// <summary>
        ///     Coordinate system wkt (Well-known text)
        ///     as defined by OGC (Open Geospatial Consortium).
        ///     The properties CoordinateX, CoordinateY, CoordinateZ,
        ///     CoordinateM and CoordinateSystemWkt defines where the
        ///     species observation was made.
        /// </summary>
        public string CoordinateSystemWkt { get; set; }

        /// <summary>
        ///     East-west value of the coordinate.
        ///     The properties CoordinateX, CoordinateY, CoordinateZ,
        ///     CoordinateM and CoordinateSystemWkt defines where the
        ///     species observation was made.
        ///     Which values that are valid depends on which
        ///     coordinate system that is used.
        /// </summary>
        public double CoordinateX { get; set; }

        /// <summary>
        ///     North-south value of the coordinate.
        ///     The properties CoordinateX, CoordinateY, CoordinateZ,
        ///     CoordinateM and CoordinateSystemWkt defines where the
        ///     species observation was made.
        ///     Which values that are valid depends on which
        ///     coordinate system that is used.
        /// </summary>
        public double CoordinateY { get; set; }

        /// <summary>
        ///     Altitude value of the coordinate.
        ///     The properties CoordinateX, CoordinateY, CoordinateZ,
        ///     CoordinateM and CoordinateSystemWkt defines where the
        ///     species observation was made.
        /// </summary>
        public string CoordinateZ { get; set; }

        /// <summary>
        ///     Taxon id (not GUID) value in Dyntaxa.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public int? DyntaxaTaxonID { get; set; }

        /// <summary>
        ///     Information about date and time when the
        ///     species observation ended.
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        ///     SwedishSpeciesObservationService specific id
        ///     for this species observation.
        ///     The id is only used in communication with
        ///     SwedishSpeciesObservationService and has no
        ///     meaning in other contexts.
        ///     This id is currently not stable.
        ///     The same observation may have another id tomorrow.
        ///     In the future this id should be stable.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        ///     Id of individual
        /// </summary>
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
        ///     Web address that leads to more information about the
        ///     occurrence. The information should be accessible
        ///     from the most commonly used web browsers.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public string OccurrenceURL { get; set; }

        /// <summary>
        ///     Name of the organization or person that
        ///     owns the species observation.
        /// </summary>
        public string Owner { get; set; }

        /// <summary>
        ///     Parish where the species observation where made.
        ///     'Socken/församling' in swedish.
        /// </summary>
        public string Parish { get; set; }

        /// <summary>
        ///     Information about the projects in which this
        ///     species observation was made.
        /// </summary>
        public IEnumerable<DarwinCoreProject> Projects { get; set; }

        /// <summary>
        ///     Protection level
        /// </summary>
        public int ProtectionLevel { get; set; }

        /// <summary>
        ///     Name of the person that reported the species observation.
        /// </summary>
        public string ReportedBy { get; set; }

        /// <summary>
        ///     Date and time when the species observation was reported.
        /// </summary>
        public DateTime? ReportedDate { get; set; }

        /// <summary>
        ///     Information about date and time when the
        ///     species observation started.
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        ///     Substrate on which the species was observed.
        /// </summary>
        public string Substrate { get; set; }

        /// <summary>
        ///     Indicates if the species observer himself is
        ///     uncertain about the taxon determination.
        /// </summary>
        public bool UncertainDetermination { get; set; }

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
    }
}