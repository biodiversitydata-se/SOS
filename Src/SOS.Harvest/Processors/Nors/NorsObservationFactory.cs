using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Nors;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Configuration.Process;

namespace SOS.Harvest.Processors.Nors
{
    public class NorsObservationFactory : ObservationFactoryBase, IObservationFactory<NorsObservationVerbatim>
    {
        private readonly IAreaHelper _areaHelper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="areaHelper"></param>
        /// <param name="processTimeManager"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public NorsObservationFactory(DataProvider dataProvider, 
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa, 
            IAreaHelper areaHelper,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration) : base(dataProvider, taxa, processTimeManager, processConfiguration)
        {
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <summary>
        /// Cast verbatim observations to processed data model
        /// </summary>
        /// <param name="verbatim"></param>
        /// <param name="diffuseIfSupported"></param>
        /// <returns></returns>
        public Observation CreateProcessedObservation(NorsObservationVerbatim verbatim, bool diffuseIfSupported)
        {
            var taxon = GetTaxon(verbatim.DyntaxaTaxonId);
            var obs = new Observation
            {                
                DataProviderId = DataProvider.Id,
                BasisOfRecord = new VocabularyValue { Id = (int)BasisOfRecordId.HumanObservation},
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.NORS}",
                DatasetName = "NORS",
                DiffusionStatus = DiffusionStatus.NotDiffused,
                Event = new Event(verbatim.Start, null, verbatim.End, null),
                Identification = new Identification
                {
                    UncertainIdentification = false,
                    Verified = false,
                    VerificationStatus = new VocabularyValue { Id = (int)ValidationStatusId.ReportedByExpert }
                },
                Location = new Location(LocationType.Point)
                {
                    Locality = verbatim.Locality,
                    VerbatimLocality = verbatim.Locality
                },
                Modified = verbatim.Modified?.ToUniversalTime(),
                Occurrence = new Occurrence
                {
                    BirdNestActivityId = 0,
                    CatalogNumber = GetCatalogNumber(verbatim.OccurrenceId),
                    OccurrenceId = verbatim.OccurrenceId,
                    IsNaturalOccurrence = true,
                    IsNeverFoundObservation = GetIsNeverFoundObservation(verbatim.DyntaxaTaxonId),
                    IsNotRediscoveredObservation = false,
                    IsPositiveObservation = GetIsPositiveObservation(verbatim.DyntaxaTaxonId),
                    RecordedBy = verbatim.RecordedBy,
                    //ProtectionLevel = CalculateProtectionLevel(taxon),
                    SensitivityCategory = CalculateProtectionLevel(taxon),
                    ReportedBy = verbatim.ReportedBy,
                    ReportedDate = verbatim.Start.ToUniversalTime(),
                    OccurrenceStatus = GetOccurrenceStatusId(verbatim.DyntaxaTaxonId)
                },
                OwnerInstitutionCode = verbatim.Owner,
                Taxon = taxon
            };

            if (int.TryParse(obs.Occurrence.OrganismQuantity, out var quantity))
            {
                obs.Occurrence.OrganismQuantityAggregation = quantity;
                obs.Occurrence.OrganismQuantityInt = quantity;
            }
            obs.AccessRights = GetAccessRightsFromSensitivityCategory(obs.Occurrence.SensitivityCategory);
            AddPositionData(obs.Location, verbatim.DecimalLongitude, verbatim.DecimalLatitude,
                CoordinateSys.WGS84, verbatim.CoordinateUncertaintyInMeters, taxon?.Attributes?.DisturbanceRadius);
            _areaHelper.AddAreaDataToProcessedLocation(obs.Location);

            // Populate generic data
            PopulateGenericData(obs);

            return obs;
        }

        /// <summary>
        ///     Creates occurrence id.
        /// </summary>
        /// <returns>The Catalog Number.</returns>
        private string GetCatalogNumber(string occurrenceId)
        {
            var pos = occurrenceId?.LastIndexOf(":", StringComparison.Ordinal) ?? -1;
            return pos == -1 ? null : occurrenceId?.Substring(pos + 1);
        }

        /// <summary>
        ///     Gets the occurrence status. Set to Present if DyntaxaTaxonId from provider is greater than 0 and Absent if
        ///     DyntaxaTaxonId is 0
        /// </summary>
        private VocabularyValue GetOccurrenceStatusId(int dyntaxaTaxonId)
        {
            if (dyntaxaTaxonId == 0)
            {
                return new VocabularyValue {Id = (int) OccurrenceStatusId.Absent};
            }

            return new VocabularyValue {Id = (int) OccurrenceStatusId.Present};
        }

        /// <summary>
        ///     Set to False if DyntaxaTaxonId from provider is greater than 0 and True if DyntaxaTaxonId is 0.
        /// </summary>
        private bool GetIsNeverFoundObservation(int dyntaxaTaxonId)
        {
            return dyntaxaTaxonId == 0;
        }

        /// <summary>
        ///     Set to True if DyntaxaTaxonId from provider is greater than 0 and False if DyntaxaTaxonId is 0.
        /// </summary>
        private bool GetIsPositiveObservation(int dyntaxaTaxonId)
        {
            return dyntaxaTaxonId != 0;
        }
    }
}