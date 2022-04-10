using System.Collections.Generic;

namespace SOS.Lib.Models.DarwinCore
{
    /// <summary>
    ///     Taxon dynamic properties.
    /// </summary>
    public class TaxonDynamicProperties
    {
        /// <summary>
        ///     Dyntaxa taxon id.
        /// </summary>
        public int DyntaxaTaxonId { get; set; }

        /// <summary>
        ///     Main parent Dyntaxa taxon id.
        /// </summary>
        public int? ParentDyntaxaTaxonId { get; set; }

        /// <summary>
        ///     Secondary parents dyntaxa taxon ids.
        /// </summary>
        public IEnumerable<int> SecondaryParentDyntaxaTaxonIds { get; set; }

        /// <summary>
        ///     Action plan
        /// </summary>
        public string ActionPlan { get; set; }

        /// <summary>
        ///     Part of bird directive?
        /// </summary>
        public bool? BirdDirective { get; set; }

        /// <summary>
        ///     Radius of disturbance
        /// </summary>
        public int DisturbanceRadius { get; set; }

        /// <summary>
        /// True if alien in sweden according to EU Regulation 1143/2014
        /// </summary>
        public bool IsEURegulation_1143_2014 { get; set; }

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

        /// <summary>
        ///     True if taxon is protected by law according to Artskyddsförordningen (SFS 2007:845)
        /// </summary>
        public bool? ProtectedByLawSpeciesProtection { get; set; }

        /// <summary>
        ///     True if taxon is protected by law and is a bird.
        /// </summary>
        public bool? ProtectedByLawBirds { get; set; }

        /// <summary>
        ///     True if taxon is protected by law according to Artskyddsförordningen (SFS 2007:845)
        /// </summary>
        public bool? ProtectedByLaw
        {
            get
            {
                if (ProtectedByLawSpeciesProtection == null && ProtectedByLawBirds == null) return null;
                return ProtectedByLawSpeciesProtection.GetValueOrDefault() || ProtectedByLawBirds.GetValueOrDefault();
            }
        }

        /// <summary>
        ///     True if taxon is protected by law
        /// </summary>
        public string ProtectionLevel { get; set; }

        /// <summary>
        ///     Redlist category
        /// </summary>
        public string RedlistCategory { get; set; }

        /// <summary>
        ///     Do taxon occur in sweden
        /// </summary>
        public string SwedishHistory { get; set; }

        /// <summary>
        /// Category if alien in Sweden
        /// </summary>
        public string SwedishHistoryCategory { get; set; }
       
        /// <summary>
        ///     Do taxon occur in sweden
        /// </summary>
        public string SwedishOccurrence { get; set; }

        /// <summary>
        /// Dyntaxa taxon category id.
        /// </summary>
        public int? TaxonCategoryId { get; set; }
        
        /// <summary>
        /// Dyntaxa taxon category swedish name.
        /// </summary>
        public string TaxonCategorySwedishName { get; set; }

        /// <summary>
        /// Dyntaxa taxon category english name.
        /// </summary>
        public string TaxonCategoryEnglishName { get; set; }
        
        /// <summary>
        /// Darwin Core taxon category name.
        /// </summary>
        public string TaxonCategoryDarwinCoreName { get; set; }
    }
}