using  SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Sighting;

namespace SOS.Process.Extensions
{
    public static class TaxonExtensions
    {
        public static ProcessedTaxon ToProcessed(this DarwinCoreTaxon sourceTaxon)
        {
            return new ProcessedTaxon
            {
                DyntaxaTaxonId = sourceTaxon.DynamicProperties.DyntaxaTaxonId, 
                ParentDyntaxaTaxonId = sourceTaxon.DynamicProperties.ParentDyntaxaTaxonId,
                SecondaryParentDyntaxaTaxonIds = sourceTaxon.DynamicProperties.SecondaryParentDyntaxaTaxonIds, 
                VernacularNames = sourceTaxon.DynamicProperties.VernacularNames,
                AcceptedNameUsage = sourceTaxon.AcceptedNameUsage,
                AcceptedNameUsageID = sourceTaxon.AcceptedNameUsageID,
                ActionPlan = sourceTaxon.DynamicProperties?.ActionPlan,
                BirdDirective = sourceTaxon.DynamicProperties?.BirdDirective,
                Class = sourceTaxon.Class,
                DisturbanceRadius = sourceTaxon.DynamicProperties?.DisturbanceRadius,
                Family = sourceTaxon.Family,
                Genus = sourceTaxon.Genus,
                HigherClassification = sourceTaxon.HigherClassification,
                Id = sourceTaxon.Id,
                InfraspecificEpithet = sourceTaxon.InfraspecificEpithet,
                Kingdom = sourceTaxon.Kingdom,
                NameAccordingTo = sourceTaxon.NameAccordingTo,
                NameAccordingToID = sourceTaxon.NameAccordingToID,
                NamePublishedIn = sourceTaxon.NamePublishedIn,
                NamePublishedInId = sourceTaxon.NamePublishedInID,
                NamePublishedInYear = sourceTaxon.NamePublishedInYear,
                Natura2000HabitatsDirectiveArticle2 = sourceTaxon.DynamicProperties?.Natura2000HabitatsDirectiveArticle2,
                Natura2000HabitatsDirectiveArticle4 = sourceTaxon.DynamicProperties?.Natura2000HabitatsDirectiveArticle4,
                Natura2000HabitatsDirectiveArticle5 = sourceTaxon.DynamicProperties?.Natura2000HabitatsDirectiveArticle5,
                NomenclaturalCode = sourceTaxon.NomenclaturalCode,
                NomenclaturalStatus = sourceTaxon.NomenclaturalStatus,
                Order = sourceTaxon.Order,
                OrganismGroup = sourceTaxon.DynamicProperties?.OrganismGroup,
                OriginalNameUsage = sourceTaxon.OriginalNameUsage,
                OriginalNameUsageId = sourceTaxon.OriginalNameUsageID,
                ParentNameUsage = sourceTaxon.ParentNameUsage,
                ParentNameUsageId = sourceTaxon.ParentNameUsageID,
                Phylum = sourceTaxon.Phylum,
                ProtectionLevel = sourceTaxon.DynamicProperties?.ProtectionLevel,
                ProtectedByLaw = sourceTaxon.DynamicProperties?.ProtectedByLaw,
                RedlistCategory = sourceTaxon.DynamicProperties?.RedlistCategory,
                ScientificName = sourceTaxon.ScientificName,
                ScientificNameAuthorship = sourceTaxon.ScientificNameAuthorship,
                ScientificNameId = sourceTaxon.ScientificNameID,
                SpecificEpithet = sourceTaxon.SpecificEpithet,
                Subgenus = sourceTaxon.Subgenus,
                SwedishHistory = sourceTaxon.DynamicProperties?.SwedishHistory,
                SwedishOccurrence = sourceTaxon.DynamicProperties?.SwedishOccurrence,
                TaxonConceptId = sourceTaxon.TaxonConceptID,
                TaxonId = sourceTaxon.TaxonID,
                TaxonRank = sourceTaxon.TaxonRank,
                TaxonRemarks = sourceTaxon.TaxonRemarks,
                TaxonomicStatus = sourceTaxon.TaxonomicStatus,
                VernacularName = sourceTaxon.VernacularName,
                VerbatimTaxonRank = sourceTaxon.VerbatimTaxonRank,
            };
        }
    }
}
