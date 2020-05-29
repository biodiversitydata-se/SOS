using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Models.Verbatim.Shark;

namespace SOS.Import.Extensions
{
    public static class SharkObservationExtensions
    {
        /// <summary>
        ///     Cast shark file data to verbatims
        /// </summary>
        /// <param name="fileData"></param>
        /// <returns></returns>
        public static IEnumerable<SharkObservationVerbatim> ToVerbatims(this SharkJsonFile fileData)
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

            return fileData.Rows.Select(r => r.ToArray().ToVerbatim(propertyMapping));
        }

        /// <summary>
        /// </summary>
        /// <param name="rowData"></param>
        /// <param name="propertyMapping"></param>
        /// <returns></returns>
        private static SharkObservationVerbatim ToVerbatim(this IReadOnlyList<string> rowData,
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