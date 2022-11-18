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
        private static IDictionary<int, VocabularyValue> _protectionLevelCache =
            new ConcurrentDictionary<int, VocabularyValue>();

        private static HashSet<string> _isInvasiveInSwedenCategories = new HashSet<string>() { "5", "7", "8", "9" };

        /// <summary>
        /// Update red list category derivied
        /// </summary>
        /// <param name="taxa"></param>
        private static IDictionary<int, Taxon> PopulateDeriviedRedListCategory(IDictionary<int, Taxon> taxa)
        {
            if (!taxa?.Any() ?? true)
            {
                return null;
            }

            foreach (var taxon in taxa.Values)
            {
                // If taxon lacks red list category or it's NE - Not Evaluated, try to get parent red list category
                if (string.IsNullOrEmpty(taxon.Attributes.RedlistCategory) || taxon.Attributes.RedlistCategory == "NE")
                {
                    taxa.TryGetValue(taxon.Attributes.ParentDyntaxaTaxonId ?? -1, out var parentTaxon);
                    while (parentTaxon != null)
                    {
                        // If parent is evaluated, get it's red list category
                        if (!string.IsNullOrEmpty(parentTaxon.Attributes?.RedlistCategory) && parentTaxon.Attributes?.RedlistCategory != "NE")
                        {
                            taxon.Attributes.RedlistCategoryDerived = parentTaxon.Attributes.RedlistCategory;
                            break;
                        }

                        taxa.TryGetValue(parentTaxon.Attributes.ParentDyntaxaTaxonId ?? -1, out parentTaxon);
                    }
                }
                else
                {
                    // If taxon is evaluated or no parent evaluation was found, Set derivied rlc to current 
                    taxon.Attributes.RedlistCategoryDerived = taxon.Attributes.RedlistCategory;
                }
            }

            return taxa;
        }

        public static IDictionary<int, Taxon> ToProcessedTaxa(this IEnumerable<DarwinCoreTaxon> sourceTaxa)
        {
            var taxa = sourceTaxa?.Select(t => t.ToProcessedTaxon());
            return PopulateDeriviedRedListCategory(taxa?.ToDictionary(t => t.Id, t => t));
        }

        public static Taxon ToProcessedTaxon(this DarwinCoreTaxon sourceTaxon)
        {
            var taxon = new Taxon();
            taxon.SecondaryParentDyntaxaTaxonIds = sourceTaxon.DynamicProperties?.SecondaryParentDyntaxaTaxonIds;
            taxon.AcceptedNameUsage = sourceTaxon.AcceptedNameUsage;
            taxon.AcceptedNameUsageId = sourceTaxon.AcceptedNameUsageID;
            taxon.BirdDirective = sourceTaxon.DynamicProperties?.BirdDirective == null ? false : sourceTaxon.DynamicProperties.BirdDirective.GetValueOrDefault();
            taxon.Class = sourceTaxon.Class;
            taxon.Family = sourceTaxon.Family;
            taxon.Genus = sourceTaxon.Genus;
            taxon.HigherClassification = sourceTaxon.HigherClassification;
            taxon.Id = sourceTaxon.Id;
            taxon.InfraspecificEpithet = sourceTaxon.InfraspecificEpithet;
            taxon.Kingdom = sourceTaxon.Kingdom?.Clean();
            taxon.NameAccordingTo = sourceTaxon.NameAccordingTo;
            taxon.NameAccordingToId = sourceTaxon.NameAccordingToID;
            taxon.NamePublishedIn = sourceTaxon.NamePublishedIn;
            taxon.NamePublishedInId = sourceTaxon.NamePublishedInID;
            taxon.NamePublishedInYear = sourceTaxon.NamePublishedInYear;
            taxon.NomenclaturalCode = sourceTaxon.NomenclaturalCode;
            taxon.NomenclaturalStatus = sourceTaxon.NomenclaturalStatus;
            taxon.Order = sourceTaxon.Order;
            taxon.OriginalNameUsage = sourceTaxon.OriginalNameUsage;
            taxon.OriginalNameUsageId = sourceTaxon.OriginalNameUsageID;
            taxon.ParentNameUsage = sourceTaxon.ParentNameUsage;
            taxon.ParentNameUsageId = sourceTaxon.ParentNameUsageID;
            taxon.Phylum = sourceTaxon.Phylum;
            taxon.ScientificName = sourceTaxon.ScientificName;
            taxon.ScientificNameAuthorship = sourceTaxon.ScientificNameAuthorship;
            taxon.ScientificNameId = sourceTaxon.ScientificNameID;
            taxon.SpecificEpithet = sourceTaxon.SpecificEpithet;
            taxon.Subgenus = sourceTaxon.Subgenus;
            taxon.TaxonConceptId = sourceTaxon.TaxonConceptID;
            taxon.Attributes = new TaxonAttributes();
            taxon.Attributes.ActionPlan = sourceTaxon.DynamicProperties?.ActionPlan;
            taxon.Attributes.DisturbanceRadius = sourceTaxon.DynamicProperties?.DisturbanceRadius;
            taxon.Attributes.DyntaxaTaxonId = sourceTaxon.DynamicProperties?.DyntaxaTaxonId ?? 0;
            taxon.Attributes.Natura2000HabitatsDirectiveArticle2 = sourceTaxon.DynamicProperties?.Natura2000HabitatsDirectiveArticle2 == null ? false : sourceTaxon.DynamicProperties.Natura2000HabitatsDirectiveArticle2.GetValueOrDefault();
            taxon.Attributes.Natura2000HabitatsDirectiveArticle4 = sourceTaxon.DynamicProperties?.Natura2000HabitatsDirectiveArticle4 == null ? false : sourceTaxon.DynamicProperties.Natura2000HabitatsDirectiveArticle4.GetValueOrDefault();
            taxon.Attributes.Natura2000HabitatsDirectiveArticle5 = sourceTaxon.DynamicProperties?.Natura2000HabitatsDirectiveArticle5 == null ? false : sourceTaxon.DynamicProperties.Natura2000HabitatsDirectiveArticle5.GetValueOrDefault();
            taxon.Attributes.OrganismGroup = sourceTaxon.DynamicProperties?.OrganismGroup;
            taxon.Attributes.ParentDyntaxaTaxonId = sourceTaxon.DynamicProperties?.ParentDyntaxaTaxonId;
            taxon.Attributes.ProtectionLevel = sourceTaxon.DynamicProperties?.ProtectionLevel.ToProtectionLevel();
            taxon.Attributes.SensitivityCategory = sourceTaxon.DynamicProperties?.ProtectionLevel.ToProtectionLevel();
            taxon.Attributes.ProtectedByLaw = sourceTaxon.DynamicProperties?.ProtectedByLaw ?? false;
            taxon.Attributes.IsInvasiveAccordingToEuRegulation = sourceTaxon.DynamicProperties?.IsEURegulation_1143_2014 ?? false;
            taxon.Attributes.IsInvasiveInSweden = _isInvasiveInSwedenCategories.Contains(sourceTaxon.DynamicProperties?.SwedishHistoryId ?? string.Empty);
            taxon.Attributes.InvasiveRiskAssessmentCategory = sourceTaxon.DynamicProperties?.SwedishHistoryCategory?.Substring(0, 2);            
            taxon.Attributes.RedlistCategory = sourceTaxon.DynamicProperties?.RedlistCategory?.Substring(0, 2);
            taxon.Attributes.SortOrder = sourceTaxon.SortOrder;
            taxon.Attributes.SwedishHistory = sourceTaxon.DynamicProperties?.SwedishHistory;
            taxon.Attributes.SwedishOccurrence = sourceTaxon.DynamicProperties?.SwedishOccurrence;
            taxon.Attributes.Synonyms = sourceTaxon.Synonyms?.ToTaxonSynonymNames();
            taxon.Attributes.TaxonCategory = VocabularyValue.Create(sourceTaxon.DynamicProperties?.TaxonCategoryId);
            taxon.Attributes.VernacularNames = sourceTaxon.VernacularNames?.ToTaxonVernacularNames();
            taxon.TaxonId = sourceTaxon.TaxonID;
            taxon.TaxonRank = sourceTaxon.TaxonRank;
            taxon.TaxonRemarks = sourceTaxon.TaxonRemarks?.Clean();
            taxon.TaxonomicStatus = sourceTaxon.TaxonomicStatus;
            taxon.VernacularName = sourceTaxon.VernacularName;
            taxon.VerbatimTaxonRank = sourceTaxon.VerbatimTaxonRank;
            return taxon;
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
                Name = darwinCoreVernacularName.VernacularName,
                ValidForSighting = darwinCoreVernacularName.ValidForSighting
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