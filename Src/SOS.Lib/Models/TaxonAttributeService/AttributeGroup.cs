using System.Collections.Generic;

namespace SOS.Lib.Models.TaxonAttributeService
{
    public class AttributeGroup
    {
        /// <summary>
        /// Attributes
        /// </summary>
        public IEnumerable<Attribute> Attributes { get; set; }

        /// <summary>
        /// Attribute types
        /// </summary>
        public IEnumerable<AttributeType> AttributeTypes { get; set; }

        /// <summary>
        ///     Attribute group id
        /// </summary>
        public int Id { get; set; }
    }
}