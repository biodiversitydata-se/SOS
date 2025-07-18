﻿using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Constants;
using SOS.Lib.DataStructures;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using System.Reflection;
using System.Text.Json;

namespace SOS.Harvest.Processors
{
    /// <summary>
    /// Base class for observation factories
    /// </summary>
    public class ObservationFactoryBase : FactoryBase
    {
        public static int NumberOfTaxonClones = 0;
        protected IDictionary<int, Lib.Models.Processed.Observation.Taxon> Taxa { get; }
        protected IDictionary<VocabularyId, IDictionary<object, int>> VocabularyById { get; }

        private struct ProtectedArea
        {
            /// <summary>
            /// Type of area
            /// </summary>
            public AreaType AreaType { get; set; }

            /// <summary>
            /// Id of area
            /// </summary>
            public string? FeatureId { get; set; }
        }

        private struct ProtectedTaxon
        {
            /// <summary>
            /// Areas taxon is protected in
            /// </summary>
            public IEnumerable<ProtectedArea>? Areas { get; set; }

            /// <summary>
            /// Id of protected taxon
            /// </summary>
            public int TaxonId { get; set; }
        }

        private readonly IDictionary<int, HashSet<string>> _taxaProtectedByLaw;
        private HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon> _taxonByScientificName { get; }
        private HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon> _taxonByScientificNameAuthor { get; }
        private HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon> _taxonBySynonymName { get; }
        private HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon> _taxonBySynonymNameAuthor { get; }

        private string GetAreaKey(AreaType areatype, string? featureId) => $"{areatype}-{featureId}";

        private Lib.Models.Processed.Observation.Taxon GetTaxonByName(string name, string? scientificNameAuthorship, bool ignoreDuplicates, out string? nameWithoutAuthor)
        {
            nameWithoutAuthor = null;
            if (string.IsNullOrEmpty(name)) return null!;
            name = name.ToLower();

            // Get by scientific name
            if (_taxonByScientificName.TryGetValues(name, out var taxa))
            {
                if (taxa.Count == 1 || ignoreDuplicates)
                {
                    return taxa.First();
                }
            }

            // Get by scientific name + author
            if (_taxonByScientificNameAuthor.TryGetValues(name, out taxa))
            {
                if (taxa.Count == 1 || ignoreDuplicates)
                {
                    return taxa.First();
                }
            }

            // Get by scientific name - author            
            if (!string.IsNullOrEmpty(scientificNameAuthorship))
            {
                nameWithoutAuthor = name.Replace(scientificNameAuthorship, "", StringComparison.InvariantCultureIgnoreCase).Trim();                
            }            
            if (nameWithoutAuthor != null)
            {
                if (_taxonByScientificName.TryGetValues(nameWithoutAuthor, out taxa))
                {
                    if (taxa.Count == 1 || ignoreDuplicates)
                    {
                        return taxa.First();
                    }
                }
            }

            // Get by synonyme
            if (_taxonBySynonymName.TryGetValues(name, out taxa))
            {
                if (taxa.Count == 1 || ignoreDuplicates)
                {
                    return taxa.First();
                }
            }

            // Get by synonyme + author
            if (_taxonBySynonymNameAuthor.TryGetValues(name, out taxa))
            {
                if (taxa.Count == 1 || ignoreDuplicates)
                {
                    return taxa.First();
                }
            }

            // Get by synonyme - author            
            if (nameWithoutAuthor != null)
            {
                if (_taxonBySynonymName.TryGetValues(nameWithoutAuthor, out taxa))
                {
                    if (taxa.Count == 1 || ignoreDuplicates)
                    {
                        return taxa.First();
                    }
                }
            }

            return null!;
        }

        private IDictionary<int, HashSet<string>> LoadTaxonProtectedByLaw()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath!, @"Resources/TaxonProtectedByLaw.json");
            using (var fs = FileSystemHelper.WaitForFile(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var taxonProtection = JsonSerializer.DeserializeAsync<IEnumerable<ProtectedTaxon>>(fs, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }).Result;
                return taxonProtection?.ToDictionary(tp => tp.TaxonId, tp => tp.Areas?.Select(a => GetAreaKey(a.AreaType, a.FeatureId)).ToHashSet() ?? new HashSet<string>()) ?? new Dictionary<int, HashSet<string>>();
            }
        }        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="processTimeManager"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected ObservationFactoryBase(DataProvider dataProvider,
            IDictionary<int, Lib.Models.Processed.Observation.Taxon>? taxa,
            IDictionary<VocabularyId, IDictionary<object, int>> vocabularyById,
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration) : base(dataProvider, processTimeManager, processConfiguration)
        {
            Taxa = taxa ?? throw new ArgumentNullException(nameof(taxa));
            VocabularyById = vocabularyById ?? throw new ArgumentNullException(nameof(vocabularyById));
            _taxonByScientificName = new HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon>();
            _taxonByScientificNameAuthor = new HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon>();
            _taxonBySynonymName = new HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon>();
            _taxonBySynonymNameAuthor = new HashMapDictionary<string, Lib.Models.Processed.Observation.Taxon>();

            foreach (var processedTaxon in taxa.Values)
            {
                _taxonByScientificName.Add(processedTaxon.ScientificName.ToLower(), processedTaxon);
                if (!string.IsNullOrEmpty(processedTaxon.ScientificNameAuthorship))
                {
                    _taxonByScientificNameAuthor.Add(processedTaxon.ScientificName.ToLower() + " " + processedTaxon.ScientificNameAuthorship.ToLower(), processedTaxon);
                }
                if (processedTaxon.Attributes?.Synonyms != null)
                {
                    foreach (var synonyme in processedTaxon.Attributes.Synonyms.Where(m => m.TaxonomicStatus != "misapplied"))
                    {
                        _taxonBySynonymName.Add(synonyme.Name.ToLower(), processedTaxon);
                        _taxonBySynonymNameAuthor.Add(synonyme.Name.ToLower() + " " + synonyme.Author.ToLower(), processedTaxon);
                    }
                }
            }

            _taxaProtectedByLaw = LoadTaxonProtectedByLaw();
        }

        /// <summary>
        ///  Get taxon
        /// </summary>
        /// <param name="taxonId"></param>
        /// <param name="names"></param>
        /// <param name="ignoreDuplicates">If false and taxon not is found by id and name search finds more than one match, no taxon is returned</param>
        /// <param name="verbatimId"></param>
        /// <param name="verbatimName"></param>
        /// <returns></returns>
        protected Lib.Models.Processed.Observation.Taxon GetTaxon(
            int taxonId, 
            IEnumerable<string> names = null!, 
            string? scientificNameAuthorship = null,
            bool ignoreDuplicates = false, 
            string? verbatimId = null, 
            string? verbatimName = null)
        {
            string? nameWithoutAuthor = null;
            Lib.Models.Processed.Observation.Taxon? taxon = null;
            var taxonFound = taxonId < 0 ? false : Taxa.TryGetValue(taxonId, out taxon);
            if ((!taxonFound || taxonId == 0) && (names?.Any() ?? false))
            {
                // If we can't find taxon by id or taxon id is 0 (biota), try by name/s if passed
                foreach (var name in names)
                {
                    taxon = GetTaxonByName(name, scientificNameAuthorship, ignoreDuplicates, out nameWithoutAuthor);
                    if (taxon != null)
                    {
                        break;
                    }
                }
            }

            if (taxon != null)
            {
                if (!string.IsNullOrEmpty(verbatimId) || !string.IsNullOrEmpty(verbatimName))
                {
                    taxon = taxon.Clone();
                    taxon.VerbatimId = verbatimId ?? taxonId.ToString();
                    taxon.VerbatimName = verbatimName ?? names?.FirstOrDefault(n => !string.IsNullOrEmpty(n));
                    NumberOfTaxonClones++;
                }

                return taxon;
            }

            List<string> verbatimNames = new List<string>();
            if (!string.IsNullOrEmpty(verbatimName))
                verbatimNames.Add(verbatimName);
            if (!string.IsNullOrEmpty(nameWithoutAuthor))
                verbatimNames.Add(nameWithoutAuthor);
            var firstName = names?.FirstOrDefault(n => !string.IsNullOrEmpty(n));
            if (!string.IsNullOrEmpty(firstName))
                verbatimNames.Add(firstName);            

            return new Lib.Models.Processed.Observation.Taxon
            {
                Id = -1,
                VerbatimId = verbatimId ?? taxonId.ToString(),
                VerbatimName = verbatimNames.Count == 0 ? null : string.Join(", ", verbatimNames.Select(n => $"'{n}'"))
            };
        }

        /// <summary>
        /// Populate some generic data
        /// </summary>
        /// <param name="observation"></param>
        protected void PopulateGenericData(Observation observation)
        {
            if (observation?.Event?.StartDate == null ||
                (observation?.Taxon?.Id ?? 0) == 0 ||
                (observation?.Location?.DecimalLatitude ?? 0) == 0 ||
                (observation?.Location?.DecimalLongitude ?? 0) == 0)
            {
                return;
            }
            // Round coordinates to 5 decimals (roughly 1m)
            var source = $"{observation!.Event.StartDate.Value.ToUniversalTime().ToString("s")}-{observation.Taxon.Id}-{Math.Round(observation!.Location!.DecimalLongitude!.Value, 5)}/{Math.Round(observation!.Location!.DecimalLatitude!.Value, 5)}";

            observation.DataQuality = new DataQuality
            {
                UniqueKey = source.ToHash()
            };

            if (observation?.Taxon?.Attributes?.ProtectedByLaw ?? false && observation.Location != null)
            {
                if (_taxaProtectedByLaw.TryGetValue(observation.Taxon.Id, out var areas))
                {
                    if (!(
                        areas.Contains(GetAreaKey(AreaType.CountryRegion, observation.Location.CountryRegion?.FeatureId)) ||
                        areas.Contains(GetAreaKey(AreaType.County, observation.Location.County?.FeatureId)) ||
                        areas.Contains(GetAreaKey(AreaType.Province, observation.Location.Province?.FeatureId)) ||
                        areas.Contains(GetAreaKey(AreaType.Municipality, observation.Location.Municipality?.FeatureId)) ||
                        areas.Contains(GetAreaKey(AreaType.Parish, observation.Location.Parish?.FeatureId))
                        )
                    )
                    {
                        // observation.taxon should already have been cloned to a unique object
                        //observation.Taxon = observation.Taxon.Clone();
                        observation.Taxon.Attributes.ProtectedByLaw = false;
                    }
                }
            }
        }

        /// <summary>
        /// Calculate organism quantity
        /// </summary>
        /// <param name="verbatimObservation"></param>
        /// <returns></returns>
        protected void CalculateOrganismQuantity(Observation observation)
        {
            if (observation.Occurrence.OrganismQuantityInt.HasValue)
            {
                observation.Occurrence.OrganismQuantityAggregation = observation.Occurrence.OrganismQuantityInt;
                return;                
            }

            if (int.TryParse(observation.Occurrence.OrganismQuantity, out var quantity))
            {
                observation.Occurrence.OrganismQuantityAggregation = quantity;
                observation.Occurrence.OrganismQuantityInt = quantity;
                return;
            }
            else if (int.TryParse(observation.Occurrence.IndividualCount, out var individualCount))
            {
                observation.Occurrence.OrganismQuantityAggregation = individualCount;                
                return;
            }

            if (observation.Occurrence.IsPositiveObservation)
            {
                observation.Occurrence.OrganismQuantityAggregation = 1;                
                return;
            }

            observation.Occurrence.OrganismQuantityAggregation = 0;            
        }

        protected int GetBirdNestActivityId(VocabularyValue? activity, Lib.Models.Processed.Observation.Taxon? taxon)
        {
            if (taxon?.IsBird() ?? false)
            {
                return (activity?.Id.Equals(VocabularyConstants.NoMappingFoundCustomValueIsUsedId) ?? true) ? 1000000 : activity.Id;
            }

            return 0;
        }

        protected VocabularyValue? GetSosId(string? val,            
            IDictionary<object, int>? sosIdByValue,            
            int? defaultValue = null,
            MappingNotFoundLogic mappingNotFoundLogic = MappingNotFoundLogic.UseSourceValue,
            string? lookupVal = null)
        {
            if (string.IsNullOrWhiteSpace(val) || sosIdByValue == null)
            {
                return defaultValue.HasValue ? new VocabularyValue { Id = defaultValue.Value } : null;
            }

            if (lookupVal == null)
            {
                lookupVal = val.ToLower();
            }
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

        protected enum MappingNotFoundLogic
        {
            UseSourceValue,
            UseDefaultValue
        }
    }
}
