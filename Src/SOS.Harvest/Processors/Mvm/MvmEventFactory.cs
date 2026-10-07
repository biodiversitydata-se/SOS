using SOS.Api.Contracts.Common.Geo;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Models.Verbatim.Mvm;
using SOS.Lib.Repositories.Resource.Interfaces;
using System.Text.RegularExpressions;

namespace SOS.Harvest.Processors.Mvm
{
    /// <summary>
    /// MVM event factory.
    /// </summary>
    public class MvmEventFactory : EventFactoryBase, IEventFactory<MvmSampleVerbatim>
    {
        private const int DefaultCoordinateUncertaintyInMeters = 5000;
        private readonly IAreaHelper _areaHelper;
        private readonly IDictionary<VocabularyId, IDictionary<object, int>> _vocabularyById;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="vocabularyById"></param>
        /// <param name="areaHelper"></param>
        public MvmEventFactory(
            DataProvider dataProvider,
            IDictionary<VocabularyId, IDictionary<object, int>> vocabularyById,
            IAreaHelper areaHelper,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration) : base(dataProvider, processTimeManager, processConfiguration)
        {
            _vocabularyById = vocabularyById ?? throw new ArgumentNullException(nameof(vocabularyById));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public Lib.Models.Processed.DataStewardship.Event.Event CreateEventObservation(MvmSampleVerbatim verbatim)
        {
            try
            {
                var dates = ParseDates(verbatim);

                var obsEvent = new Event(dates.SamplingDate!.Value);
                var processedEvent = new Lib.Models.Processed.DataStewardship.Event.Event();
                processedEvent.Created = DateTime.Now.ToUniversalTime();
                processedEvent.DataProviderId = DataProvider.Id;
                processedEvent.StartDate = obsEvent.StartDate;
                processedEvent.EndDate = obsEvent.EndDate;
                processedEvent.PlainStartDate = obsEvent.PlainStartDate;
                processedEvent.PlainEndDate = obsEvent.PlainEndDate;
                processedEvent.PlainStartTime = obsEvent.PlainStartTime;
                processedEvent.PlainEndTime = obsEvent.PlainEndTime;
                processedEvent.EventId = $"urn:lsid:mvm:event:{verbatim.SampleId}";
                processedEvent.Habitat = verbatim.MediumTypeName?.Clean();                
                //processedEvent.EventRemarks = $"StudyName={verbatim.StudyName?.Clean()}, ProductName={verbatim.ProductName?.Clean()}, DeliveryTitle={verbatim.DeliveryTitle?.Clean()}";
                //processedEvent.SampleSizeUnit = verbatim.SampleSizeUnit?.Clean();
                //processedEvent.SampleSizeValue = verbatim.SampleSizeValue?.Clean();
                //processedEvent.SamplingEffort = verbatim.SamplingEffort?.Clean();
                //processedEvent.SamplingProtocol = verbatim.SamplingProtocol?.Clean();
                processedEvent.OccurrenceIds = verbatim.Observations?.Select(m => $"urn:lsid:mvm:observation:{m.ObservationId}")?.ToList(); // todo - some occurrences could be invalid.
                processedEvent.Location = CreateLocation(verbatim);
                return processedEvent;
            }
            catch (Exception e)
            {
                throw new Exception($"Error when processing MVM verbatim event with Id={verbatim.Id}, SampleId={verbatim.SampleId}", e);
            }
        }     

        private enum MappingNotFoundLogic
        {
            UseSourceValue,
            UseDefaultValue
        }

        private Location CreateLocation(MvmSampleVerbatim verbatim)
        {
            var processedLocation = new Location(LocationType.Point);
            processedLocation.Locality = verbatim.SamplingSiteName?.Clean();
            processedLocation.LocationId = $"urn:lsid:mvm.se:site:{verbatim.SamplingSiteId}";
            processedLocation.MinimumDepthInMeters = verbatim.MinDepth;
            processedLocation.MaximumDepthInMeters = verbatim.MaxDepth;
            double? verbatimLongitude = double.TryParse(verbatim.SamplingSiteX, out var parsedSamplingSiteX) ? parsedSamplingSiteX : null;
            double? verbatimLatitude = double.TryParse(verbatim.SamplingSiteY, out var parsedSamplingSiteY) ? parsedSamplingSiteY : null;
            int coordinateUncertaintyInMeters = 0;
            processedLocation.Locality = verbatim.StationName?.Clean();
            processedLocation.LocationId = $"urn:lsid:mvm.se:site:{verbatim.StationEUID}";

            if (verbatimLongitude.HasValue && verbatimLatitude.HasValue)
            {
                coordinateUncertaintyInMeters = 1000;
                processedLocation.LocationRemarks = verbatim.SamplingSiteName?.Clean();
            }
            else
            {
                verbatimLongitude = verbatim.StationCoordinateX;
                verbatimLatitude = verbatim.StationCoordinateY;
                coordinateUncertaintyInMeters = 5000;
            }

            AddPositionData(
                processedLocation,
                verbatimLongitude,
                verbatimLatitude,
                CoordinateSys.SWEREF99_TM,
                coordinateUncertaintyInMeters,
                null);

            _areaHelper.AddAreaDataToProcessedLocation(processedLocation);
            return processedLocation;
        }

        private VocabularyValue? GetSosId(string val,
            IDictionary<object, int> sosIdByValue,
            int? defaultValue = null,
            MappingNotFoundLogic mappingNotFoundLogic = MappingNotFoundLogic.UseSourceValue)
        {
            if (string.IsNullOrWhiteSpace(val) || sosIdByValue == null)
            {
                return defaultValue.HasValue ? new VocabularyValue { Id = defaultValue.Value } : null;
            }

            var lookupVal = val.ToLower();
            if (sosIdByValue.TryGetValue(lookupVal, out var sosId))
            {
                return new VocabularyValue { Id = sosId };
            }

            if (mappingNotFoundLogic == MappingNotFoundLogic.UseDefaultValue && defaultValue.HasValue)
            {
                return new VocabularyValue { Id = defaultValue.Value };
            }

            return new VocabularyValue
            { Id = VocabularyConstants.NoMappingFoundCustomValueIsUsedId, Value = val };
        }

        /// <summary>
        ///     Get vocabulary mappings for external system.
        /// </summary>
        /// <param name="externalSystemId"></param>
        /// <param name="allVocabularies"></param>
        /// <param name="convertValuesToLowercase"></param>
        /// <returns></returns>
        private static IDictionary<VocabularyId, IDictionary<object, int>> GetVocabulariesDictionary(
            ExternalSystemId externalSystemId,
            ICollection<Vocabulary> allVocabularies,
            bool convertValuesToLowercase)
        {
            var dic = new Dictionary<VocabularyId, IDictionary<object, int>>();

            foreach (var vocabulary in allVocabularies)
            {
                var vocabularies = vocabulary.ExternalSystemsMapping.FirstOrDefault(m => m.Id == externalSystemId);
                if (vocabularies != null)
                {
                    var mapping = vocabularies.Mappings.Single();
                    var sosIdByValue = mapping.GetIdByValueDictionary(convertValuesToLowercase);
                    dic.Add(vocabulary.Id, sosIdByValue);
                }
            }

            // Add missing entries. Initialize with empty dictionary.
            foreach (VocabularyId vocabularyId in (VocabularyId[])Enum.GetValues(typeof(VocabularyId)))
            {
                if (!dic.ContainsKey(vocabularyId))
                {
                    dic.Add(vocabularyId, new Dictionary<object, int>());
                }
            }

            return dic;
        }

        /// <summary>
        /// Try parse taxon id from string
        /// </summary>
        /// <param name="taxonIdString"></param>
        /// <returns></returns>
        private int TryParseTaxonId(string taxonIdString)
        {
            taxonIdString = Regex.Match(taxonIdString, @"\d+").Value;

            if (!int.TryParse(taxonIdString, out var taxonId))
            {
                return -1;
            }

            return taxonId;
        }

        /// <summary>
        /// Try to parse taxon id strings
        /// </summary>
        /// <param name="taxonIdStrings"></param>
        /// <returns></returns>
        private IEnumerable<int> TryParseTaxonIds(IEnumerable<string> taxonIdStrings)
        {
            if (!taxonIdStrings?.Any() ?? true)
            {
                return null!;
            }

            var taxonIds = new HashSet<int>();

            foreach (var taxonIdString in taxonIdStrings!)
            {
                var taxonId = TryParseTaxonId(taxonIdString);
                if (taxonId >= 0)
                {
                    taxonIds.Add(taxonId);
                }
            }

            return taxonIds;
        }

        public static async Task<MvmEventFactory> CreateAsync(
            DataProvider dataProvider,
            IVocabularyRepository processedVocabularyRepository,
            IAreaHelper areaHelper,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration)
        {
            var vocabularies = await processedVocabularyRepository.GetAllAsync();
            var vocabularyById = GetVocabulariesDictionary(
                ExternalSystemId.DarwinCore,
                vocabularies.ToArray(),
                true);
            return new MvmEventFactory(dataProvider, vocabularyById, areaHelper, processTimeManager, processConfiguration);
        }

        private (DateTime? SamplingDate, DateTime? InsertDate, DateTime? EditDate) ParseDates(MvmSampleVerbatim verbatim)
        {
            DateTime? samplingDate = null;
            DateTime? insertDate = null;
            DateTime? editDate = null;
            if (DateTime.TryParse(verbatim.SamplingDate, out var parsedSamplingDate))
            {
                samplingDate = parsedSamplingDate;
            }
            if (DateTime.TryParse(verbatim.InsertDate, out var parsedInsertDate))
            {
                insertDate = parsedInsertDate;
            }
            if (DateTime.TryParse(verbatim.EditDate, out var parsedEditDate))
            {
                editDate = parsedEditDate;
            }

            return (samplingDate, insertDate, editDate);
        }
    }
}