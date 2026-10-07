using SOS.Lib.Helpers;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Lib.Models.Search.Filters;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace SOS.Lib.Extensions;

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

    extension(SearchFilterBase filter)
    {
        /// <summary>
        /// Get filter as json string.
        /// </summary>
        /// <returns></returns>
        public string GetFilterAsJson()
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
    }

    extension(OutputFilter filter)
    {
        /// <summary>
        /// Populate output fields based on property set
        /// </summary>
        /// <param name="outputFieldSet"></param>
        public void PopulateFields(OutputFieldSet? outputFieldSet)
        {
            if (filter?.Fields?.Any() == true && outputFieldSet == null) return;

            const OutputFieldSet defaultFieldSet = OutputFieldSet.Minimum;
            var fieldSet = outputFieldSet ?? defaultFieldSet;
            if (fieldSet == OutputFieldSet.None)
            {
                // None adds no predefined fields. With no selected fields, keep
                // the matching records but exclude their entire source content.
                if (filter.Fields?.Any() != true) filter.ExcludeFields = ["*"];
                return;
            }
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
