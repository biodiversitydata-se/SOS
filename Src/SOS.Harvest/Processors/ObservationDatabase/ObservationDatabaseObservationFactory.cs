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
using SOS.Lib.Configuration.Process;
using SOS.Lib.Repositories.Resource.Interfaces;

namespace SOS.Harvest.Processors.ObservationDatabase
{
    /// <summary>
    /// Observation database factory
    /// </summary>
    public class ObservationDatabaseObservationFactory : ObservationFactoryBase, IObservationFactory<ObservationDatabaseVerbatim>
    {
        private readonly IAreaHelper _areaHelper;
        private readonly IDictionary<VocabularyId, IDictionary<object, int>> _vocabularyById;

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
            IDictionary<VocabularyId, IDictionary<object, int>> vocabularyById,
            IAreaHelper areaHelper,
            IProcessTimeManager processTimeManager, 
            ProcessConfiguration processConfiguration) : base(dataProvider, taxa, processTimeManager, processConfiguration)
        {
            _vocabularyById = vocabularyById ?? throw new ArgumentNullException(nameof(vocabularyById));
            _areaHelper = areaHelper ?? throw new ArgumentNullException(nameof(areaHelper));
        }

        public static async Task<ObservationDatabaseObservationFactory> CreateAsync(
            DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa,
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
            return new ObservationDatabaseObservationFactory(dataProvider, taxa, vocabularyById, areaHelper, processTimeManager, processConfiguration);
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
                    VerbatimEventDate = DwcFormatter.CreateDateIntervalString(verbatim.StartDate.HasValue ? verbatim.StartDate.Value.ToLocalTime() : null, verbatim.EndDate.HasValue ? verbatim.EndDate.Value.ToLocalTime() : null)
                },
                Identification = new Identification
                {
                    UncertainIdentification = false,
                    Verified = false,
                    VerificationStatus = new VocabularyValue { Id = (int)ValidationStatusId.ReportedByExpert }
                },
                InstitutionId = verbatim.SCI_code,
                Location = new Location(LocationType.Point)
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
                    LifeStage = GetSosId(verbatim.Stadium, _vocabularyById[VocabularyId.LifeStage]),
                    Sex = GetSosId(verbatim.Stadium, _vocabularyById[VocabularyId.Sex]),
                    Behavior = GetSosId(verbatim.Stadium, _vocabularyById[VocabularyId.Behavior]),                    
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
                    //ProtectionLevel = verbatim.ProtectionLevel,
                    SensitivityCategory = verbatim.ProtectionLevel,
                    RecordedBy = verbatim.Observers,
                    ReportedDate = verbatim.StartDate.HasValue ? verbatim.StartDate.Value.ToUniversalTime() : null
                },
                OwnerInstitutionCode = verbatim.SCI_code,
                RightsHolder = verbatim.SCI_name?.Clean(),
                Taxon = taxon
            };
                        
            obs.Occurrence.OrganismQuantity = obs.Occurrence.IndividualCount;
            if (int.TryParse(obs.Occurrence.OrganismQuantity, out var quantity))
            {
                obs.Occurrence.OrganismQuantityAggregation = quantity;
                obs.Occurrence.OrganismQuantityInt = quantity;
            }
            AddPositionData(obs.Location, verbatim.CoordinateX, verbatim.CoordinateY, CoordinateSys.Rt90_25_gon_v, 
                verbatim.CoordinateUncertaintyInMeters, taxon?.Attributes?.DisturbanceRadius);

            _areaHelper.AddAreaDataToProcessedLocation(obs.Location);

            // Populate generic data
            PopulateGenericData(obs);
            obs.Occurrence.Activity = GetActivityValue(verbatim, taxon);
            obs.Occurrence.BirdNestActivityId = GetBirdNestActivityId(obs.Occurrence.Activity, taxon);
            return obs;
        }

        private VocabularyValue GetActivityValue(ObservationDatabaseVerbatim verbatim, Lib.Models.Processed.Observation.Taxon taxon)
        {
            if (taxon.IsBird())
            {
                if (!string.IsNullOrEmpty(verbatim.Stadium))
                {
                    return verbatim.Stadium.ToLowerInvariant() switch
                    {
                        "säker häckning" => new VocabularyValue { Id = (int)ActivityId.NestBuilding }, // 12
                        "trolig häckning" => new VocabularyValue { Id = (int)ActivityId.PermanentTerritory }, // 17
                        _ => new VocabularyValue { Id = (int)ActivityId.InNestingHabitat } // 20
                    };
                }
                else // No value
                {
                    return new VocabularyValue { Id = (int)ActivityId.InNestingHabitat }; // 20
                }
            }

            return GetSosId(verbatim.Stadium, _vocabularyById[VocabularyId.Activity]);
        }

        private VocabularyValue GetSosId(string val,
            IDictionary<object, int> sosIdByValue,
            int? defaultValue = null,
            MappingNotFoundLogic mappingNotFoundLogic = MappingNotFoundLogic.UseSourceValue)
        {
            if (string.IsNullOrWhiteSpace(val) || sosIdByValue == null)
            {
                return defaultValue.HasValue ? new VocabularyValue { Id = defaultValue.Value } : null!;
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

        private enum MappingNotFoundLogic
        {
            UseSourceValue,
            UseDefaultValue
        }        

        /// <summary>
        ///     Get vocabulary mappings.
        /// </summary>
        /// <param name="externalSystemId"></param>
        /// <param name="allVocabularies"></param>
        /// <param name="convertValuesToLowercase"></param>
        /// <returns></returns>
        public static IDictionary<VocabularyId, IDictionary<object, int>> GetVocabulariesDictionary(
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
    }
}