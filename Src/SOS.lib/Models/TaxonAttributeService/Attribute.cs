namespace SOS.Lib.Models.TaxonAttributeService
{
    public class Attribute
    {
        /// <summary>
        ///     Attribute group id
        /// </summary>
        public int AttributeGroupId { get; set; }

        /// <summary>
        /// Attribute type id
        /// </summary>
        public int AttributeTypeId { get; set; }

        /// <summary>
        /// Comp field idx
        /// </summary>
        public int CompFieldIdx { get; set; }

        /// <summary>
        ///     Is mainfield
        /// </summary>
        public bool IsMainField { get; set; }
    }
}