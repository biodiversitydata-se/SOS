using System;
using System.Collections.Generic;
using SOS.Lib.Models.Shared;
using SOS.Lib.Swagger;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    ///     Observation information specific for Artportalen.
    /// </summary>
    public class ArtportalenInternal
    {
        /// <summary>
        /// Bird validation areas.
        /// </summary>
        public IEnumerable<string> BirdValidationAreaIds { get; set; }

        /// <summary>
        /// Id of checklist
        /// </summary>
        public int? ChecklistId { get; set; }

        /// <summary>
        /// Year of confirmation.
        /// </summary>
        public int? ConfirmationYear { get; set; }

        /// <summary>
        /// Data source id
        /// </summary>
        public int? DatasourceId { get; set; }

        /// <summary>
        /// Year of determination.
        /// </summary>
        public int? DeterminationYear { get; set; }

        /// <summary>
        /// Field diary group id
        /// </summary>
        public int? FieldDiaryGroupId { get; set; }

        /// <summary>
        ///     Has Triggered Validation Rules
        /// </summary>
        [Obsolete("Replaced by HasTriggeredVerificationRules")]
        public bool HasTriggeredValidationRules { get; set; }

        /// <summary>
        ///     Has Triggered Verification Rules
        /// </summary>
        public bool HasTriggeredVerificationRules { get; set; }

        /// <summary>
        ///     Has any Triggered Validation Rule with Warning
        /// </summary>
        [Obsolete("Replaced by HasAnyTriggeredVerificationRuleWithWarning")]
        public bool HasAnyTriggeredValidationRuleWithWarning { get; set; }

        /// <summary>
        ///     Has any Triggered Verification Rule with Warning
        /// </summary>
        public bool HasAnyTriggeredVerificationRuleWithWarning { get; set; }

        /// <summary>
        ///     HasUserComments
        /// </summary>
        public bool HasUserComments { get; set; }

        /// <summary>
        ///     ExternalId of Site in Artportalen.
        /// </summary>
        [Obsolete("Use Location.Attributes.ExternalId")]
        public string LocationExternalId { get; set; }

        /// <summary>
        ///     Note of Interest.
        /// </summary>
        public bool NoteOfInterest { get; set; }

        /// <summary>
        /// Sighting Id.
        /// </summary>
        public int SightingId { get; set; }

        /// <summary>
        ///     Id of SightingSpeciesCollectionItem in Artportalen.
        /// </summary>
        public int? SightingSpeciesCollectionItemId { get; set; }

        /// <summary>
        ///     Sighting type.
        /// </summary>
        public int SightingTypeId { get; set; }

        /// <summary>
        ///     Sighting type search group id.
        /// </summary>
        public int SightingTypeSearchGroupId { get; set; }

        /// <summary>
        ///     Ids of Species Facts connected to Taxon
        /// </summary>
        public IEnumerable<int> SpeciesFactsIds { get; set; }

        /// <summary>
        /// Species group id.
        /// </summary>
        public int? SpeciesGroupId { get; set; }

        /// <summary>
        ///     Id of sightings RegionalSightingState
        /// </summary>
        [Obsolete("This is too be deleted")]
        public int? RegionalSightingStateId { get; set; }

        /// <summary>
        ///     Id of publishing types.
        /// </summary>
        public IEnumerable<int> SightingPublishTypeIds { get; set; }
        
        /// <summary>
        ///     Internal field used for searches by Artportalen, contains extra user information.
        /// </summary>
        public IEnumerable<UserInternal> OccurrenceRecordedByInternal { get; set; }

        /// <summary>
        /// Info about users verifying the observation
        /// </summary>
        public IEnumerable<UserInternal> OccurrenceVerifiedByInternal { get; set; }

        /// <summary>
        ///     The original presentation name for ParishRegion from data provider.
        /// </summary>
        public string LocationPresentationNameParishRegion { get; set; }

        /// <summary>
        ///     The parent location id of the current location, this is used by Artportalen for bird locations that
        ///     have one main location and several sublocation.
        /// </summary>
        [SwaggerExclude]
        public int? ParentLocationId { get; set; }

        /// <summary>
        ///     Name of parent location, if any.
        /// </summary>
        public string ParentLocality { get; set; }

        /// <summary>
        ///     User id of the person that reported the species observation.
        /// </summary>
        public int? ReportedByUserId { get; set; }

        /// <summary>
        ///     User Service id of the person that reported the species observation.
        /// </summary>
        [SwaggerExclude]
        public int? ReportedByUserServiceUserId { get; set; }
        
        /// <summary>
        ///     Alias for the reporter, internal use only.
        /// </summary>
        [SwaggerExclude]
        public string ReportedByUserAlias { get; set; }

        /// <summary>
        /// True if sighting was incremental harvested.
        /// </summary>
        public bool IncrementalHarvested { get; set; }

        /// <summary>
        ///  Sighting barcode url
        /// </summary>
        public string SightingBarcodeURL { get; set; }

        /// <summary>
        /// Second hand information flag
        /// </summary>
        public bool SecondHandInformation { get; set; }

        /// <summary>
        /// Triggered observation rule frequency id
        /// </summary>
        public int? TriggeredObservationRuleFrequencyId { get; set; }

        /// <summary>
        /// Triggered observation rule reproduction id
        /// </summary>
        public int? TriggeredObservationRuleReproductionId { get; set; }
    }
}