﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SOS.Lib.Models
{
    public class PropertyFieldDescription
    {
        public int Id { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
        public string ParentName { get; set; }
        public string DataType { get; set; }
        public bool? DataTypeNullable { get; set; }
        public bool IsClass { get; set; }
        [JsonIgnore]
        public List<PropertyFieldDescription> Children { get; set; }
        [JsonIgnore]
        public PropertyFieldDescription Parent { get; set; }
        public int? ParentId { get; set; }
        public string SwedishTitle { get; set; }
        public string EnglishTitle { get; set; }
        public string DwcName { get; set; }
        public string DwcIdentifier { get; set; }
        public string FlatPropertyName { get; set; }
        public bool? IsPartOfFlatObservation { get; set; }
        public string DependsOn { get; set; }
        public string FieldSet { get; set; }
        public string Comment { get; set; }
        public PropertyFieldDataType DataTypeEnum { get; set; }
        public string GetSwedishTitle()
        {
            return string.IsNullOrEmpty(SwedishTitle) ? ShortName : SwedishTitle;
        }
        public string GetEnglishTitle()
        {
            return string.IsNullOrEmpty(EnglishTitle) ? ShortName : EnglishTitle;
        }
        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(DataType)}: {DataType}, {nameof(IsClass)}: {IsClass}, #{nameof(Children)}: {Children?.Count ?? 0}";
        }
    }
}