namespace SOS.Lib.Enums.Artportalen
{
    /// <summary>
    /// All possible values in table SightingType
    /// </summary>
    public enum SightingType
    {
        /// <summary>
        /// Normal sighting.
        /// </summary>
        NormalSighting = 0,

        /// <summary>
        /// Aggregation sighting. Used e.g. to collect a number of bird sightings into a new family sighting that replaces the others.
        /// </summary>
        AggregationSighting = 1,

        /// <summary>
        /// Assessment sighting. Used for evaluating a number of sightings.
        /// </summary>
        AssessmentSighting = 2,

        /// <summary>
        /// Replacement sighting. Used when creating a new sighting to replace a number of other sightings.
        /// </summary>
        ReplacementSighting = 3,

        /// <summary>
        /// Special case of a Normal sighting that is mapped to class NonvalidatingSighting. Used during import to save a partially validating sighting.
        /// </summary>
        SpecialNormalSighting = 4,

        /// <summary>
        /// Assessment sighting for breeding. Used for evaluating a number of sightings.
        /// </summary>
        AssessmentSightingForBreeding = 5,

        /// <summary>
        /// Assessment sighting for max count. Used for evaluating a number of sightings.
        /// </summary>
        AssessmentSightingForMaxCount = 6,

        /// <summary>
        /// Assessment sighting different sites. Used for evaluating a number of sightings.
        /// </summary>
        AssessmentSightingForDifferentSites = 7,

        /// <summary>
        /// Correction sighting. Used for correcting a sighting.
        /// </summary>
        CorrectionSighting = 8,

        /// <summary>
        /// Assessment sighting for herd. Used for evaluating a number of sightings.
        /// </summary>
        AssessmentSightingForHerd = 9,

        /// <summary>
        /// Assessment sighting for own breeding sighting. Used for evaluating a number of sightings.
        /// </summary>
        AssessmentSightingForOwnBreeding = 10
    }
}