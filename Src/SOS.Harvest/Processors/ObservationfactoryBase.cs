using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Harvest.Managers.Interfaces;

namespace SOS.Harvest.Processors
{
    /// <summary>
    /// Base class for observation factories
    /// </summary>
    public class ObservationFactoryBase : FactoryBase
    {
        protected IDictionary<int, Lib.Models.Processed.Observation.Taxon> Taxa { get; }
        protected IDictionary<string, Lib.Models.Processed.Observation.Taxon> TaxaByName { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        /// <param name="taxa"></param>
        /// <param name="processTimeManager"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected ObservationFactoryBase(DataProvider dataProvider, IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa, IProcessTimeManager processTimeManager) : base(dataProvider, processTimeManager)
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
        }
    }
}
