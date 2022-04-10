using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Lib.Extensions
{
    /// <summary>
    ///     Extensions for Darwin Core
    /// </summary>
    public static class DarwinCoreTaxonExtensions
    {
        private static IDictionary<int, VocabularyValue> _protectionLevelCache = new ConcurrentDictionary<int, VocabularyValue>();

        public static IEnumerable<Taxon> ToProcessedTaxa(this IEnumerable<DarwinCoreTaxon> sourceTaxa)
        {
            return sourceTaxa?.Select(t => t.ToProcessedTaxon());
        }

        public static Taxon ToProcessedTaxon(this DarwinCoreTaxon sourceTaxon)
        {
            return new Taxon
            {
                SecondaryParentDyntaxaTaxonIds = sourceTaxon.DynamicProperties?.SecondaryParentDyntaxaTaxonIds,
                AcceptedNameUsage = sourceTaxon.AcceptedNameUsage,
                AcceptedNameUsageId = sourceTaxon.AcceptedNameUsageID,
                BirdDirective = sourceTaxon.DynamicProperties?.BirdDirective,
                Class = sourceTaxon.Class,
                Family = sourceTaxon.Family,
                Genus = sourceTaxon.Genus,
                HigherClassification = sourceTaxon.HigherClassification,
                Id = sourceTaxon.Id,
                InfraspecificEpithet = sourceTaxon.InfraspecificEpithet,
                Kingdom = sourceTaxon.Kingdom?.Clean(),
                NameAccordingTo = sourceTaxon.NameAccordingTo,
                NameAccordingToId = sourceTaxon.NameAccordingToID,
                NamePublishedIn = sourceTaxon.NamePublishedIn,
                NamePublishedInId = sourceTaxon.NamePublishedInID,
                NamePublishedInYear = sourceTaxon.NamePublishedInYear,
                NomenclaturalCode = sourceTaxon.NomenclaturalCode,
                NomenclaturalStatus = sourceTaxon.NomenclaturalStatus,
                Order = sourceTaxon.Order,
                OriginalNameUsage = sourceTaxon.OriginalNameUsage,
                OriginalNameUsageId = sourceTaxon.OriginalNameUsageID,
                ParentNameUsage = sourceTaxon.ParentNameUsage,
                ParentNameUsageId = sourceTaxon.ParentNameUsageID,
                Phylum = sourceTaxon.Phylum,
                ScientificName = sourceTaxon.ScientificName,
                ScientificNameAuthorship = sourceTaxon.ScientificNameAuthorship,
                ScientificNameId = sourceTaxon.ScientificNameID,
                SpecificEpithet = sourceTaxon.SpecificEpithet,
                Subgenus = sourceTaxon.Subgenus,
                TaxonConceptId = sourceTaxon.TaxonConceptID,
                Attributes = new TaxonAttributes
                {
                    ActionPlan = sourceTaxon.DynamicProperties?.ActionPlan,
                    DisturbanceRadius = sourceTaxon.DynamicProperties?.DisturbanceRadius,
                    DyntaxaTaxonId = sourceTaxon.DynamicProperties?.DyntaxaTaxonId ?? 0, 
                    IsEURegulation_1143_2014 = sourceTaxon.DynamicProperties?.IsEURegulation_1143_2014 ?? false,
                    Natura2000HabitatsDirectiveArticle2 =
                        sourceTaxon.DynamicProperties?.Natura2000HabitatsDirectiveArticle2,
                    Natura2000HabitatsDirectiveArticle4 =
                        sourceTaxon.DynamicProperties?.Natura2000HabitatsDirectiveArticle4,
                    Natura2000HabitatsDirectiveArticle5 =
                        sourceTaxon.DynamicProperties?.Natura2000HabitatsDirectiveArticle5,
                    OrganismGroup = sourceTaxon.DynamicProperties?.OrganismGroup,
                    ParentDyntaxaTaxonId = sourceTaxon.DynamicProperties?.ParentDyntaxaTaxonId,
                    ProtectionLevel = sourceTaxon.DynamicProperties?.ProtectionLevel.ToProtectionLevel(),
                    SensitivityCategory = sourceTaxon.DynamicProperties?.ProtectionLevel.ToProtectionLevel(),
                    ProtectedByLaw = sourceTaxon.DynamicProperties?.ProtectedByLaw ?? false,
                    RedlistCategory = sourceTaxon.DynamicProperties?.RedlistCategory,
                    SortOrder = sourceTaxon.SortOrder,
                    SwedishHistory = sourceTaxon.DynamicProperties?.SwedishHistory,
                    SwedishHistoryCategory = sourceTaxon.DynamicProperties?.SwedishHistoryCategory,
                    SwedishOccurrence = sourceTaxon.DynamicProperties?.SwedishOccurrence,
                    Synonyms = sourceTaxon.Synonyms?.ToTaxonSynonymNames(),
                    TaxonCategory = VocabularyValue.Create(sourceTaxon.DynamicProperties?.TaxonCategoryId),
                    VernacularNames = sourceTaxon.VernacularNames?.ToTaxonVernacularNames()
                },
                TaxonId = sourceTaxon.TaxonID,
                TaxonRank = sourceTaxon.TaxonRank,
                TaxonRemarks = sourceTaxon.TaxonRemarks?.Clean(),
                TaxonomicStatus = sourceTaxon.TaxonomicStatus,
                VernacularName = sourceTaxon.VernacularName,
                VerbatimTaxonRank = sourceTaxon.VerbatimTaxonRank,
               
            };
        }

        /// <summary>
        ///     Cast DarwinCoreVernacularNames to TaxonVernacularNames.
        /// </summary>
        /// <param name="darwinCoreVernacularNames"></param>
        /// <returns></returns>
        private static IEnumerable<TaxonVernacularName> ToTaxonVernacularNames(
            this IEnumerable<DarwinCoreVernacularName> darwinCoreVernacularNames)
        {
            return darwinCoreVernacularNames?.Select(m => m.ToTaxonVernacularName());
        }

        /// <summary>
        ///     Cast DarwinCoreVernacularName object to TaxonVernacularName.
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

        /// <summary>
        ///     Cast DarwinCoreSynonymNames to TaxonSynonymNames.
        /// </summary>
        /// <param name="synonyms"></param>
        /// <returns></returns>
        private static IEnumerable<TaxonSynonymName> ToTaxonSynonymNames(
            this IEnumerable<DarwinCoreSynonymName> synonyms)
        {
            return synonyms?.Select(m => m.ToTaxonSynonymName());
        }

        /// <summary>
        ///     Cast DarwinCoreSynonymName object to TaxonSynonymName.
        /// </summary>
        /// <param name="synonym"></param>
        /// <returns></returns>
        private static TaxonSynonymName ToTaxonSynonymName(this DarwinCoreSynonymName synonym)
        {
            return new TaxonSynonymName()
            {
                Name = synonym.ScientificName?.Clean(),
                Author = synonym.ScientificNameAuthorship?.Clean(),
                NomenclaturalStatus = synonym.NomenclaturalStatus,
                TaxonomicStatus = synonym.TaxonomicStatus,
                //NameId = synonyme.NameId, // probably not needed
                //Remarks = synonyme.TaxonRemarks // probably not needed
            };
        }

        /// <summary>
        /// Try to parse string as protection level object
        /// </summary>
        /// <param name="protectionLevelString"></param>
        /// <returns></returns>
        private static VocabularyValue ToProtectionLevel(
            this string protectionLevelString)
        {
            if (string.IsNullOrEmpty(protectionLevelString))
            {
                return null;
            }

            var regex = new Regex(@"^\d");
            if (!int.TryParse(regex.Match(protectionLevelString).Value, out var protectionLevelId))
            {
                return null;
            }

            if (!_protectionLevelCache.TryGetValue(protectionLevelId, out var protectionLevel))
            {
                regex = new Regex(@"(?<=\.)(.*?)((?=\.)|$)");
                protectionLevel = new VocabularyValue
                {
                    Id = protectionLevelId,
                    Value = regex.Match(protectionLevelString).Value?.Trim()
                };

                _protectionLevelCache.Add(protectionLevelId, protectionLevel);
            }

            return protectionLevel;
        }
    }
}