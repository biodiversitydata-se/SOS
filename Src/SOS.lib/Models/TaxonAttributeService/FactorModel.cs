using System.Collections.Generic;

namespace SOS.Lib.Models.TaxonAttributeService
{
    public class FactorModel
    {
        /// <summary>
        ///     Attributes property
        /// </summary>
        public IEnumerable<AttributeValueModel> Attributes { get; set; }

        /// <summary>
        ///     Current quality
        /// </summary>
        public string Quality { get; set; }

        /// <summary>
        ///     Current Reference ids
        /// </summary>
        public IEnumerable<int> ReferenceIds { get; set; }

        /// <summary>
        ///     Factor id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     Factor name
        /// </summary>
        public string Name { get; set; }
    }
}