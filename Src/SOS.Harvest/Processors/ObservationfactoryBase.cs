using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using System.Reflection;
using System.Text.Json;

namespace SOS.Harvest.Processors
{
    /// <summary>
    /// Base class for observation factories
    /// </summary>
    public class ObservationFactoryBase : FactoryBase
    {
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

        private readonly IDictionary<int, HashSet<string>> _protectedTaxa;

        private string GetAreaKey(AreaType areatype, string? featureId) => $"{areatype}-{featureId}";

        private IDictionary<int, HashSet<string>> LoadTaxonProtection()
        {
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(assemblyPath!, @"Resources\TaxonProtection.json");
            using (var fs = FileSystemHelper.WaitForFile(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var taxonProtection = JsonSerializer.DeserializeAsync<IEnumerable<ProtectedTaxon>>(fs, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }).Result;
                return taxonProtection?.ToDictionary(tp => tp.TaxonId, tp => tp.Areas?.Select(a => GetAreaKey(a.AreaType, a.FeatureId)).ToHashSet() ?? new HashSet<string>()) ?? new Dictionary<int, HashSet<string>>();
            }
        }

        protected IDictionary<int, Lib.Models.Processed.Observation.Taxon> Taxa { get; }
        protected IDictionary<string, Lib.Models.Processed.Observation.Taxon> TaxaByName { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="processTimeManager"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected ObservationFactoryBase(DataProvider dataProvider, 
            IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa, 
            IProcessTimeManager processTimeManager, 
            ProcessConfiguration processConfiguration) : base(dataProvider, processTimeManager, processConfiguration)
        {
            Taxa = taxa ?? throw new ArgumentNullException(nameof(taxa));

            TaxaByName = new Dictionary<string, Lib.Models.Processed.Observation.Taxon>();
            foreach(var taxon in taxa.Values)
            {
                var taxonName = taxon.ScientificName.ToLower();
                if (!TaxaByName.ContainsKey(taxonName))
                {
                    TaxaByName.Add(taxonName, taxon);
                }
            }

            _protectedTaxa = LoadTaxonProtection();
        }
        
        /// <summary>
        /// Get taxon
        /// </summary>
        /// <param name="taxonId"></param>
        /// <returns></returns>
        protected Lib.Models.Processed.Observation.Taxon GetTaxon(int taxonId, IEnumerable<string> names = null!)
        {
            Taxa.TryGetValue(taxonId, out var taxon);

            // If we can't find taxon by id, try by scientific name if passed
            if (taxon == null && (names?.Any() ?? false))
            {
                foreach(var name in names)
                {
                    if(!string.IsNullOrEmpty(name) && TaxaByName.TryGetValue(name?.ToLower() ?? string.Empty, out taxon)){
                        break;
                    }
                }
            }

            return taxon ?? new Lib.Models.Processed.Observation.Taxon { Id = -1, VerbatimId = taxonId.ToString() };
        }

        /// <summary>
        /// Populate some generic data
        /// </summary>
        /// <param name="observation"></param>
        protected void PopulateGenericData(Observation observation)
        {
            if (observation.Event?.StartDate == null ||
                (observation.Taxon?.Id ?? 0) == 0 ||
                (observation.Location?.DecimalLatitude ?? 0) == 0 ||
                (observation.Location?.DecimalLongitude ?? 0) == 0)
            {
                return;
            }
            // Round coordinates to 5 decimals (roughly 1m)
            var source = $"{observation.Event.StartDate.Value.ToUniversalTime().ToString("s")}-{observation.Taxon.Id}-{Math.Round(observation.Location.DecimalLongitude.Value, 5)}/{Math.Round(observation.Location.DecimalLatitude.Value, 5)}";

            observation.DataQuality = new DataQuality
            {
                UniqueKey = source.ToHash()
            };

            if (observation?.Taxon?.Attributes?.ProtectedByLaw ?? false && observation.Location != null)
            {
                if (_protectedTaxa.TryGetValue(observation.Taxon.Id, out var areas))
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
                        observation.Taxon = observation.Taxon.Clone();
                        observation.Taxon.Attributes.ProtectedByLaw = false;
                    }
                }
            }
        }
    }
}
