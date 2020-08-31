using System.Collections.Generic;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    ///     This class contains information specific for Artportalen
    /// </summary>
    public class ArtportalenInternal
    {
        /// <summary>
        ///     Ids of Species Facts connected to Taxon
        /// </summary>
        public IEnumerable<int> SpeciesFactsIds { get; set; }

        /// <summary>
        ///     Id of SightingSpeciesCollectionItem in Artportalen.
        /// </summary>
        public int? SightingSpeciesCollectionItemId { get; set; }

        /// <summary>
        ///     Private Collection
        /// </summary>
        public string PrivateCollection { get; set; }

        /// <summary>
        ///     Has Triggered Validation Rules
        /// </summary>
        public bool HasTriggeredValidationRules { get; set; }

        /// <summary>
        ///     Has any Triggered Validation Rule with Warning
        /// </summary>
        public bool HasAnyTriggeredValidationRuleWithWarning { get; set; }

        /// <summary>
        ///     Internal field: ExternalId of Site in Artportalen.
        /// </summary>
        public string LocationExternalId { get; set; }

        /// <summary>
        ///     Note of Interest
        /// </summary>
        public bool NoteOfInterest { get; set; }

        /// <summary>
        ///     Sighting type
        /// </summary>
        public int SightingTypeId { get; set; }

        /// <summary>
        ///     Sighting type search group id
        /// </summary>
        public int SightingTypeSearchGroupId { get; set; }

        /// <summary>
        ///     Id of sightings RegionalSightingState
        /// </summary>
        public int? RegionalSightingStateId { get; set; }

        /// <summary>
        ///     Id of publishing types.
        /// </summary>
        public IEnumerable<int> SightingPublishTypeIds { get; set; }
        
        /// <summary>
        ///     Internal field used for searches by Artportalen, contains extra user information
        /// </summary>
        public IEnumerable<UserInternal> OccurrenceRecordedByInternal { get; set; }

        /// <summary>
        ///     The original presentation name for ParisRegion from data provider
        /// </summary>
        public string LocationPresentationNameParishRegion { get; set; }

        /// <summary>
        ///     User id of the person that reported the species observation.
        /// </summary>
        public int? ReportedByUserId { get; set; }
    }
}