using System;
using System.Collections.Generic;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Models.Verbatim.Artportalen
{
    /// <summary>
    ///     Sighting object
    /// </summary>
    public class ArtportalenObservationVerbatim : IEntity<int>
    {
        /// <summary>
        ///     Id of activity
        /// </summary>
        public MetadataWithCategory Activity { get; set; }

        /// <summary>
        ///     Biotope
        /// </summary>
        public Metadata Biotope { get; set; }

        /// <summary>
        ///     Description of bioptpe
        /// </summary>
        public string BiotopeDescription { get; set; }

        /// <summary>
        ///     Id of collection
        /// </summary>
        public string CollectionID { get; set; }

        /// <summary>
        ///     SightingCommentPublic comment
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        ///     DiscoveryMethod
        /// </summary>
        public Metadata DiscoveryMethod { get; set; }

        /// <summary>
        ///     DeterminationMethod
        /// </summary>
        public Metadata DeterminationMethod { get; set; }

        /// <summary>
        /// Sighting Edit date
        /// </summary>
        public DateTime EditDate { get; set; }

        /// <summary>
        ///     Sigthing end data
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        ///     Sighting end time
        /// </summary>
        public TimeSpan? EndTime { get; set; }

        /// <summary>
        ///     Taxon gender id
        /// </summary>
        public Metadata Gender { get; set; }

        /// <summary>
        ///     Has sighting images
        /// </summary>
        public bool HasImages { get; set; }

        /// <summary>
        ///     Has Triggered Validation Rules
        /// </summary>
        public bool HasTriggeredValidationRules { get; set; }

        /// <summary>
        ///     Has any Triggered Validation Rule with Warning
        /// </summary>
        public bool HasAnyTriggeredValidationRuleWithWarning { get; set; }


        /// <summary>
        ///     Hidden by provider date
        /// </summary>
        public DateTime? HiddenByProvider { get; set; }

        /// <summary>
        ///     SightingSpeciesCollectionItem label
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        ///     Taxon length
        /// </summary>
        public int? Length { get; set; }

        /// <summary>
        ///     Max depth
        /// </summary>
        public int? MaxDepth { get; set; }

        /// <summary>
        ///     Max height
        /// </summary>
        public int? MaxHeight { get; set; }

        /// <summary>
        ///     Migrate obs id
        /// </summary>
        public int? MigrateSightingObsId { get; set; }

        /// <summary>
        ///     Migrate Portal id
        /// </summary>
        public int? MigrateSightingPortalId { get; set; }

        /// <summary>
        ///     Min depth
        /// </summary>
        public int? MinDepth { get; set; }

        /// <summary>
        ///     Min height
        /// </summary>
        public int? MinHeight { get; set; }

        /// <summary>
        ///     Note of Interest
        /// </summary>
        public bool NoteOfInterest { get; set; }

        /// <summary>
        ///    HasUserComments
        /// </summary>
        public bool HasUserComments { get; set; }

        /// <summary>
        ///     Not present flag
        /// </summary>
        public bool NotPresent { get; set; }

        /// <summary>
        ///     Not recovered flag
        /// </summary>
        public bool NotRecovered { get; set; }

        /// <summary>
        ///     Owner organization
        /// </summary>
        public Metadata OwnerOrganization { get; set; }

        /// <summary>
        ///     Protected by system flag
        /// </summary>
        public bool ProtectedBySystem { get; set; }

        /// <summary>
        ///     Projects
        /// </summary>
        public IEnumerable<Project> Projects { get; set; }

        /// <summary>
        ///     Number of taxa found
        /// </summary>
        public int? Quantity { get; set; }

        /// <summary>
        ///     Quality of substrate
        /// </summary>
        public int? QuantityOfSubstrate { get; set; }

        /// <summary>
        ///     Date sighting was added
        /// </summary>
        public DateTime? ReportedDate { get; set; }

        /// <summary>
        ///     Rights holder
        /// </summary>
        public string RightsHolder { get; set; }

        /// <summary>
        ///     Id of site
        /// </summary>
        public Site Site { get; set; }

        /// <summary>
        ///     Id of SightingSpeciesCollectionItem
        /// </summary>
        public int? SightingSpeciesCollectionItemId { get; set; }

        /// <summary>
        ///     Taxon stage id
        /// </summary>
        public Metadata Stage { get; set; }

        /// <summary>
        ///     Sighting start date
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        ///     Sighting start time
        /// </summary>
        public TimeSpan? StartTime { get; set; }

        /// <summary>
        ///     Substrate
        /// </summary>
        public Metadata Substrate { get; set; }

        /// <summary>
        ///     Description of substrate
        /// </summary>
        public string SubstrateDescription { get; set; }

        /// <summary>
        ///     Description of substrate species
        /// </summary>
        public string SubstrateSpeciesDescription { get; set; }

        /// <summary>
        ///     Substrate taxon id
        /// </summary>
        public int? SubstrateSpeciesId { get; set; }

        /// <summary>
        ///     Taxon Id
        /// </summary>
        public int? TaxonId { get; set; }

        /// <summary>
        ///     Id of unit
        /// </summary>
        public Metadata Unit { get; set; }

        /// <summary>
        ///     Un spontaneous flag
        /// </summary>
        public bool Unspontaneous { get; set; }

        /// <summary>
        ///     Unsecure determination
        /// </summary>
        public bool UnsureDetermination { get; set; }

        /// <summary>
        ///     Taxon weight
        /// </summary>
        public int? Weight { get; set; }

        /// <summary>
        ///     SightingBarcode url
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        ///     Validation status
        /// </summary>
        public Metadata ValidationStatus { get; set; }

        public string VerifiedBy { get; set; }
        public IEnumerable<UserInternal> VerifiedByInternal { get; set; }
        public string Observers { get; set; }
        public IEnumerable<UserInternal> ObserversInternal { get; set; }
        public string ReportedBy { get; set; }
        public int ReportedByUserId { get; set; }
        public string ReportedByUserAlias { get; set; }
        public string SpeciesCollection { get; set; }

        public Metadata PublicCollection { get; set; }
        public string PrivateCollection { get; set; }


        public string DeterminedBy { get; set; }
        public int? DeterminationYear { get; set; }
        public string ConfirmedBy { get; set; }
        public int? ConfirmationYear { get; set; }

        /// <summary>
        ///     Incremented number
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Sighting id
        /// </summary>
        public int SightingId { get; set; }

        public int SightingTypeSearchGroupId { get; set; }
        public int SightingTypeId { get; set; }

        public int? RegionalSightingStateId { get; set; }
        public IEnumerable<int> SightingPublishTypeIds { get; set; }

        public IEnumerable<int> SpeciesFactsIds { get; set; }
        public int FirstImageId { get; set; }
    }
}