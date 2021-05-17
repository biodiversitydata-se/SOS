using System;
using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.DarwinCore.Vocabulary;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.ObservationDatabase;
using SOS.Process.Processors.Interfaces;

namespace SOS.Process.Processors.ObservationDatabase
{
    /// <summary>
    /// Observation database factory
    /// </summary>
    public class ObservationDatabaseObservationFactory : IObservationFactory<ObservationDatabaseVerbatim>
    {
        private readonly DataProvider _dataProvider;
        private readonly IDictionary<int, Lib.Models.Processed.Observation.Taxon> _taxa;
        private readonly IAreaHelper _areaHelper;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        public ObservationDatabaseObservationFactory(DataProvider dataProvider, IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa, IAreaHelper areaHelper)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _taxa = taxa ?? throw new ArgumentNullException(nameof(taxa));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <summary>
        ///     Cast multiple clam observations to ProcessedObservation
        /// </summary>
        /// <param name="verbatims"></param>
        /// <returns></returns>
        public IEnumerable<Observation> CreateProcessedObservations(
            IEnumerable<ObservationDatabaseVerbatim> verbatims)
        {
            return verbatims.Select(CreateProcessedObservation);
        }

        /// <summary>
        ///     Cast KUL observation verbatim to ProcessedObservation
        /// </summary>
        /// <param name="verbatim"></param>
        /// <returns></returns>
        public Observation CreateProcessedObservation(ObservationDatabaseVerbatim verbatim)
        {
            _taxa.TryGetValue(verbatim.TaxonId, out var taxon);

            var obs = new Observation
            {
                AccessRights = new VocabularyValue { Id = (int)AccessRightsId.NotForPublicUsage },
                DataProviderId = _dataProvider.Id,
                BasisOfRecord = new VocabularyValue { Id = (int)BasisOfRecordId.HumanObservation },
                CollectionCode = verbatim.CollectionCode,
                CollectionId = verbatim.CollectionId,
                DatasetId = $"urn:lsid:observationsdatabasen.se:dataprovider:{DataProviderIdentifiers.ObservationDatabase}",
                DatasetName = "Observation database",
                Event = new Event
                {
                    EndDate = verbatim.EndDate.ToUniversalTime(),
                    FieldNotes = verbatim.Origin, // Is there any better field for this?
                    Habitat = verbatim.Habitat,
                    StartDate = verbatim.StartDate.ToUniversalTime(),
                   // Substrate = verbatim.Substrate, Todo map 
                    VerbatimEventDate = DwcFormatter.CreateDateIntervalString(verbatim.StartDate, verbatim.EndDate)
                },
                Identification = new Identification
                {
                    UncertainIdentification = false,
                    Validated = false,
                    ValidationStatus = new VocabularyValue { Id = (int)ValidationStatusId.ReportedByExpert }
                },
                InstitutionId = verbatim.SCI_code,
                Location = new Location(verbatim.CoordinateX, verbatim.CoordinateY, CoordinateSys.Rt90_25_gon_v, verbatim.CoordinateUncertaintyInMeters, taxon?.Attributes?.DisturbanceRadius)
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
                    CatalogId = verbatim.Id,
                    CatalogNumber = verbatim.Id.ToString(),
                    IndividualCount = verbatim.IndividualCount,
                    IsNaturalOccurrence = true,
                    IsNeverFoundObservation = verbatim.IsNeverFoundObservation,
                    IsNotRediscoveredObservation = verbatim.IsNotRediscoveredObservation,
                    IsPositiveObservation = verbatim.TaxonId != 0,
                    OccurrenceId = $"urn:lsid:observationsdatabasen.se:Observation:{verbatim.Id}",
                    OccurrenceRemarks = verbatim.OccurrenceRemarks,
                    OccurrenceStatus = new VocabularyValue { Id = (int)(verbatim.TaxonId == 0 ? OccurrenceStatusId.Absent : OccurrenceStatusId.Present) },
                    ProtectionLevel = verbatim.ProtectionLevel,
                    RecordedBy = verbatim.Observers,
                    ReportedDate = verbatim.StartDate.ToUniversalTime()
                },
                OwnerInstitutionCode = verbatim.SCI_code,
                RightsHolder = verbatim.SCI_name,
                Taxon = taxon
            };

            _areaHelper.AddAreaDataToProcessedObservation(obs);

            return obs;
        }
    }
}