using System.Collections.Generic;
using System.Text.Json.Serialization;
using SOS.Lib.Enums;

namespace SOS.Lib.Models
{
    public class PropertyFieldDescription
    {
        public string PropertyName { get; set; }
        public string PropertyPath { get; set; }
        public string SwedishTitle { get; set; }
        public string EnglishTitle { get; set; }
        public string DataType { get; set; }
        public bool? DataTypeIsNullable { get; set; }
        public string DwcName { get; set; }
        public string DwcIdentifier { get; set; }
        public string DependsOn { get; set; }
        public string FieldSet { get; set; }
        [JsonIgnore]
        public OutputFieldSet FieldSetEnum { get; set; }
        [JsonIgnore]
        public List<OutputFieldSet> FieldSets { get; set; }
        public string Comment { get; set; }
        public PropertyFieldDataType DataTypeEnum { get; set; }
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