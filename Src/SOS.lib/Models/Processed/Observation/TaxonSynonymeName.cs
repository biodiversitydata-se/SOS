namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// Taxon synonym.
    /// </summary>
    public class TaxonSynonymeName
    {
        /// <summary>
        /// Name.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Author.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        ///     The status of the use of the scientificName as a label
        ///     for a taxon. Requires taxonomic opinion to define the
        ///     scope of a taxon. Rules of priority then are used to
        ///     define the taxonomic status of the nomenclature contained
        ///     in that scope, combined with the experts opinion.
        ///     It must be linked to a specific taxonomic reference that
        ///     defines the concept.
        ///     Recommended best practice is to use a controlled vocabulary.
        /// </summary>
        public string TaxonomicStatus { get; set; }

        /// <summary>
        ///     The status related to the original publication of the
        ///     name and its conformance to the relevant rules of
        ///     nomenclature. It is based essentially on an algorithm
        ///     according to the business rules of the code.
        ///     It requires no taxonomic opinion.
        /// </summary>
        public string NomenclaturalStatus { get; set; }
    }
}