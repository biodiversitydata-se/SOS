using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Import.Factories.Harvest.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Verbatim.Shark;

namespace SOS.Import.Factories.Harvest
{
    public class SharkHarvestFactory : IHarvestFactory<SharkJsonFile, SharkObservationVerbatim>
    {
        /// <inheritdoc />
        public async Task<IEnumerable<SharkObservationVerbatim>> CastEntitiesToVerbatimsAsync(SharkJsonFile fileData, bool incrementalHarvest = false)
        {
            return await Task.Run(() =>
            {
                if ((!fileData?.Header?.Any() ?? true) || (!fileData?.Rows?.Any() ?? true))
                {
                    return null;
                }

                var header = fileData.Header.ToArray();
                var propertyMapping = new Dictionary<string, int>();
                for (var i = 0; i < header.Length; i++)
                {
                    propertyMapping.Add(header[i].Replace("_", "").ToLower(), i);
                }

                // If file don't contains taxon id there's no reason to go on 
                if (!propertyMapping.ContainsKey("dyntaxaid"))
                {
                    return null;
                }

                return from r in fileData.Rows
                    select CastEntityToVerbatim(r.ToArray(), propertyMapping);
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="rowData"></param>
        /// <param name="propertyMapping"></param>
        /// <returns></returns>
        private SharkObservationVerbatim CastEntityToVerbatim(IReadOnlyList<string> rowData,
            IDictionary<string, int> propertyMapping)
        {
            var observation = new SharkObservationVerbatim();
            foreach (var propertyName in propertyMapping)
            {
                if (string.IsNullOrEmpty(rowData[propertyName.Value]))
                {
                    continue;
                }

                observation.SetProperty(propertyName.Key, rowData[propertyName.Value]);
            }

            return observation;
        }
    }
}
