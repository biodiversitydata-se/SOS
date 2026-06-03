using Aspose.Html.Toolkit.Markdown.Syntax;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Dtos.Enum;
using SOS.Lib.Dtos.Filter;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using System.Text.Json;
using static SOS.Lib.Dtos.Filter.ExtendedFilterDto;

namespace SOS.Lib.Helpers;

public class FilterHelper : IFilterHelper
{
    private readonly IAreaCache _areaCache;
    private readonly IDataProviderCache _dataProviderCache;
    private readonly IDatasetRepository _datasetRepository;
    private readonly ICache<int, ProjectInfo> _projectCache;
    private readonly ICache<int, Taxon> _taxonCache;
    private readonly ICache<int, TaxonList> _taxonListCache;
    private readonly IVocabularyRepository _vocabularyRepository;
    private readonly ILogger<FilterHelper> _logger;

    /// <summary>
    /// Adds an ATX-style header to the specified Markdown syntax tree at the given heading level.
    /// </summary>
    /// <remarks>This method appends a new header node as a child of the provided Markdown syntax tree. The
    /// level parameter should correspond to standard Markdown heading levels. Supplying a value outside the range of 1
    /// to 6 may result in invalid Markdown output.</remarks>
    /// <param name="md">The MarkdownSyntaxTree instance to which the header will be added.</param>
    /// <param name="text">The text content to use for the header.</param>
    /// <param name="level">The heading level for the header. Must be an integer between 1 and 6, where 1 represents the largest header.</param>
    private void AddHeader(MarkdownSyntaxTree md, string text, int level)
    {
        var mdf = md.SyntaxFactory;
        var header = mdf.AtxHeading(text, level);
        header.GetTrailingTrivia().Add(mdf.NewLineTrivia());
        md.AppendChild(header);
    }

    /// <summary>
    /// Attempts to add an unordered list of items to the specified Markdown syntax tree under the given header.
    /// </summary>
    /// <remarks>If listData is null or contains no items, the method does not modify the Markdown syntax
    /// tree.</remarks>
    /// <param name="md">The MarkdownSyntaxTree instance to which the list will be added.</param>
    /// <param name="header">A tuple containing the header text and its level, specifying where the list should be inserted.</param>
    /// <param name="listData">An optional collection of strings representing the items to include in the list. If null or empty, no list is
    /// added.</param>
    private void TryAddList(MarkdownSyntaxTree md, (string Text, int Level) header, IEnumerable<string>? listData)
    {
        if ((listData?.Count() ?? 0) == 0)
        {
            return;
        }
        var mdf = md.SyntaxFactory;

        AddHeader(md, header.Text, header.Level);

        var list = mdf.UnorderedList();
        foreach (var item in listData)
        {
            var paragraph = mdf.Paragraph();
            paragraph.AppendChild(mdf.Text(item));
            var listItem = mdf.UnorderedListItem("-");
            listItem.AppendChild(paragraph);
            list.AppendChild(listItem);
        }
        md.AppendChild(list);

    }

    private void TryAddParagraph(MarkdownSyntaxTree md, string label, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }
        var mdf = md.SyntaxFactory;
        var paragraph = mdf.Paragraph();
        var bold = mdf.Emphasis(Emphasis.Strong);
        bold.AppendChild(mdf.Text($"{label}:"));
        paragraph.AppendChild(bold);
        paragraph.AppendChild(mdf.Whitespace());
        paragraph.AppendChild(mdf.Text($"{value}"));
        paragraph.AppendChild(mdf.Whitespace());
        paragraph.AppendChild(mdf.Whitespace());
        md.AppendChild(paragraph);
    }

    #region Filter

    private async Task AddExtendedInfoAsync(MarkdownSyntaxTree md, ExtendedFilterDto filter, IEnumerable<Vocabulary> vocabularies)
    {
        if (filter == null)
        {
            return;
        }
        AddHeader(md, "Extended Filter", 2);
        if (filter.ActivityIds?.Count() > 0)
        {
            var activities = vocabularies.FirstOrDefault(v => v.Id == VocabularyId.Activity)?.Values;
            if ((activities?.Count() ?? 0) == 0)
            {
                _logger.LogWarning("No activities found when adding info to markdown.");
            }
            else
            {
                TryAddList(md, ("Activities", 3), activities.Where(a => filter.ActivityIds.Contains(a.Id)).Select(a => a.Value));
            }
        }

        if (filter.BiotopeId.HasValue)
        {
            var biotopes = vocabularies.FirstOrDefault(v => v.Id == VocabularyId.Biotope)?.Values;
            if ((biotopes?.Count() ?? 0) == 0)
            {
                _logger.LogWarning("No biotopes found when adding info to markdown.");
            }
            else
            {
                TryAddParagraph(md, "Biotope", (biotopes.FirstOrDefault(b => b.Id == filter.BiotopeId)?.Value ?? filter.BiotopeId.ToString()));
            }
        }

        TryAddParagraph(md, "Checklist Id", filter.ChecklistId.HasValue && filter.ChecklistId.Value != 0 ? filter.ChecklistId.ToString() : null);
        TryAddList(md, ("Datasource Id´s", 3), filter.DatasourceIds?.Select(dsi => dsi.ToString()));

        if (filter.DiscoveryMethodIds?.Count() > 0)
        {
            var discoveryMethods = vocabularies.FirstOrDefault(v => v.Id == VocabularyId.DiscoveryMethod)?.Values;
            if ((discoveryMethods?.Count() ?? 0) == 0)
            {
                _logger.LogWarning("No discovery methods found when adding info to markdown.");
            }
            else
            {
                TryAddList(md, ("Discovery Methods", 3), discoveryMethods.Where(dmi => filter.DiscoveryMethodIds.Contains(dmi.Id)).Select(dmi => dmi.Value));
            }
        }

        if (filter.ExcludeVerificationStatusIds?.Count() > 0)
        {
            var verificationStatuses = vocabularies.FirstOrDefault(v => v.Id == VocabularyId.VerificationStatus)?.Values;
            if ((verificationStatuses?.Count() ?? 0) == 0)
            {
                _logger.LogWarning("No verification statuses found when adding info to markdown.");
            }
            else
            {
                TryAddList(md, ("Exclude Verification Status", 3), verificationStatuses.Where(vs => filter.ExcludeVerificationStatusIds.Contains(vs.Id)).Select(vs => vs.Value));
            }
        }

        TryAddList(md, ("Field DiaryGroup Id`s", 3), filter.FieldDiaryGroupIds?.Select(fdgi => fdgi.ToString()));
        TryAddParagraph(md, "Not Present Filter", filter.NotPresentFilter switch
        {
            SightingNotPresentFilterDto.DontIncludeNotPresent => "Don`t Include Not Present",
            SightingNotPresentFilterDto.IncludeNotPresent => "Include Not Present",
            SightingNotPresentFilterDto.OnlyNotPresent => "Only Not Present",
            _ => ""
        });
        TryAddParagraph(md, "Observed By User Id", filter.ObservedByUserId.HasValue && filter.ObservedByUserId.Value != 0 ? filter.ObservedByUserId.Value.ToString() : null);
        TryAddParagraph(md, "Observed By Userservice User Id", filter.ObservedByUserServiceUserId.HasValue && filter.ObservedByUserServiceUserId.Value != 0 ? filter.ObservedByUserServiceUserId.Value.ToString() : null);
        TryAddParagraph(md, "Only Secondhand Information", filter.OnlySecondHandInformation ? "Yes" : "No");
        TryAddParagraph(md, "Only With Barcode", filter.OnlyWithBarcode ? "Yes" : "No");
        TryAddParagraph(md, "Only With Media", filter.OnlyWithMedia ? "Yes" : "No");
        TryAddParagraph(md, "Only With Notes", filter.OnlyWithNotes ? "Yes" : "No");
        TryAddParagraph(md, "Only With Notes Of Interest", filter.OnlyWithNotesOfInterest ? "Yes" : "No");
        TryAddParagraph(md, "Only With User Comments", filter.OnlyWithUserComments ? "Yes" : "No");
        TryAddParagraph(md, "Private Collection", filter.PrivateCollection);
        TryAddParagraph(md, "Public Collection", filter.PublicCollection);
        TryAddList(md, ("Publish Type Id`s", 3), filter.PublishTypeIdsFilter?.Select(pti => pti.ToString()));

        if (filter.Quantity.HasValue)
        {
            TryAddParagraph(md, "Quantity", filter.Quantity.Value.ToString());
            TryAddParagraph(md, "Quantity", filter.QuantityOperator);
        }
        TryAddParagraph(md, "Reported By User Id", filter.ReportedByUserId.HasValue && filter.ReportedByUserId != 0 ? filter.ReportedByUserId.ToString() : null);
        TryAddParagraph(md, "Reported By Userservice User Id", filter.ReportedByUserServiceUserId.HasValue && filter.ReportedByUserServiceUserId.Value != 0 ? filter.ReportedByUserServiceUserId.Value.ToString() : null);
        TryAddParagraph(md, "Reported Date From", filter.ReportedDateFrom.HasValue ? filter.ReportedDateFrom.Value.ToString("yyyy-MM-dd HH:mm:ss") : null);
        TryAddParagraph(md, "Reported Date To", filter.ReportedDateTo.HasValue ? filter.ReportedDateTo.Value.ToString("yyyy-MM-dd HH:mm:ss") : null);
        if (filter.SexIds?.Count() > 0)
        {
            var sexes = vocabularies.FirstOrDefault(v => v.Id == VocabularyId.Sex)?.Values;
            if ((sexes?.Count() ?? 0) == 0)
            {
                _logger.LogWarning("No sexes found when adding info to markdown.");
            }
            else
            {
                TryAddList(md, ("Sexes", 3), sexes.Where(dmi => filter.SexIds.Contains(dmi.Id)).Select(dmi => dmi.Value));
            }
        }
        TryAddList(md, ("Site Id`s", 3), filter.SiteIds?.Select(s => s.ToString()));
        TryAddList(md, ("Site Project Id`s", 3), filter.SiteProjectIds?.Select(spi => spi.ToString()));
        TryAddParagraph(md, "Species Collection Label", filter.SpeciesCollectionLabel);
        TryAddList(md, ("Species Facts Id`s", 3), filter.SpeciesFactsIds?.Select(sfi => sfi.ToString()));

        if (filter.SubstrateId.HasValue)
        {
            var substrates = vocabularies.FirstOrDefault(v => v.Id == VocabularyId.Substrate)?.Values;
            if ((substrates?.Count() ?? 0) == 0)
            {
                _logger.LogWarning("No substrates found when adding info to markdown.");
            }
            else
            {
                TryAddParagraph(md, "Substrate", substrates.First(s => s.Id == filter.SubstrateId.Value).Value);
            }
        }

        if (filter.SubstrateSpeciesId.HasValue)
        {
            var taxon = await _taxonCache.GetAsync(filter.SubstrateSpeciesId.Value);
            if (taxon == null)
            {
                _logger.LogWarning($"Taxon with id {filter.SubstrateSpeciesId.Value} not found when adding info to markdown.");
            }
            else
            {
                TryAddParagraph(md, "Substrate Species", taxon?.ScientificName ?? filter.SubstrateSpeciesId.Value.ToString());
            }
        }
        TryAddList(md, ("Triggered Observation Rule Frequency Id`s", 3), filter.TriggeredObservationRuleFrequencyIds?.Select(torfi => torfi.ToString()));
        TryAddList(md, ("Triggered Observation Rule Reproduction Id`s", 3), filter.TriggeredObservationRuleReproductionIds?.Select(torri => torri.ToString()));
        TryAddParagraph(md, "Not Present Filter", filter.TypeFilter switch
        {
            SightingTypeFilterDto.DoNotShowMergedIncludeReplacementChilds => "Do Not Show Merged Include Replacement Childs",
            SightingTypeFilterDto.DoNotShowSightingsInMerged => "Do Not Show Sightings In Merged",
            SightingTypeFilterDto.ShowBoth => "Show Both",
            SightingTypeFilterDto.ShowChildrenAndReplacements => "Show Children And Replacements",
            SightingTypeFilterDto.ShowOnlyMerged => "Show Only Merged",
            _ => "Do Not Show Merged"
        });
        TryAddParagraph(md, "Not Present Filter", filter.UnspontaneousFilter switch
        {
            SightingUnspontaneousFilterDto.NotUnspontaneous => "Not Unspontaneous",
            SightingUnspontaneousFilterDto.Unspontaneous => "Unspontaneous",
            _ => ""
        });
        TryAddParagraph(md, "Use Period For All Years", filter.UsePeriodForAllYears ? "Yes" : "No");
        TryAddParagraph(md, "Use Period For All Years", filter.UsePeriodForAllYears ? "Yes" : "No");
        if (filter.VerificationStatusIds?.Count() > 0)
        {
            var verificationStatuses = vocabularies.FirstOrDefault(v => v.Id == VocabularyId.VerificationStatus)?.Values;
            if ((verificationStatuses?.Count() ?? 0) == 0)
            {
                _logger.LogWarning("No verification statuses found when adding info to markdown.");
            }
            else
            {
                TryAddList(md, ("Verification Status", 3), verificationStatuses.Where(vs => filter.VerificationStatusIds.Contains(vs.Id)).Select(vs => vs.Value));
            }
        }
        if (filter.Weight.HasValue)
        {
            TryAddParagraph(md, "Weight", filter.Weight.Value.ToString());
            TryAddParagraph(md, "Weight Operator", filter.WeightOperator);
        }
        if ((filter.Years?.Count() ?? 0) != 0)
        {
            TryAddList(md, ("Years", 3), filter.Years.Select(y => y.ToString()));
            TryAddParagraph(md, "Years Comparison", filter.YearsComparison switch
            {
                DateFilterComparisonDto.BothStartDateAndEndDate => "Both Start Date And End Date",
                DateFilterComparisonDto.EndDate => "End Date",
                DateFilterComparisonDto.StartDateEndDateMonthRange => "Start Date End Date Month Range",
                _ => "Start Date"
            });
        }
    }

    /// <summary>
    /// Attempts to add data stewardship information to the specified Markdown syntax tree using the provided filter
    /// criteria. Adds sections for dataset title and identifiers if the corresponding filters are present.
    /// </summary>
    /// <remarks>If neither the dataset title filter nor dataset identifiers are specified in the filter, the
    /// method does not modify the Markdown syntax tree.</remarks>
    /// <param name="md">The MarkdownSyntaxTree instance to which data stewardship information will be added.</param>
    /// <param name="filter">The DataStewardshipFilterDto containing filter criteria for dataset title and identifiers. If both are not
    /// provided, no changes are made.</param>
    private async Task TryAddDataStewardshipInfoAsync(MarkdownSyntaxTree md, DataStewardshipFilterDto filter)
    {
        if (string.IsNullOrEmpty(filter?.DatasetTitleFilter) && (filter?.DatasetIdentifiers?.Count() ?? 0) == 0)
        {
            return;
        }
        AddHeader(md, "Data Stewardship", 2);
        TryAddParagraph(md, "Title Filter", filter.DatasetTitleFilter);
        if (filter.DatasetIdentifiers?.Count() > 0)
        {
            var datasets = await _datasetRepository.GetDatasetsByIds(filter.DatasetIdentifiers);
            if ((datasets?.Count() ?? 0) == 0)
            {
                _logger.LogWarning("No datasets found when adding info to markdown.");
                return;
            }
            TryAddList(md, ("Identifiers", 3), datasets.Select(ds => ds.Title));
        }
    }

    private async Task TryAddDataProjectInfoAsync(MarkdownSyntaxTree md, IEnumerable<int> projectIds)
    {
        if ((projectIds?.Count() ?? 0) == 0)
        {
            return;
        }

        var projects = await _projectCache.GetAllAsync();
        if ((projects?.Count() ?? 0) == 0)
        {
            _logger.LogWarning("No projects found when adding info to markdown.");
            return;
        }
        TryAddList(md, ("Projects", 2), projects
            .Where(p => projectIds.Contains(p.Id))
            .Select(p => p.Name)
        );
    }

    /// <summary>
    /// Attempts to add data provider information to the specified Markdown syntax tree based on the data provider
    /// identifiers present in the given search filter.
    /// </summary>
    /// <remarks>If the filter does not specify any data provider identifiers, or if no matching data
    /// providers are found, the method logs a warning and does not modify the Markdown syntax tree.</remarks>
    /// <param name="md">The MarkdownSyntaxTree instance to which data provider information will be added.</param>
    /// <param name="filter">The SearchFilterBaseDto containing the data provider identifiers to process.</param>
    /// <returns>A task that represents the asynchronous operation of adding data provider information to the Markdown syntax
    /// tree.</returns>
    private async Task TryAddDataProviderInfoAsync(MarkdownSyntaxTree md, DataProviderFilterDto filter)
    {
        if ((filter?.Ids?.Count() ?? 0) == 0)
        {
            return;
        }
        var dataProviders = await _dataProviderCache.GetAllAsync();
        if ((dataProviders?.Count() ?? 0) == 0)
        {
            _logger.LogWarning("No data providers found when adding info to markdown.");
            return;
        }

        TryAddList(md, ("Data Providers", 2), dataProviders
            .Where(dp => filter.Ids.Contains(dp.Id))
            .Select(dp => dp.Identifier)
        );
    }

    /// <summary>
    /// Attempts to add event information to the specified Markdown syntax tree based on the provided filter criteria.
    /// </summary>
    /// <remarks>If the filter's Ids collection is null or empty, no event information will be added to the
    /// Markdown syntax tree.</remarks>
    /// <param name="md">The MarkdownSyntaxTree instance to which the event information will be added.</param>
    /// <param name="filter">The EventFilterDto containing the filter criteria, including a list of event IDs to be processed.</param>
    private void TryAddEventInfo(MarkdownSyntaxTree md, EventFilterDto filter)
    {
        if ((filter?.Ids?.Count() ?? 0) == 0)
        {
            return;
        }

        AddHeader(md, "Event", 2);
        var mdf = md.SyntaxFactory;

        TryAddList(md, ("Id´s", 3), filter.Ids);

    }

    /// <summary>
    /// Attempts to add exclusion information to the specified Markdown syntax tree based on the provided filter.
    /// </summary>
    /// <remarks>If the filter does not contain any occurrence IDs, no action is taken. The method adds a
    /// header and a list of occurrence IDs to the Markdown syntax tree.</remarks>
    /// <param name="md">The MarkdownSyntaxTree instance to which the exclusion information will be added.</param>
    /// <param name="filter">The ExcludeFilterDto containing the occurrence IDs to be excluded. Must not be null and should contain at least
    /// one occurrence ID.</param>
    private void TryAddExcludeInfo(MarkdownSyntaxTree md, ExcludeFilterDto filter)
    {
        if ((filter?.OccurrenceIds?.Count() ?? 0) == 0)
        {
            return;
        }

        AddHeader(md, "Exclude", 2);
        var mdf = md.SyntaxFactory;

        TryAddList(md, ("Occurrence Id´s", 3), filter.OccurrenceIds);

    }

    /// <summary>
    /// Attempts to add date-related information to the specified Markdown syntax tree based on the provided date
    /// filter.
    /// </summary>
    /// <remarks>If the filter is null or does not contain any valid date information, no changes are made to
    /// the Markdown syntax tree. The method adds headers for the start and end dates if they are present, and includes
    /// a list of time ranges if provided.</remarks>
    /// <param name="md">The MarkdownSyntaxTree instance to which the date information will be added.</param>
    /// <param name="filter">The DateFilterDto containing the start date, end date, and any time ranges to be included in the Markdown
    /// output.</param>
    private void TryAddDateInfo(MarkdownSyntaxTree md, DateFilterDto filter)
    {
        if (filter == null || (!filter.StartDate.HasValue && !filter.EndDate.HasValue && (filter.TimeRanges?.Count() ?? 0) == 0))
        {
            return;
        }

        AddHeader(md, "Date Filter", 2);
        TryAddParagraph(md, "Start Date", filter.StartDate.HasValue ? filter.StartDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : null);
        TryAddParagraph(md, "End Date", filter.EndDate.HasValue ? filter.EndDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : null);
        TryAddList(md, ("Time Ranges", 3), filter.TimeRanges?.Select(tr => tr.ToString()));
    }

    private void TryAddGeneralizationInfo(MarkdownSyntaxTree md, GeneralizationFilterDto filter)
    {
        if (!(filter?.PublicGeneralizationFilter.HasValue ?? false) && !(filter?.SensitiveGeneralizationFilter.HasValue ?? false))
        {
            return;
        }
        AddHeader(md, "Generalization Filter", 2);
        TryAddParagraph(md, "Public Generalization Filter", filter.PublicGeneralizationFilter.HasValue ? filter.PublicGeneralizationFilter.Value switch
        {
            PublicGeneralizationFilterDto.DontIncludeGeneralized => "Don't Include Generalized",
            PublicGeneralizationFilterDto.OnlyGeneralized => "Only Generalized",
            _ => "No filter"
        } : null);
        TryAddParagraph(md, "Sensitive Generalization Filter", filter.SensitiveGeneralizationFilter.HasValue ? filter.SensitiveGeneralizationFilter.Value switch
        {
            SensitiveGeneralizationFilterDto.IncludeGeneralizedObservations => "Include Generalized",
            SensitiveGeneralizationFilterDto.OnlyGeneralizedObservations => "Only Generalized",
            _ => "Don't Include Generalized"
        } : null);
    }


    /// <summary>
    /// Asynchronously adds geographic filter information to the specified Markdown syntax tree based on the provided
    /// filter criteria.
    /// </summary>
    /// <remarks>The method appends various geographic details to the Markdown syntax tree, including filter
    /// options, area lists, bounding box coordinates, geometries, and location identifiers, depending on the properties
    /// set in the filter. No changes are made if the filter is null.</remarks>
    /// <param name="md">The MarkdownSyntaxTree instance to which geographic filter details will be appended.</param>
    /// <param name="filter">The GeographicsFilterDto containing criteria that determine which geographic information is included. If null,
    /// no information is added.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task TryAddGeographicsInfoAsync(MarkdownSyntaxTree md, GeographicsFilterDto filter)
    {
        if (filter == null)
        {
            return;
        }

        AddHeader(md, "Geographics Filter", 2);
        TryAddParagraph(md, "Consider Authorization Buffer", filter.ConsiderAuthorizationBuffer.HasValue ? filter.ConsiderAuthorizationBuffer.Value ? "Yes" : "No" : null);
        TryAddParagraph(md, "Consider Disturbance Radius", filter.ConsiderDisturbanceRadius ? "Yes" : "No");
        TryAddParagraph(md, "Consider Observation Accuracy", filter.ConsiderObservationAccuracy ? "Yes" : "No");
        TryAddParagraph(md, "Include Observations Outside Sweden", filter.IncludeObservationsOutsideSweden.HasValue ? filter.IncludeObservationsOutsideSweden.Value ? "Yes" : "No" : null);
        TryAddParagraph(md, "Location Name Filter", filter.LocationNameFilter.HasValue() ? filter.LocationNameFilter : null);
        TryAddParagraph(md, "Location Name Filter", filter.MaxAccuracy.HasValue ? filter.MaxAccuracy.Value.ToString() : null);
        TryAddParagraph(md, "Location Name Filter", filter.MaxDistanceFromPoint.HasValue ? filter.MaxDistanceFromPoint.Value.ToString() : null);

        if ((filter.Areas?.Count() ?? 0) != 0)
        {
            var areaTypes = filter.Areas.Select(a => a.AreaType).Distinct();
            foreach (var areaType in areaTypes)
            {
                var areas = await _areaCache.GetAreasAsync(filter.Areas.Where(a => a.AreaType == areaType).Select(a => ((AreaType)areaType, a.FeatureId)));
                if ((areas?.Count() ?? 0) == 0)
                {
                    _logger.LogWarning("No areas found when adding info to markdown.");
                    continue;
                }
                TryAddList(md, (areaType.ToString(), 3), areas.Select(a => a.Name));
            }
        }
        TryAddParagraph(md, "Bounding Box", filter.BoundingBox?.BottomRight != null && filter.BoundingBox?.TopLeft != null ? $"Min Latitude: {filter.BoundingBox.BottomRight.Latitude}, Min Longitude: {filter.BoundingBox.BottomRight.Longitude}, Max Latitude: {filter.BoundingBox.TopLeft.Latitude}, Max Longitude: {filter.BoundingBox.TopLeft.Longitude}" : null);
        TryAddList(md, ("Geometries", 3), filter.Geometries?.Select(g => g.AsText()));
        TryAddList(md, ("Location Id`s", 3), filter.LocationIds);
    }

    /// <summary>
    /// Attempts to add modified date filter information to the specified Markdown syntax tree based on the provided
    /// filter criteria.
    /// </summary>
    /// <remarks>If the filter is null or both From and To dates are not specified, no modifications are made
    /// to the Markdown syntax tree. When applicable, the method adds a header and paragraphs to represent the date
    /// range in a formatted manner.</remarks>
    /// <param name="md">The MarkdownSyntaxTree instance to which the modified date filter information will be added.</param>
    /// <param name="filter">The ModifiedDateFilterDto containing the date range for filtering. Must not be null, and at least one of the
    /// From or To properties must be specified.</param>
    private void TryAddModifiedDateInfo(MarkdownSyntaxTree md, ModifiedDateFilterDto filter)
    {
        if (filter == null || (!filter.From.HasValue && !filter.To.HasValue))
        {
            return;
        }
        AddHeader(md, "Modified Date Filter", 2);
        TryAddParagraph(md, "From", filter.From.HasValue ? filter.From.Value.ToString("yyyy-MM-dd HH:mm:ss") : null);
        TryAddParagraph(md, "To", filter.To.HasValue ? filter.To.Value.ToString("yyyy-MM-dd HH:mm:ss") : null);
    }

    /// <summary>
    /// Attempts to add taxonomic filter information to the specified Markdown syntax tree based on the provided filter
    /// criteria.
    /// </summary>
    /// <remarks>This method generates sections in the Markdown document based on the filter's properties,
    /// including invasive status, taxon categories, taxa, taxon lists, and red list categories. No changes are made if
    /// the filter is null or contains no relevant criteria.</remarks>
    /// <param name="md">The MarkdownSyntaxTree instance to which taxonomic information will be added.</param>
    /// <param name="filter">The TaxonFilterDto containing criteria for filtering taxonomic information, such as IDs and categories. No
    /// information is added if this parameter is null or contains no filter criteria.</param>
    /// <param name="taxonCategories">An enumerable collection of VocabularyValueInfo representing the available taxon categories to be displayed if
    /// referenced by the filter.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task TryAddTaxaInfoAsync(MarkdownSyntaxTree md, TaxonFilterDto filter, IEnumerable<VocabularyValueInfo> taxonCategories)
    {
        if (filter == null || (
            (filter.Ids?.Count() ?? 0) == 0 &&
            (filter.TaxonListIds?.Count() ?? 0) == 0 &&
            (filter.RedListCategories?.Count() ?? 0) == 0 &&
            (filter.TaxonCategories?.Count() ?? 0) == 0 &&
            !filter.IsInvasiveInSweden.HasValue
        ))
        {
            return;
        }
        AddHeader(md, "Taxon Filter", 2);
        TryAddParagraph(md, "Is Invasive In Sweden", filter.IsInvasiveInSweden.HasValue ? filter.IsInvasiveInSweden.Value ? "Yes" : "No" : null);
        TryAddParagraph(md, "Include Underlying Taxa", filter.IncludeUnderlyingTaxa.HasValue ? filter.IncludeUnderlyingTaxa.Value ? "Yes" : "No" : null);

        if ((filter.TaxonCategories?.Count() ?? 0) != 0)
        {
            TryAddList(md, ("Taxon Categories", 3), taxonCategories?.Where(tc => filter.TaxonCategories.Contains(tc.Id)).Select(tc => tc.Value));
        }

        if ((filter.Ids?.Count() ?? 0) != 0)
        {
            var taxa = await _taxonCache.GetAllAsync();
            TryAddList(md, ("Taxa", 3), taxa?.Where(t => filter.Ids.Contains(t.Id)).Select(t => $"{t.ScientificName} ({t.TaxonId})"));
        }

        if ((filter.TaxonListIds?.Count() ?? 0) != 0)
        {
            var taxonLists = await _taxonListCache.GetAllAsync();
            TryAddList(md, ("Taxon Lists", 3), taxonLists?.Where(tl => filter.TaxonListIds.Contains(tl.Id)).Select(tl => $"{tl.Names.FirstOrDefault(n => n.CultureCode.Contains("en")).Value} ({tl.Id})"));
            TryAddParagraph(md, "Taxon List Operator", (filter.Ids?.Count() ?? 0) != 0 ? filter.TaxonListOperator.ToString() : null);
        }

        TryAddList(md, ("Red List Categories", 3), filter.RedListCategories);
    }
    #endregion Filter

    public FilterHelper(IAreaCache areaCache, IDataProviderCache dataProviderCache, IDatasetRepository datasetRepository, ICache<int, ProjectInfo> projectCache, ICache<int, Taxon> taxonCache, ICache<int, TaxonList> taxonListCache, IVocabularyRepository vocabularyRepository, ILogger<FilterHelper> logger)
    {
        _areaCache = areaCache ?? throw new ArgumentNullException(nameof(areaCache));
        _dataProviderCache = dataProviderCache ?? throw new ArgumentNullException(nameof(dataProviderCache));
        _datasetRepository = datasetRepository ?? throw new ArgumentNullException(nameof(datasetRepository));
        _projectCache = projectCache ?? throw new ArgumentNullException(nameof(projectCache));
        _taxonCache = taxonCache ?? throw new ArgumentNullException(nameof(taxonCache));
        _taxonListCache = taxonListCache ?? throw new ArgumentNullException(nameof(taxonListCache));
        _vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<MemoryStream> GetMarkDownAsync(SearchFilterBaseDto filter, CancellationToken cancellationToken)
    {
        try
        {
            if (filter == null)
            {
                return null;
            }
            var vocabularies = await _vocabularyRepository.GetAllAsync();
            var md = new MarkdownSyntaxTree();
            AddHeader(md, "Filter", 1);

            TryAddParagraph(md, "Occurrence Status", filter.OccurrenceStatus.HasValue ? filter.OccurrenceStatus.Value switch
            {
                OccurrenceStatusFilterValuesDto.Absent => "Absent",
                OccurrenceStatusFilterValuesDto.Present => "Present",
                OccurrenceStatusFilterValuesDto.BothPresentAndAbsent => "Present and Absent",
                _ => ""
            } : null);
            TryAddParagraph(md, "Bird Nest Activity Limit", filter.BirdNestActivityLimit.HasValue ? filter.BirdNestActivityLimit.Value.ToString() : null);
            TryAddParagraph(md, "Determination Filter", filter.DeterminationFilter.HasValue ? filter.DeterminationFilter.Value switch
            {
                SightingDeterminationFilterDto.NotUnsureDetermination => "Not Unsure Determination",
                SightingDeterminationFilterDto.OnlyUnsureDetermination => "Only Unsure Determination",
                _ => "No Filter"
            } : null);
            TryAddList(md, ("Diffusion Statuses", 2), filter.DiffusionStatuses?.Select(ds => ds switch
            {
                DiffusionStatusDto.DiffusedByProvider => "Diffused By Provider",
                DiffusionStatusDto.DiffusedBySystem => "Diffused By System",
                _ => "Not Diffused"
            }));
            TryAddParagraph(md, "Not Recovered Filter", filter.NotRecoveredFilter.HasValue ? filter.NotRecoveredFilter.Value switch
            {
                SightingNotRecoveredFilterDto.DontIncludeNotRecovered => "Don't Include Not Recovered",
                SightingNotRecoveredFilterDto.IncludeNotRecovered => "Include Not Recovered",
                SightingNotRecoveredFilterDto.OnlyNotRecovered => "Only Not Recovered",
                _ => "No Filter"
            } : null);
            TryAddParagraph(md, "Verification Status", filter.VerificationStatus.HasValue ? filter.VerificationStatus.Value switch
            {
                StatusVerificationDto.NotVerified => "Not Verified",
                StatusVerificationDto.Verified => "Verified",
                _ => "Both Verified and not Verified"
            } : null);
            TryAddParagraph(md, "Observed By Me", filter.ObservedByMe.HasValue ? filter.ObservedByMe.Value ? "Yes" : "No" : null);
            TryAddParagraph(md, "Reported By Me", filter.ReportedByMe.HasValue ? filter.ReportedByMe.Value ? "Yes" : "No" : null);
            TryAddDateInfo(md, filter.Date);
            await TryAddTaxaInfoAsync(md, filter.Taxon, vocabularies.FirstOrDefault(v => v.Id == VocabularyId.TaxonCategory)?.Values);
            await TryAddGeographicsInfoAsync(md, filter.Geographics);
            await TryAddDataProviderInfoAsync(md, filter.DataProvider);
            await TryAddDataProjectInfoAsync(md, filter.ProjectIds);
            TryAddEventInfo(md, filter.Event);
            TryAddModifiedDateInfo(md, filter.ModifiedDate);
            TryAddExcludeInfo(md, filter.ExcludeFilter);
            await TryAddDataStewardshipInfoAsync(md, filter.DataStewardship);

            if (filter is SearchFilterInternalBaseDto internalBaseFilter)
            {
                TryAddGeneralizationInfo(md, internalBaseFilter.GeneralizationFilter);
                await AddExtendedInfoAsync(md, internalBaseFilter.ExtendedFilter, vocabularies);
                TryAddParagraph(md, "Occurrence Status", internalBaseFilter.ProtectionFilter.HasValue ? internalBaseFilter.ProtectionFilter.Value switch
                {
                    ProtectionFilterDto.BothPublicAndSensitive => "Both Public And Sensitive",
                    ProtectionFilterDto.Sensitive => "Sensitive",
                    _ => "Public"
                } : null);
            }

            var stream = new MemoryStream();
            md.Save(stream);
            stream.Position = 0;
            return stream;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create filter markdown");
        }
        return null;
    }

    public async Task<Stream> GetAnalysisMarkDownAsync(
        SearchFilterAggregationInternalDto? searchFilter,
        double[]? alphaValues,
        int? gridCellSizeInMeters,
        bool? useCenterPoint,
        bool? useEdgeLengthRatio,
        bool? allowHoles,
        bool? returnGridCells,
        bool? includeEmptyCells,
        MetricCoordinateSys? metricCoordinateSys,
        CoordinateSys? coordinateSystem,
        CancellationToken cancellationToken)
    {
        try
        {
            if (searchFilter == null)
            {
                return null;
            }

            var md = new MarkdownSyntaxTree();
            AddHeader(md, "Analysis", 1);
            var mdf = md.SyntaxFactory;

            // Add analysis parameters
            AddHeader(md, "Analysis Parameters", 2);

            if (alphaValues?.Length > 0)
            {
                TryAddList(md, ("Alpha Values", 3), alphaValues.Select(a => a.ToString()));
            }

            if (gridCellSizeInMeters.HasValue)
            {
                var paragraph = mdf.Paragraph();
                paragraph.AppendChild(mdf.Text($"Grid Cell Size In Meters: {gridCellSizeInMeters.Value}"));
                md.AppendChild(paragraph);
            }

            if (useCenterPoint.HasValue)
            {
                var paragraph = mdf.Paragraph();
                paragraph.AppendChild(mdf.Text($"Use Center Point: {(useCenterPoint.Value ? "Yes" : "No")}"));
                md.AppendChild(paragraph);
            }

            if (useEdgeLengthRatio.HasValue)
            {
                var paragraph = mdf.Paragraph();
                paragraph.AppendChild(mdf.Text($"Use Edge Length Ratio: {(useEdgeLengthRatio.Value ? "Yes" : "No")}"));
                md.AppendChild(paragraph);
            }

            if (allowHoles.HasValue)
            {
                var paragraph = mdf.Paragraph();
                paragraph.AppendChild(mdf.Text($"Allow Holes: {(allowHoles.Value ? "Yes" : "No")}"));
                md.AppendChild(paragraph);
            }

            if (returnGridCells.HasValue)
            {
                var paragraph = mdf.Paragraph();
                paragraph.AppendChild(mdf.Text($"Return Grid Cells: {(returnGridCells.Value ? "Yes" : "No")}"));
                md.AppendChild(paragraph);
            }

            if (includeEmptyCells.HasValue)
            {
                var paragraph = mdf.Paragraph();
                paragraph.AppendChild(mdf.Text($"Include Empty Cells: {(includeEmptyCells.Value ? "Yes" : "No")}"));
                md.AppendChild(paragraph);
            }

            if (metricCoordinateSys.HasValue)
            {
                var paragraph = mdf.Paragraph();
                paragraph.AppendChild(mdf.Text($"Metric Coordinate System: {metricCoordinateSys.Value}"));
                md.AppendChild(paragraph);
            }

            if (coordinateSystem.HasValue)
            {
                var paragraph = mdf.Paragraph();
                paragraph.AppendChild(mdf.Text($"Coordinate System: {coordinateSystem.Value}"));
                md.AppendChild(paragraph);
            }

            // Add search filter information if provided
            if (searchFilter != null)
            {
                var vocabularies = await _vocabularyRepository.GetAllAsync();

                AddHeader(md, "Search Filter", 2);

                TryAddDateInfo(md, searchFilter.Date);
                await TryAddTaxaInfoAsync(md, searchFilter.Taxon, vocabularies.FirstOrDefault(v => v.Id == VocabularyId.TaxonCategory)?.Values);
                await TryAddGeographicsInfoAsync(md, searchFilter.Geographics);
                await TryAddDataProviderInfoAsync(md, searchFilter.DataProvider);
                await TryAddDataProjectInfoAsync(md, searchFilter.ProjectIds);
                TryAddEventInfo(md, searchFilter.Event);
                TryAddModifiedDateInfo(md, searchFilter.ModifiedDate);
                TryAddExcludeInfo(md, searchFilter.ExcludeFilter);
                await TryAddDataStewardshipInfoAsync(md, searchFilter.DataStewardship);
            }

            var stream = new MemoryStream();
            md.Save(stream);
            stream.Position = 0;
            return stream;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create filter markdown");
        }
        return null;
    }

    /// <summary>
    /// Gets the analysis markdown for AOO/EOO Article 17 calculations.
    /// </summary>
    public async Task<Stream> GetAnalysisMarkDownAsync(
        SearchFilterBaseDto? searchFilter,
        int? maxDistance,
        int? gridCellSizeInMeters,
        MetricCoordinateSys? metricCoordinateSys,
        CoordinateSys? coordinateSystem,
        CancellationToken cancellationToken)
    {
        try
        {
            if (searchFilter == null)
            {
                return null;
            }

            var md = new MarkdownSyntaxTree();
            AddHeader(md, "Analysis", 1);
            var mdf = md.SyntaxFactory;

            // Add analysis parameters
            AddHeader(md, "Analysis Parameters", 2);

            if (maxDistance.HasValue)
            {
                var paragraph = mdf.Paragraph();
                paragraph.AppendChild(mdf.Text($"Max Distance: {maxDistance.Value}"));
                md.AppendChild(paragraph);
            }

            if (gridCellSizeInMeters.HasValue)
            {
                var paragraph = mdf.Paragraph();
                paragraph.AppendChild(mdf.Text($"Grid Cell Size In Meters: {gridCellSizeInMeters.Value}"));
                md.AppendChild(paragraph);
            }

            if (metricCoordinateSys.HasValue)
            {
                var paragraph = mdf.Paragraph();
                paragraph.AppendChild(mdf.Text($"Metric Coordinate System: {metricCoordinateSys.Value}"));
                md.AppendChild(paragraph);
            }

            if (coordinateSystem.HasValue)
            {
                var paragraph = mdf.Paragraph();
                paragraph.AppendChild(mdf.Text($"Coordinate System: {coordinateSystem.Value}"));
                md.AppendChild(paragraph);
            }

            // Add search filter information if provided
            if (searchFilter != null)
            {
                var vocabularies = await _vocabularyRepository.GetAllAsync();

                AddHeader(md, "Search Filter", 2);

                TryAddDateInfo(md, searchFilter.Date);
                await TryAddTaxaInfoAsync(md, searchFilter.Taxon, vocabularies.FirstOrDefault(v => v.Id == VocabularyId.TaxonCategory)?.Values);
                await TryAddGeographicsInfoAsync(md, searchFilter.Geographics);
                await TryAddDataProviderInfoAsync(md, searchFilter.DataProvider);
                await TryAddDataProjectInfoAsync(md, searchFilter.ProjectIds);
                TryAddEventInfo(md, searchFilter.Event);
                TryAddModifiedDateInfo(md, searchFilter.ModifiedDate);
                TryAddExcludeInfo(md, searchFilter.ExcludeFilter);
                await TryAddDataStewardshipInfoAsync(md, searchFilter.DataStewardship);
            }

            var stream = new MemoryStream();
            md.Save(stream);
            stream.Position = 0;
            return stream;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create filter markdown for Article 17");
        }
        return null;
    }

    public MemoryStream TryGetFeatureCollection(IEnumerable<Geometry> geometries)
    {
        if ((geometries?.Count() ?? 0) == 0)
        {
            return null;
        }

        var featureCollection = new FeatureCollection();
        foreach (var geometry in geometries)
        {
            var feature = new Feature
            {
                Geometry = geometry
            };
            featureCollection.Add(feature);
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = false
        };
        options.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());

        var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, featureCollection, options);
        stream.Position = 0;
        return stream;
    }
}