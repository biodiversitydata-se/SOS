using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.ObservationDatabase;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;

namespace SOS.Harvest.Processors.ObservationDatabase
{
    /// <summary>
    /// Observation database factory
    /// </summary>
    public class ObservationDatabaseObservationFactory : ObservationFactoryBase, IObservationFactory<ObservationDatabaseVerbatim>
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
        public ObservationDatabaseObservationFactory(DataProvider dataProvider, 
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa, 
            IAreaHelper areaHelper,
            IProcessTimeManager processTimeManager) : base(dataProvider, taxa, processTimeManager)
        {
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <summary>
        /// Cast verbatim observations to processed data model
        /// </summary>
        /// <param name="verbatim"></param>
        /// <param name="diffuseIfSupported"></param>
        /// <returns></returns>
        public Observation CreateProcessedObservation(ObservationDatabaseVerbatim verbatim, bool diffuseIfSupported)
        {
            var taxon = GetTaxon(verbatim.TaxonId);

            var obs = new Observation
            {
                AccessRights = new VocabularyValue { Id = (int)AccessRightsId.NotForPublicUsage },
                DataProviderId = DataProvider.Id,
                BasisOfRecord = new VocabularyValue { Id = (int)BasisOfRecordId.HumanObservation },
                CollectionCode = verbatim.CollectionCode,
                CollectionId = verbatim.CollectionId,
                DatasetId = $"urn:lsid:observationsdatabasen.se:dataprovider:{DataProviderIdentifiers.ObservationDatabase}",
                DatasetName = "Observation database",
                DiffusionStatus = DiffusionStatus.NotDiffused,
                Event = new Event(verbatim.StartDate, null, verbatim.EndDate, null)
                {
                    FieldNotes = verbatim.Origin?.Clean(), // Is there any better field for this?
                    Habitat = verbatim.Habitat?.Clean(),
                    // Substrate = verbatim.Substrate, Todo map 
                    VerbatimEventDate = DwcFormatter.CreateDateIntervalString(verbatim.StartDate.ToLocalTime(), verbatim.EndDate.ToLocalTime())
                },
                Identification = new Identification
                {
                    UncertainIdentification = false,
                    Validated = false,
                    Verified = false,
                    ValidationStatus = new VocabularyValue { Id = (int)ValidationStatusId.ReportedByExpert },
                    VerificationStatus = new VocabularyValue { Id = (int)ValidationStatusId.ReportedByExpert }
                },
                InstitutionId = verbatim.SCI_code,
                Location = new Location
                {
                    Attributes = new LocationAttributes
                    {
                        VerbatimMunicipality = verbatim.Municipality,
                        VerbatimProvince = verbatim.Province
                    },
                    Locality = verbatim.Locality?.Clean(),
                    VerbatimCoordinateSystem = "EPSG:3857",
                    VerbatimLocality = verbatim.Locality?.Clean()
                },
                Modified = verbatim.EditDate,
                Occurrence = new Occurrence
                {
                    BirdNestActivityId = taxon?.IsBird() ?? false ? 1000000 : 0,
                    CatalogId = verbatim.Id,
                    CatalogNumber = verbatim.Id.ToString(),
                    IndividualCount = verbatim.IndividualCount?.Clean(),
                    IsNaturalOccurrence = true,
                    IsNeverFoundObservation = verbatim.IsNeverFoundObservation,
                    IsNotRediscoveredObservation = verbatim.IsNotRediscoveredObservation,
                    IsPositiveObservation = verbatim.TaxonId != 0,
                    OccurrenceId = $"urn:lsid:observationsdatabasen.se:observation:{verbatim.Id}",
                    OccurrenceRemarks = verbatim.OccurrenceRemarks?.Clean(),
                    OccurrenceStatus = new VocabularyValue { Id = (int)(verbatim.TaxonId == 0 ? OccurrenceStatusId.Absent : OccurrenceStatusId.Present) },
                    ProtectionLevel = verbatim.ProtectionLevel,
                    SensitivityCategory = verbatim.ProtectionLevel,
                    RecordedBy = verbatim.Observers,
                    ReportedDate = verbatim.StartDate.ToUniversalTime()
                },
                OwnerInstitutionCode = verbatim.SCI_code,
                RightsHolder = verbatim.SCI_name?.Clean(),
                Taxon = taxon
            };

            obs.Occurrence.OrganismQuantity = obs.Occurrence.IndividualCount;
            if (int.TryParse(obs.Occurrence.OrganismQuantity, out var quantity))
            {
                obs.Occurrence.OrganismQuantityInt = quantity;
            }
            AddPositionData(obs.Location, verbatim.CoordinateX, verbatim.CoordinateY, CoordinateSys.Rt90_25_gon_v, 
                verbatim.CoordinateUncertaintyInMeters, taxon?.Attributes?.DisturbanceRadius);

            _areaHelper.AddAreaDataToProcessedLocation(obs.Location);

            // Populate generic data
            PopulateGenericData(obs);

            return obs;
        }
    }
}