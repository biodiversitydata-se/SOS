namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// Taxon properties record relation.
    /// </summary>    
    public class TaxonProperties<T>
    {
        public T TaxonId { get; set; }
        public string ScientificName { get; set; }
        public string VernacularName { get; set; }
        public int? SortOrder { get; set; }
        public int? TaxonCategoryId { get; set; }
        public string TaxonCategorySwedishName { get; set; }
        public string TaxonCategoryEnglishName { get; set; }
        public string TaxonCategoryDarwinCoreName { get; set; }
        public int? GbifTaxonId { get; set; }
        public string OrganismLabel1 { get; set; }
        public string OrganismLabel2 { get; set; }
        public int? ProtectionLevel { get; set; }
        public int? DisturbanceRadius { get; set; }
        public string IucnRedlistCategory { get; set; }
        public string IucnRedlistCategoryDerived { get; set; }
        public string RedlistCategory { get; set; }
        public bool BannedForReporting { get; set; }
        public bool ExcludeFromReportingSystem { get; set; }
        public string ActionPlan { get; set; }
        public bool BirdDirective { get; set; }
        public bool EuRegulation_1143_2014 { get; set; }
        public string RiskLista { get; set; }
        public string InvasiveRiskAssessmentCategory { get; set; }
        public bool IsInvasiveInSweden { get; set; }
        public bool Natura2000HabitatsDirectiveArticle2 { get; set; }
        public bool Natura2000HabitatsDirectiveArticle4 { get; set; }
        public bool Natura2000HabitatsDirectiveArticle5 { get; set; }
        public bool ProtectedByLaw { get; set; }
        public string SwedishOccurrence { get; set; }
        public string SwedishHistory { get; set; }
    }
}
