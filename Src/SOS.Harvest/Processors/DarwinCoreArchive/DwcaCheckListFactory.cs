using System.Text.RegularExpressions;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.CheckList;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Harvest.Processors.Interfaces;

namespace SOS.Harvest.Processors.DarwinCoreArchive
{
    /// <summary>
    ///     DwC-A observation factory.
    /// </summary>
    public class DwcaCheckListFactory : CheckListFactoryBase, ICheckListFactory<DwcEventOccurrenceVerbatim>
    {
        private const int DefaultCoordinateUncertaintyInMeters = 5000;
        private readonly IAreaHelper _areaHelper;
        private readonly IDictionary<VocabularyId, IDictionary<object, int>> _vocabularyById;

        private enum MappingNotFoundLogic
        {
            UseSourceValue,
            UseDefaultValue
        }

        private Event CreateEvent(DwcEventOccurrenceVerbatim verbatim)
        {
            var processedEvent = new Event();
            processedEvent.EventId = verbatim.EventID;
            processedEvent.ParentEventId = verbatim.ParentEventID;
            processedEvent.EventRemarks = verbatim.EventRemarks;
            processedEvent.FieldNotes = verbatim.FieldNotes;
            processedEvent.FieldNumber = verbatim.FieldNumber;
            processedEvent.Habitat = verbatim.Habitat;
            processedEvent.SampleSizeUnit = verbatim.SampleSizeUnit;
            processedEvent.SampleSizeValue = verbatim.SampleSizeValue;
            processedEvent.SamplingEffort = verbatim.SamplingEffort;
            processedEvent.SamplingProtocol = verbatim.SamplingProtocol;
            processedEvent.VerbatimEventDate = verbatim.VerbatimEventDate;

            DwcParser.TryParseEventDate(
                verbatim.EventDate,
                verbatim.Year,
                verbatim.Month,
                verbatim.Day,
                verbatim.EventTime,
                out var startDate,
                out var endDate);

            processedEvent.StartDate = startDate?.ToUniversalTime();
            processedEvent.EndDate = endDate?.ToUniversalTime();

            return processedEvent;
        }

        private Location CreateLocation(DwcEventOccurrenceVerbatim verbatim)
        {
            var processedLocation = new Location();
            processedLocation.Continent = GetSosId(
                verbatim.Continent,
                _vocabularyById[VocabularyId.Continent],
                (int)ContinentId.Europe,
                MappingNotFoundLogic.UseDefaultValue);
            processedLocation.CoordinatePrecision = verbatim.CoordinatePrecision.ParseDouble();
            processedLocation.CoordinateUncertaintyInMeters =
                verbatim.CoordinateUncertaintyInMeters?.ParseDoubleConvertToInt() ?? DefaultCoordinateUncertaintyInMeters;
            processedLocation.Country = GetSosId(
                verbatim.Country,
                _vocabularyById[VocabularyId.Country],
                (int)CountryId.Sweden,
                MappingNotFoundLogic.UseDefaultValue);
            processedLocation.CountryCode = verbatim.CountryCode;
            processedLocation.FootprintSpatialFit = verbatim.FootprintSpatialFit;
            processedLocation.FootprintSRS = verbatim.FootprintSRS;
            processedLocation.FootprintWKT = verbatim.FootprintWKT;
            processedLocation.GeoreferencedBy = verbatim.GeoreferencedBy;
            processedLocation.GeoreferencedDate = verbatim.GeoreferencedDate;
            processedLocation.GeoreferenceProtocol = verbatim.GeoreferenceProtocol;
            processedLocation.GeoreferenceRemarks = verbatim.GeoreferenceRemarks;
            processedLocation.GeoreferenceSources = verbatim.GeoreferenceSources;
            processedLocation.GeoreferenceVerificationStatus = verbatim.GeoreferenceVerificationStatus;
            processedLocation.HigherGeography = verbatim.HigherGeography;
            processedLocation.HigherGeographyId = verbatim.HigherGeographyID;
            processedLocation.Island = verbatim.Island;
            processedLocation.IslandGroup = verbatim.IslandGroup;
            processedLocation.Locality = verbatim.Locality;
            processedLocation.LocationAccordingTo = verbatim.LocationAccordingTo;
            processedLocation.LocationId = verbatim.LocationID;
            processedLocation.LocationRemarks = verbatim.LocationRemarks;
            processedLocation.MaximumDepthInMeters = verbatim.MaximumDepthInMeters.ParseDouble();
            processedLocation.MaximumDistanceAboveSurfaceInMeters =
                verbatim.MaximumDistanceAboveSurfaceInMeters.ParseDouble();
            processedLocation.MaximumElevationInMeters = verbatim.MaximumElevationInMeters.ParseDouble();
            processedLocation.MinimumDepthInMeters = verbatim.MinimumDepthInMeters.ParseDouble();
            processedLocation.MinimumDistanceAboveSurfaceInMeters =
                verbatim.MinimumDistanceAboveSurfaceInMeters.ParseDouble();
            processedLocation.MinimumElevationInMeters = verbatim.MinimumElevationInMeters.ParseDouble();
            processedLocation.Attributes.VerbatimMunicipality = verbatim.Municipality;
            processedLocation.Attributes.VerbatimProvince = verbatim.StateProvince;
            processedLocation.VerbatimCoordinates = verbatim.VerbatimCoordinates;
            processedLocation.VerbatimCoordinateSystem = verbatim.VerbatimCoordinateSystem;
            processedLocation.VerbatimDepth = verbatim.VerbatimDepth;
            processedLocation.VerbatimElevation = verbatim.VerbatimElevation;
            processedLocation.WaterBody = verbatim.WaterBody;

            return processedLocation;
        }

        private VocabularyValue GetSosId(string val,
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

            foreach (var taxonIdString in taxonIdStrings)
            {
                var taxonId = TryParseTaxonId(taxonIdString);
                if (taxonId >= 0)
                {
                    taxonIds.Add(taxonId);
                }
            }

            return taxonIds;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="vocabularyById"></param>
        /// <param name="areaHelper"></param>
        public DwcaCheckListFactory(
            DataProvider dataProvider,
            IDictionary<VocabularyId, IDictionary<object, int>> vocabularyById,
            IAreaHelper areaHelper) : base(dataProvider)
        {
            _vocabularyById = vocabularyById ?? throw new ArgumentNullException(nameof(vocabularyById));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }


        public static async Task<DwcaCheckListFactory> CreateAsync(
            DataProvider dataProvider,
            IVocabularyRepository processedVocabularyRepository,
            IAreaHelper areaHelper)
        {
            var vocabularies = await processedVocabularyRepository.GetAllAsync();
            var vocabularyById = GetVocabulariesDictionary(
                ExternalSystemId.DarwinCore,
                vocabularies.ToArray(),
                true);
            return new DwcaCheckListFactory(dataProvider, vocabularyById, areaHelper);
        }

        /// <summary>
        ///     Cast verbatim check list to processed data model
        /// </summary>
        /// <param name="verbatimCheckList"></param>
        /// <returns></returns>
        public CheckList CreateProcessedCheckList(DwcEventOccurrenceVerbatim verbatimCheckList)
        {
            try
            {
                if (verbatimCheckList == null)
                {
                    return null;
                }

                var id = $"urn:lsid:{DataProvider.CheckListIdentifier}:Checklist:{verbatimCheckList.Id}";

                DateTime.TryParse(verbatimCheckList.EventDate, out var eventDate);
                DateTime.TryParse(verbatimCheckList.Modified, out var modified);

                var checkList = new CheckList
                {
                    DataProviderId = DataProvider.Id,
                    Id = id,
                    Event = CreateEvent(verbatimCheckList),
                    Location = CreateLocation(verbatimCheckList),
                    Modified = modified,
                    Name = $"{DataProvider.CheckListIdentifier}-{verbatimCheckList.EventID}",
                    OccurrenceIds =
                        verbatimCheckList.Observations?.Select(o => o.OccurrenceID),
                    RecordedBy = verbatimCheckList.RecordedBy,
                    SamplingEffortTime = verbatimCheckList.SamplingEffortTime,
                    TaxonIds = TryParseTaxonIds(verbatimCheckList.Taxa?.Select(t => t.TaxonID)),
                    TaxonIdsFound = TryParseTaxonIds(verbatimCheckList.Observations?.Select(o => o.TaxonID))?.Distinct()
                };

                if (!GISExtensions.TryParseCoordinateSystem(verbatimCheckList.GeodeticDatum, out var coordinateSystem))
                {
                    coordinateSystem = CoordinateSys.WGS84;
                }
                AddPositionData(checkList.Location, verbatimCheckList.DecimalLongitude.ParseDouble(), verbatimCheckList.DecimalLatitude.ParseDouble(),
                    coordinateSystem, verbatimCheckList.CoordinateUncertaintyInMeters?.ParseDoubleConvertToInt() ?? DefaultCoordinateUncertaintyInMeters, 0);
                _areaHelper.AddAreaDataToProcessedLocation(checkList.Location);

                return checkList;
            }
            catch (Exception e)
            {
                throw new Exception($"Error when processing DwC verbatim check list with Id={verbatimCheckList.Id}", e);
            }
        }
    }
}