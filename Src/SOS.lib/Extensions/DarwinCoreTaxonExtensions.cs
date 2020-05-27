using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Extensions
{
    /// <summary>
    /// Extensions for Darwin Core
    /// </summary>
    public static class DarwinCoreTaxonExtensions
    {
        public static ProcessedTaxon ToProcessedTaxon(this DarwinCoreTaxon sourceTaxon)
        {
            return new ProcessedTaxon
            {
                DyntaxaTaxonId = sourceTaxon.DynamicProperties?.DyntaxaTaxonId ?? 0,
                ParentDyntaxaTaxonId = sourceTaxon.DynamicProperties?.ParentDyntaxaTaxonId,
                SecondaryParentDyntaxaTaxonIds = sourceTaxon.DynamicProperties?.SecondaryParentDyntaxaTaxonIds,
                VernacularNames = sourceTaxon.VernacularNames?.ToTaxonVernacularNames(),
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
                SortOrder = sourceTaxon.SortOrder
            };
        }

        public static ProcessedBasicTaxon ToProcessedBasicTaxon(this DarwinCoreTaxon sourceTaxon)
        {
            return new ProcessedBasicTaxon()
            {
                DyntaxaTaxonId = sourceTaxon.DynamicProperties.DyntaxaTaxonId,
                ParentDyntaxaTaxonId = sourceTaxon.DynamicProperties.ParentDyntaxaTaxonId,
                SecondaryParentDyntaxaTaxonIds = sourceTaxon.DynamicProperties.SecondaryParentDyntaxaTaxonIds,
                Id = sourceTaxon.Id,
                ScientificName = sourceTaxon.ScientificName
            };
        }

        /// <summary>
        /// Cast DarwinCoreVernacularNames to TaxonVernacularNames.
        /// </summary>
        /// <param name="darwinCoreVernacularNames"></param>
        /// <returns></returns>
        private static IEnumerable<TaxonVernacularName> ToTaxonVernacularNames(this IEnumerable<DarwinCoreVernacularName> darwinCoreVernacularNames)
        {
            return darwinCoreVernacularNames?.Select(m => m.ToTaxonVernacularName());
        }

        /// <summary>
        /// Cast DarwinCoreVernacularName object to TaxonVernacularName.
        /// </summary>
        /// <param name="darwinCoreVernacularName"></param>
        /// <returns></returns>
        private static TaxonVernacularName ToTaxonVernacularName(this DarwinCoreVernacularName darwinCoreVernacularName)
        {
            return new TaxonVernacularName
            {
                CountryCode = darwinCoreVernacularName.CountryCode,
                IsPreferredName = darwinCoreVernacularName.IsPreferredName,
                Language = darwinCoreVernacularName.Language,
                Name = darwinCoreVernacularName.VernacularName
            };
        }
    }
}
