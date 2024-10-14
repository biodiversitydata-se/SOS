﻿namespace SOS.Lib.Models.TaxonAttributeService
{
    /// <summary>
    ///     Taxon attribute model
    /// </summary>
    public class TaxonAttributeValue
    {
        /// <summary>
        ///     Attribute property
        /// </summary>
        public Attribute AttributeInfo { get; set; }

        /// <summary>
        ///    Value
        /// </summary>
        public string Value { get; set; }
    }
}