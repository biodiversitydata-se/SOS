using System;
using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Search;

namespace SOS.Lib.Extensions
{
    /// <summary>
    /// Search filter extensions
    /// </summary>
    public static class SearchFilterExtensions
    {
        /// <summary>
        /// Populate output fields based on property set
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="outputFieldSet"></param>
        public static void PopulateOutputFields(this SearchFilter filter, OutputFieldSet? outputFieldSet)
        {
            if (filter.OutputFields?.Any() == true && outputFieldSet == null) return;
            const OutputFieldSet defaultFieldSet = OutputFieldSet.Minimum;
            var fieldSet = outputFieldSet ?? defaultFieldSet;
            if (fieldSet == OutputFieldSet.AllWithValues || fieldSet == OutputFieldSet.All)
            {
                filter.OutputFields = null;
                return;
            }

            var propertyFieldsDependencySet =
                ObservationPropertyFieldDescriptionHelper.JsonFormatDependencyByFieldSet[fieldSet];
            List<string> outputFields = propertyFieldsDependencySet.ToList();

            if (filter.OutputFields?.Any() ?? false)
            {
                outputFields.AddRange(filter.OutputFields.Where(of => !outputFields.Contains(of, StringComparer.CurrentCultureIgnoreCase)));
            }
           
            filter.OutputFields = outputFields;
        }
    }
}
