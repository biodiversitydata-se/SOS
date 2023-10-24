namespace SOS.Lib.Models.DarwinCore
{
    /// <summary>
    /// </summary>
    public class DarwinCoreVernacularName
    {
        /// <summary>
        ///     Country Code
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        ///     TaxonId
        /// </summary>
        public string TaxonID { get; set; }

        public string TaxonNameID { get; set; }

        /// <summary>
        ///     Preferred name
        /// </summary>
        public bool IsPreferredName { get; set; }

        /// <summary>
        ///     Language property
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        ///     Name source
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        ///     Taxon remarks
        /// </summary>
        public string TaxonRemarks { get; set; }

        /// <summary>
        /// Valid for sighting
        /// </summary>
        public bool ValidForSighting { get; set; }

        /// <summary>
        ///     Vernacular Name
        /// </summary>
        public string Name { get; set; }
        public string NameCategory { get; set; }
        public string Author { get; set; }
        public bool IsOkForObsSystems { get; set; }
        public int NameCategoryId { get; set; }
        public int NameStatusTypeId { get; set; }
    }
}