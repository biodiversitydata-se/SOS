using System.Collections.Generic;
using Nest;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// Taxon attributes 
    /// </summary>
    public class TaxonAttributes 
    {
        /// <summary>
        ///     Action plan
        /// </summary>
        public string ActionPlan { get; set; }

        /// <summary>
        ///     Radius of disturbance
        /// </summary>
        public int? DisturbanceRadius { get; set; }

        /// <inheritdoc />
        public int DyntaxaTaxonId { get; set; }

        /// <summary>
        ///     part of Habitats directive article 2
        /// </summary>
        public bool? Natura2000HabitatsDirectiveArticle2 { get; set; }

        /// <summary>
        ///     part of Habitats directive article 2
        /// </summary>
        public bool? Natura2000HabitatsDirectiveArticle4 { get; set; }

        /// <summary>
        ///     part of Habitats directive article 2
        /// </summary>
        public bool? Natura2000HabitatsDirectiveArticle5 { get; set; }

        /// <summary>
        ///     Organism group
        /// </summary>
        public string OrganismGroup { get; set; }

        /// <inheritdoc />
        public int? ParentDyntaxaTaxonId { get; set; }

        /// <summary>
        ///     True if taxon is protected by law
        /// </summary>
        public bool? ProtectedByLaw { get; set; }

        /// <summary>
        ///     True if taxon is protected by law
        /// </summary>
        public VocabularyValue ProtectionLevel { get; set; }

        /// <summary>
        ///     Redlist category
        /// </summary>
        public string RedlistCategory { get; set; }

        /// <summary>
        ///     Systematic sort order
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        ///     Do taxon occur in sweden
        /// </summary>
        public string SwedishHistory { get; set; }

        /// <summary>
        ///     Do taxon occur in sweden
        /// </summary>
        public string SwedishOccurrence { get; set; }

        /// <summary>
        ///     Synonyme names.
        /// </summary>
        [Nested]
        public IEnumerable<TaxonSynonymeName> Synonyms { get; set; }

        /// <summary>
        ///     Vernacular names.
        /// </summary>
        [Nested]
        public IEnumerable<TaxonVernacularName> VernacularNames { get; set; }
    }
}
