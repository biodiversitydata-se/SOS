using System.Collections.Generic;
using SOS.Lib.Enums;
using SOS.Lib.Models;

namespace SOS.Observations.Api.Dtos.Vocabulary
{
    /// <summary>
    /// Information about a property field.
    /// </summary>
    public class PropertyFieldDescriptionDto
    {
        /// <summary>
        /// Property name.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Property path.
        /// </summary>
        public string PropertyPath { get; set; }
        
        /// <summary>
        /// Swedish title.
        /// </summary>
        public string SwedishTitle { get; set; }
        
        /// <summary>
        /// English title.
        /// </summary>
        public string EnglishTitle { get; set; }
        
        /// <summary>
        /// Darwin Core name.
        /// </summary>
        public string DwcName { get; set; }
        
        /// <summary>
        /// Darwin Core identifier.
        /// </summary>
        public string DwcIdentifier { get; set; }
        
        /// <summary>
        /// Data type.
        /// </summary>
        public PropertyFieldDataType DataType { get; set; }
        
        /// <summary>
        /// Indicates whether the data type is nullable.
        /// </summary>
        public bool DataTypeNullable { get; set; }
        
        /// <summary>
        /// The min field set this property is part of.
        /// </summary>
        public OutputFieldSet FieldSet { get; set; }
        
        /// <summary>
        /// The field sets this property is part of.
        /// </summary>
        public List<OutputFieldSet> PartOfFieldSets { get; set; }
    }
}