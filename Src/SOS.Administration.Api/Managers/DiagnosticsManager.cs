using Microsoft.Extensions.Logging;
using SOS.Administration.Api.Managers.Interfaces;
using SOS.Administration.Api.Models.Ipt;
using SOS.Lib.Models.Diagnostics;
using SOS.Lib.Repositories.Resource.Interfaces;
using SOS.Lib.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace SOS.Administration.Api.Managers;

/// <summary>
/// Diagnostics manager
/// </summary>
public class DiagnosticsManager
{
    private readonly ITaxonRepository _taxonRepository;
    private readonly ILogger<DiagnosticsManager> _logger;

    public DiagnosticsManager(ITaxonRepository taxonRepository, ILogger<DiagnosticsManager> logger)
    {
        _taxonRepository = taxonRepository;
        _logger = logger;
    }

    public async Task<string> CreateBasicTaxaSummaryJsonAsync(string collectionName)
    {
        var collection = _taxonRepository.GetMongoCollection(collectionName);
        var allTaxa = await _taxonRepository.GetAllAsync(collection);
        var taxonSummary = new TaxonSummary();
        foreach (var taxon in allTaxa)
        {
            taxonSummary.TaxonIds.Add(taxon.Id);
            if (taxon.Attributes.IsRedlisted) taxonSummary.IsRedlisted.Add(taxon.Id);
            if (taxon.Attributes.IsInvasiveInSweden) taxonSummary.IsInvasiveInSweden.Add(taxon.Id);
            if (taxon.Attributes.IsInvasiveAccordingToEuRegulation) taxonSummary.IsInvasiveInEu.Add(taxon.Id);
            if (taxon.Attributes.ProtectedByLaw) taxonSummary.IsProtectedByLaw.Add(taxon.Id);
            if (taxon.BirdDirective) taxonSummary.IsBirdDirective.Add(taxon.Id);
            taxonSummary.Taxa.Add(new TaxonSummaryItem()
            {
                Id = taxon.Id,
                ScientificName = taxon.ScientificName,
                ActionPlan = taxon.Attributes.ActionPlan,
                DisturbanceRadius = taxon.Attributes.DisturbanceRadius,
                GbifTaxonId = taxon.Attributes.GbifTaxonId,
                IsInvasiveAccordingToEuRegulation = taxon.Attributes.IsInvasiveAccordingToEuRegulation,
                IsInvasiveInSweden = taxon.Attributes.IsInvasiveInSweden,
                InvasiveRiskAssessmentCategory = taxon.Attributes.InvasiveRiskAssessmentCategory,
                IsRedlisted = taxon.Attributes.IsRedlisted,
                Natura2000HabitatsDirectiveArticle2 = taxon.Attributes.Natura2000HabitatsDirectiveArticle2,
                Natura2000HabitatsDirectiveArticle4 = taxon.Attributes.Natura2000HabitatsDirectiveArticle4,
                Natura2000HabitatsDirectiveArticle5 = taxon.Attributes.Natura2000HabitatsDirectiveArticle5,
                OrganismGroup = taxon.Attributes.OrganismGroup,
                ProtectedByLaw = taxon.Attributes.ProtectedByLaw,
                RedlistCategory = taxon.Attributes.RedlistCategory,
                RedlistCategoryDerived = taxon.Attributes.RedlistCategoryDerived,
                SensitivityCategory = taxon.Attributes.SensitivityCategory != null ? taxon.Attributes.SensitivityCategory.Id : -1,
                SpeciesGroup = taxon.Attributes.SpeciesGroup,
                SwedishOccurrence = taxon.Attributes.SwedishOccurrence,
                SwedishHistory = taxon.Attributes.SwedishHistory,
                TaxonCategory = taxon.Attributes.TaxonCategory.Id
            });
        }

        taxonSummary.Taxa = taxonSummary.Taxa.OrderBy(m => m.Id).ToList();
        taxonSummary.TaxonIds = taxonSummary.TaxonIds.Order().ToList();
        taxonSummary.IsRedlisted = taxonSummary.IsRedlisted.Order().ToList();
        taxonSummary.IsInvasiveInSweden = taxonSummary.IsInvasiveInSweden.Order().ToList();
        taxonSummary.IsInvasiveInEu = taxonSummary.IsInvasiveInEu.Order().ToList();
        taxonSummary.IsProtectedByLaw = taxonSummary.IsProtectedByLaw.Order().ToList();
        taxonSummary.IsBirdDirective = taxonSummary.IsBirdDirective.Order().ToList();
        
        string json = JsonSerializer.Serialize(taxonSummary, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement), // Display å,ä,ö e.t.c. properly
        });

        return json;
    }        
}
