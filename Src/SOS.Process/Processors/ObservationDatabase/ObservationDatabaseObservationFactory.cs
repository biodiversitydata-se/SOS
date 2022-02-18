using System;
using System.Collections.Generic;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.ObservationDatabase;
using SOS.Process.Processors.Interfaces;

namespace SOS.Process.Processors.ObservationDatabase
{
    /// <summary>
    /// Observation database factory
    /// </summary>
    public class ObservationDatabaseObservationFactory : ObservationFactoryBase, IObservationFactory<ObservationDatabaseVerbatim>
    {
        private readonly DataProvider _dataProvider;
        private readonly IAreaHelper _areaHelper;

        /// <summary>
        ///  Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="areaHelper"></param>
        /// <param name="geometryManager"></param>
        public ObservationDatabaseObservationFactory(DataProvider dataProvider, IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa, IAreaHelper areaHelper) : base(taxa)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
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
                DataProviderId = _dataProvider.Id,
                BasisOfRecord = new VocabularyValue { Id = (int)BasisOfRecordId.HumanObservation },
                CollectionCode = verbatim.CollectionCode,
                CollectionId = verbatim.CollectionId,
                DatasetId = $"urn:lsid:observationsdatabasen.se:dataprovider:{DataProviderIdentifiers.ObservationDatabase}",
                DatasetName = "Observation database",
                DiffusionStatus = DiffusionStatus.NotDiffused,
                Event = new Event
                {
                    EndDate = verbatim.EndDate.ToUniversalTime(),
                    FieldNotes = verbatim.Origin, // Is there any better field for this?
                    Habitat = verbatim.Habitat,
                    StartDate = verbatim.StartDate.ToUniversalTime(),
                    PlainStartDate = verbatim.StartDate.ToLocalTime().ToString("yyyy-MM-dd"),
                    PlainEndDate = verbatim.EndDate.ToLocalTime().ToString("yyyy-MM-dd"),
                    PlainStartTime = null,
                    PlainEndTime = null,
                    // Substrate = verbatim.Substrate, Todo map 
                    VerbatimEventDate = DwcFormatter.CreateDateIntervalString(verbatim.StartDate.ToLocalTime(), verbatim.EndDate.ToLocalTime())
                },
                Identification = new Identification
                {
                    UncertainIdentification = false,
                    Validated = true,
                    Verified = true,
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
                    Locality = verbatim.Locality,
                    VerbatimCoordinateSystem = "EPSG:3857"
                },
                Modified = verbatim.EditDate,
                Occurrence = new Occurrence
                {
                    BirdNestActivityId = taxon?.IsBird() ?? false ? 1000000 : 0,
                    CatalogId = verbatim.Id,
                    CatalogNumber = verbatim.Id.ToString(),
                    IndividualCount = verbatim.IndividualCount,
                    IsNaturalOccurrence = true,
                    IsNeverFoundObservation = verbatim.IsNeverFoundObservation,
                    IsNotRediscoveredObservation = verbatim.IsNotRediscoveredObservation,
                    IsPositiveObservation = verbatim.TaxonId != 0,
                    OccurrenceId = $"urn:lsid:observationsdatabasen.se:observation:{verbatim.Id}",
                    OccurrenceRemarks = verbatim.OccurrenceRemarks,
                    OccurrenceStatus = new VocabularyValue { Id = (int)(verbatim.TaxonId == 0 ? OccurrenceStatusId.Absent : OccurrenceStatusId.Present) },
                    ProtectionLevel = verbatim.ProtectionLevel,
                    SensitivityCategory = verbatim.ProtectionLevel,
                    RecordedBy = verbatim.Observers,
                    ReportedDate = verbatim.StartDate.ToUniversalTime()
                },
                OwnerInstitutionCode = verbatim.SCI_code,
                RightsHolder = verbatim.SCI_name,
                Taxon = taxon
            };

            AddPositionData(obs.Location, verbatim.CoordinateX, verbatim.CoordinateY, CoordinateSys.Rt90_25_gon_v, 
                verbatim.CoordinateUncertaintyInMeters, taxon?.Attributes?.DisturbanceRadius);

            _areaHelper.AddAreaDataToProcessedLocation(obs.Location);

            return obs;
        }
    }
}