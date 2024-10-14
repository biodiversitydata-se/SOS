using System.Collections.Generic;

namespace SOS.Lib.Models.TaxonAttributeService
{
    public class AttributeType
    {
        /// <summary>
        ///     Attribute type id
        /// </summary>
        public int AttributeTypeId { get; set; }

        /// <summary>
        /// Attribute type
        /// </summary>
        public string BaseType { get; set; }

        /// <summary>
        /// Enumeration values for base type "Enum"
        /// </summary>
        public IEnumerable<Enumeration> Enumerations { get; set; }
    }
}