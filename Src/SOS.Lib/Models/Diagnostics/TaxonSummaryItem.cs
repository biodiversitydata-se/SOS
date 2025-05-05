using SOS.Lib.Enums;

namespace SOS.Lib.Models.Diagnostics;

public class TaxonSummaryItem
{
    public int Id { get; set; }
    public string ScientificName { get; set; }    
    public string ActionPlan { get; set; }    
    public int? DisturbanceRadius { get; set; }
    public int? GbifTaxonId { get; set; }
    public bool IsInvasiveAccordingToEuRegulation { get; set; }    
    public bool IsInvasiveInSweden { get; set; }
    public string InvasiveRiskAssessmentCategory { get; set; }    
    public bool IsRedlisted { get; set; }
    public bool Natura2000HabitatsDirectiveArticle2 { get; set; }
    public bool Natura2000HabitatsDirectiveArticle4 { get; set; }
    public bool Natura2000HabitatsDirectiveArticle5 { get; set; }
    public string OrganismGroup { get; set; }
    public bool ProtectedByLaw { get; set; }   
    public string RedlistCategory { get; set; }
    public string RedlistCategoryDerived { get; set; }
    public int SensitivityCategory { get; set; }    
    public SpeciesGroup SpeciesGroup { get; set; }
    public string SwedishOccurrence { get; set; }
    public string SwedishHistory { get; set; }    
    public int TaxonCategory { get; set; }    
}
