﻿using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Search.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace SOS.Lib.Extensions
{
    /// <summary>
    /// Search filter extensions
    /// </summary>
    public static class SearchFilterExtensions
    {
        private static JsonSerializerOptions _serializeOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };

        /// <summary>
        /// Get filter as json string.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static string GetFilterAsJson(this SearchFilterBase filter)
        {
            try
            {
                return JsonSerializer.Serialize(filter, _serializeOptions);
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Populate output fields based on property set
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="outputFieldSet"></param>
        public static void PopulateFields(this OutputFilter filter, OutputFieldSet? outputFieldSet)
        {
            if (filter?.Fields?.Any() == true && outputFieldSet == null) return;

            const OutputFieldSet defaultFieldSet = OutputFieldSet.Minimum;
            var fieldSet = outputFieldSet ?? defaultFieldSet;
            if (fieldSet == OutputFieldSet.AllWithValues || fieldSet == OutputFieldSet.All)
            {
                filter.Fields = null;
                return;
            }

            var propertyFieldsDependencySet =
                ObservationPropertyFieldDescriptionHelper.JsonFormatDependencyByFieldSet[fieldSet];
            List<string> outputFields = propertyFieldsDependencySet.ToList();

            if (filter.Fields?.Any() ?? false)
            {
                outputFields.AddRange(filter.Fields.Where(of => !outputFields.Contains(of, StringComparer.CurrentCultureIgnoreCase)));
            }
            
            filter.Fields = outputFields;                        
        }
    }
}
