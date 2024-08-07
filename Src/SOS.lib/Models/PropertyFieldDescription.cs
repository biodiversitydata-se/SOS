﻿using SOS.Lib.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace SOS.Lib.Models
{
    public class PropertyFieldDescription
    {
        /// <summary>
        /// Used for i.e. to handle projects
        /// </summary>
        public bool IsDynamicCreated { get; set; }
        /// <summary>
        /// Used for i.e. to handle project parameters
        /// </summary>
        public IEnumerable<int> DynamicIds { get; set; }

        public string PropertyName { get; set; }
        public string PropertyPath { get; set; }
        public string SwedishTitle { get; set; }
        public string EnglishTitle { get; set; }
        public string DataType { get; set; }
        public bool? DataTypeIsNullable { get; set; }
        public string DwcName { get; set; }
        public string DwcIdentifier { get; set; }
        public string DependsOn { get; set; }
        public string JsonFormatDependsOn { get; set; }
        public string FieldSet { get; set; }
        [JsonIgnore]
        public OutputFieldSet FieldSetEnum { get; set; }
        [JsonIgnore]
        public List<OutputFieldSet> FieldSets { get; set; }
        public string Comment { get; set; }
        public PropertyFieldDataType DataTypeEnum { get; set; }

        private string[] _jsonFormatDependsOn;
        public string[] GetJsonFormatDependsOn()
        {
            if (_jsonFormatDependsOn == null)
            {
                if (string.IsNullOrEmpty(JsonFormatDependsOn))
                {
                    _jsonFormatDependsOn = new[] { DependsOn };
                }
                else
                {
                    _jsonFormatDependsOn = JsonFormatDependsOn.Split(",").Select(m => m.Trim()).ToArray();
                }
            }

            return _jsonFormatDependsOn;
        }
        public string GetSwedishTitle()
        {
            return string.IsNullOrEmpty(SwedishTitle) ? PropertyName : SwedishTitle;
        }
        public string GetEnglishTitle()
        {
            return string.IsNullOrEmpty(EnglishTitle) ? PropertyName : EnglishTitle;
        }
        public override string ToString()
        {
            return $"{nameof(PropertyPath)}: {PropertyPath}, {nameof(DataType)}: {DataType}, {nameof(FieldSet)}: {FieldSet}";
        }
    }
}