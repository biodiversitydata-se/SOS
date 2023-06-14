using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shark;
using SOS.Harvest.Constants;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Configuration.Process;

namespace SOS.Harvest.Processors.Shark
{
    public class SharkObservationFactory : ObservationFactoryBase, IObservationFactory<SharkObservationVerbatim>
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
        public SharkObservationFactory(DataProvider dataProvider, 
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa, 
            IAreaHelper areaHelper,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration) : base(dataProvider, taxa, processTimeManager, processConfiguration)
        {
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        /// <summary>
        ///  Cast verbatim observations to processed data model
        /// </summary>
        /// <param name="verbatim"></param>
        /// <param name="diffuseIfSupported"></param>
        /// <returns></returns>
        public Observation CreateProcessedObservation(SharkObservationVerbatim verbatim, bool diffuseIfSupported)
        {
            var taxon = GetTaxon(verbatim.DyntaxaId.HasValue ? verbatim.DyntaxaId.Value : -1, new[] {verbatim.ScientificName, verbatim.ReportedScientificName}.Distinct());
            var sharkSampleId = $"{(string.IsNullOrEmpty(verbatim.Sharksampleidmd5) ? verbatim.SharkSampleId : verbatim.Sharksampleidmd5)}-{taxon.Id}".RemoveWhiteSpace();

            var obs = new Observation
            {                
                DataProviderId = DataProvider.Id,
                BasisOfRecord = new VocabularyValue { Id = (int)BasisOfRecordId.HumanObservation},
                DatasetId = $"urn:lsid:swedishlifewatch.se:dataprovider:{DataProviderIdentifiers.SHARK}",
                DatasetName = verbatim.DatasetName,
                DiffusionStatus = DiffusionStatus.NotDiffused,
                Event = new Event(verbatim.SampleDate, null, verbatim.SampleDate, null),
                Identification = new Identification
                {
                    IdentifiedBy = verbatim.AnalysedBy,
                    UncertainIdentification = false,
                    Verified = false,
                    VerificationStatus = new VocabularyValue { Id = (int)ValidationStatusId.ReportedByExpert }
                },
                Location = new Location(LocationType.Point)
                {
                    MaximumDepthInMeters = verbatim.WaterDepthM,
                    MinimumDepthInMeters = verbatim.WaterDepthM
                },
                MeasurementOrFacts = verbatim.Parameters?.Select(p => new ExtendedMeasurementOrFact
                {
                    MeasurementType = p.Name,
                    MeasurementUnit = p.Unit,
                    MeasurementValue = p.Value
                })?.ToArray(),
                Occurrence = new Occurrence
                {
                    BirdNestActivityId = 0,
                    CatalogNumber = sharkSampleId,
                    OccurrenceId = $"urn:lsid:shark:observation:{sharkSampleId}",
                    IsNaturalOccurrence = true,
                    IsNeverFoundObservation = GetIsNeverFoundObservation(verbatim.DyntaxaId),
                    IsNotRediscoveredObservation = false,
                    IsPositiveObservation = GetIsPositiveObservation(verbatim.DyntaxaId),
                    //ProtectionLevel = CalculateProtectionLevel(taxon),
                    SensitivityCategory = CalculateProtectionLevel(taxon),
                    RecordedBy = verbatim.Taxonomist,
                    ReportedBy = verbatim.ReportedStationName,
                    OccurrenceStatus = GetOccurrenceStatusId(verbatim.DyntaxaId)
                },
                OwnerInstitutionCode = verbatim.ReportingInstituteNameSv,
                Taxon = taxon
            };

            obs.AccessRights = GetAccessRightsFromSensitivityCategory(obs.Occurrence.SensitivityCategory);
            AddPositionData(obs.Location, verbatim.SampleLongitudeDd, verbatim.SampleLatitudeDd, 
                CoordinateSys.WGS84, ProcessConstants.DefaultAccuracyInMeters, taxon?.Attributes?.DisturbanceRadius);
            _areaHelper.AddAreaDataToProcessedLocation(obs.Location);

            /*
            DataType
            SamplerType
            Species
            ReportingInstitutionCode ?
            AnalyticalLaboratoryCode ?
            Status => Occurrence.Status ?

             */
            // Populate generic data
            PopulateGenericData(obs);

            return obs;
        }

        /// <summary>
        ///     Gets the occurrence status. Set to Present if DyntaxaTaxonId from provider is greater than 0 and Absent if
        ///     DyntaxaTaxonId is 0
        /// </summary>
        private VocabularyValue GetOccurrenceStatusId(int? dyntaxaTaxonId)
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
        private bool GetIsNeverFoundObservation(int? dyntaxaTaxonId)
        {
            return dyntaxaTaxonId == 0;
        }

        /// <summary>
        ///     Set to True if DyntaxaTaxonId from provider is greater than 0 and False if DyntaxaTaxonId is 0.
        /// </summary>
        private bool GetIsPositiveObservation(int? dyntaxaTaxonId)
        {
            return dyntaxaTaxonId != 0;
        }
    }
}