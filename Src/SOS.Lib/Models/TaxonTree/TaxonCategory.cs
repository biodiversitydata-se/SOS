using System.Collections.Generic;

namespace SOS.Lib.Models.TaxonTree
{
    /// <summary>
    /// Taxon category.
    /// </summary>
    public class TaxonCategory
    {
        public int Id { get; set; }
        public string SwedishName { get; set; }
        public string EnglishName { get; set; }
        public string DwcName { get; set; }
        public HashSet<TaxonCategory> Parents { get; set; } = new HashSet<TaxonCategory>();
        public HashSet<TaxonCategory> Children { get; set; } = new HashSet<TaxonCategory>();
        public HashSet<TaxonCategory> MainParents { get; set; } = new HashSet<TaxonCategory>();
        public HashSet<TaxonCategory> MainChildren { get; set; } = new HashSet<TaxonCategory>();
        public HashSet<TaxonCategory> SecondaryParents { get; set; } = new HashSet<TaxonCategory>();
        public HashSet<TaxonCategory> SecondaryChildren { get; set; } = new HashSet<TaxonCategory>();


        public override bool Equals(object obj)
        {
            return obj is TaxonCategory category &&
                   Id == category.Id;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Id);
        }

        public override string ToString()
        {
            return $"{EnglishName} [{Id}], Parents={Parents.Count}, Children={Children.Count}";
        }
    }
}