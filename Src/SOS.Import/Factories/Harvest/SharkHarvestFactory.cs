using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SOS.Import.Factories.Harvest.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Verbatim.Shark;

namespace SOS.Import.Factories.Harvest
{
    public class SharkHarvestFactory : HarvestBaseFactory, IHarvestFactory<SharkJsonFile, SharkObservationVerbatim>
    {
        private static readonly Encoding Utf8Encoder = Encoding.GetEncoding(
            "UTF-8",
            new EncoderReplacementFallback(string.Empty),
            new DecoderExceptionFallback()
        );

        /// <summary>
        /// </summary>
        /// <param name="rowData"></param>
        /// <param name="propertyMapping"></param>
        /// <returns></returns>
        private SharkObservationVerbatim CastEntityToVerbatim(IReadOnlyList<string> rowData,
            IDictionary<string, int> propertyMapping)
        {
            if (propertyMapping.TryGetValue("value", out var valueIndex))
            {
                if (!double.TryParse(rowData[valueIndex], NumberStyles.Number, CultureInfo.InvariantCulture, out var value) || value == 0)
                {
                    return null;
                }

                var observation = new SharkObservationVerbatim
                {
                    Id = NextId
                };
                foreach (var propertyName in propertyMapping)
                {
                    var propertyValue = Utf8Encoder.GetString(Utf8Encoder.GetBytes(rowData[propertyName.Value]));

                    if (string.IsNullOrEmpty(propertyValue))
                    {
                        continue;
                    }

                    observation.SetProperty(propertyName.Key, propertyValue);
                }

                return observation;
            }

            return null;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<SharkObservationVerbatim>> CastEntitiesToVerbatimsAsync(SharkJsonFile fileData)
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

                var verbatims = from r in fileData.Rows
                    select CastEntityToVerbatim(r.ToArray(), propertyMapping);

                return from v in verbatims where v != null select v;
            });
        }
    }
}
