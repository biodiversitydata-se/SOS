using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using SOS.Lib.Models.Verbatim.Shark;

namespace SOS.Import.Extensions
{
    public static class SharkObservationExtensions
    {
        /// <summary>
        /// Cast shark file data to verbatims
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

            return fileData.Rows.Select(r => r.ToArray().ToVerbatim(propertyMapping));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowData"></param>
        /// <param name="propertyMapping"></param>
        /// <returns></returns>
        private static SharkObservationVerbatim ToVerbatim(this IReadOnlyList<string> rowData, IDictionary<string, int> propertyMapping)
        {
            var observation = new SharkObservationVerbatim();
            var observationType = observation.GetType();
            foreach (var propertyName in propertyMapping)
            {
                if (string.IsNullOrEmpty(rowData[propertyName.Value]))
                {
                    continue;
                }

                var property = observationType.GetProperty(propertyName.Key,
                    BindingFlags.SetProperty | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (property == null)
                {
                    continue;
                }

                var propertyType = property.PropertyType;
                var underlyingType = Nullable.GetUnderlyingType(propertyType);

                property.SetValue(observation,
                    Convert.ChangeType(rowData[propertyName.Value], underlyingType ?? propertyType,
                        CultureInfo.InvariantCulture));
            }
            return observation;
        }
    }
}
