namespace SOS.Import.Models.TaxonAttributeService
{
    public class AttributeValueModel
    {
        /// <summary>
        ///     Attribute id
        /// </summary>
        public int AttributeId { get; set; }

        /// <summary>
        ///     Attribute name
        /// </summary>
        public string Attribute { get; set; }

        /// <summary>
        ///     Compfield index
        /// </summary>
        public int CompFieldIdx { get; set; }

        /// <summary>
        ///     Is mainfield
        /// </summary>
        public bool IsMainField { get; set; }

        /// <summary>
        ///     Current value
        /// </summary>
        public string Value { get; set; }
    }
}