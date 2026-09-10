using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Dtos.Enum;
using SOS.Lib.Dtos.Filter;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Repositories.Resource.Interfaces;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using static SOS.Lib.Dtos.Filter.ExtendedFilterDto;

namespace SOS.Lib.Helpers;

public class MarkdownHelper : IMarkdownHelper
{
    private readonly IProcessedObservationCoreRepository _processedObservationCoreRepository;
    private readonly IProcessInfoRepository _processInfoRepository;
    private readonly IAreaCache _areaCache;
    private readonly IDataProviderCache _dataProviderCache;
    private readonly IDatasetRepository _datasetRepository;
    private readonly IFilterManager _filterManager;
    private readonly ICache<int, ProjectInfo> _projectCache;
    private readonly ICache<int, Taxon> _taxonCache;
    private readonly ICache<int, TaxonList> _taxonListCache;
    private readonly IVocabularyRepository _vocabularyRepository;
    private readonly ILogger<MarkdownHelper> _logger;

    private const string BR = "  \n";

    /// <summary>
    /// Adds an ATX-style header to the specified Markdown syntax tree at the given heading level.
    /// </summary>
    /// <remarks>This method appends a new header node as a child of the provided Markdown syntax tree. The
    /// level parameter should correspond to standard Markdown heading levels. Supplying a value outside the range of 1
    /// to 6 may result in invalid Markdown output.</remarks>
    /// <param name="sb">String builder used to store markdown</param>
    /// <param name="text">The text content to use for the header.</param>
    /// <param name="level">The heading level for the header. Must be an integer between 1 and 6, where 1 represents the largest header.</param>
    private void AddHeader(StringBuilder sb, string text, int level)
    {
        var delimiter = "";
        for (var i = 0; i < level; i++)
        {
            delimiter += "#";
        }

        sb.Append($"{delimiter} {text} {delimiter}{BR}\n");
    }

    /// <summary>
    /// Attempts to add an unordered list of items to the specified Markdown syntax tree under the given header.
    /// </summary>
    /// <remarks>If listData is null or contains no items, the method does not modify the Markdown syntax
    /// tree.</remarks>
    /// <param name="sb">String builder used to store markdown</param>
    /// <param name="header">A tuple containing the header text and its level, specifying where the list should be inserted.</param>
    /// <param name="listData">An optional collection of strings representing the items to include in the list. If null or empty, no list is
    /// added.</param>
    private void TryAddList(StringBuilder sb, (string Text, int Level) header, IEnumerable<string>? listData)
    {
        if ((listData?.Count() ?? 0) == 0)
        {
            return;
        }
        
        AddHeader(sb, header.Text, header.Level);
        foreach (var item in listData)
        {
            sb.Append($"* {item}{BR}");
        }
        sb.Append("\n");
    }

    private void TryAddParagraph(StringBuilder sb, string label, string value, bool pagebreakAfterLabel = false, bool onlySinglebreakAfterValue = false)
    {
        if (string.IsNullOrEmpty(value))
        {
            return;
        }

        if (!string.IsNullOrEmpty(label))
        {
            sb.Append($"**{label}**");
            if (pagebreakAfterLabel)
            {
                sb.Append(BR);
            }
            else
            {
                sb.Append(" ");
            }
        }

        sb.Append($"{value}{BR}{(onlySinglebreakAfterValue ? "" : BR)}");
    }


    private void TryAddTable(StringBuilder sb, string label, IEnumerable<string> columnLabels, IEnumerable<IEnumerable<string>> values)
    {
        if (!values?.Any() ?? true)
        {
            return;
        }
       
        if (!string.IsNullOrEmpty(label))
        {
            sb.Append($"**{label}**{BR}");
        }

        sb.Append("|");
        foreach (var columnLabel in columnLabels)
        {
            sb.Append($" {columnLabel} |");
        }
        sb.Append(BR);

        sb.Append("|");
        foreach (var _ in columnLabels)
        {
            sb.Append(" ---------- |");
        }
        sb.Append(BR);

        foreach (var rowValues in values)
        {
            sb.Append("|");
            foreach (var value in rowValues)
            {
                sb.Append($" {value} |");
            }
            sb.Append(BR);
        }
        sb.Append("\n");
    }
    #region Filter

    private async Task AddExtendedInfoAsync(StringBuilder sb, ExtendedFilterDto filter, IEnumerable<Vocabulary> vocabularies)
    {
        if (filter == null)
        {
            return;
        }
        AddHeader(sb, "Extended Filter", 2);
        if (filter.ActivityIds?.Count() > 0)
        {
            var activities = vocabularies.FirstOrDefault(v => v.Id == VocabularyId.Activity)?.Values;
            if ((activities?.Count() ?? 0) == 0)
            {
                _logger.LogWarning("No activities found when adding info to markdown.");
            }
            else
            {
                TryAddList(sb, ("Activities", 3), activities.Where(a => filter.ActivityIds.Contains(a.Id)).Select(a => a.Value));
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
                TryAddParagraph(sb, "Biotope", (biotopes.FirstOrDefault(b => b.Id == filter.BiotopeId)?.Value ?? filter.BiotopeId.ToString()));
            }
        }

        TryAddParagraph(sb, "Checklist Id", filter.ChecklistId.HasValue && filter.ChecklistId.Value != 0 ? filter.ChecklistId.ToString() : null);
        TryAddList(sb, ("Datasource Id´s", 3), filter.DatasourceIds?.Select(dsi => dsi.ToString()));

        if (filter.DiscoveryMethodIds?.Count() > 0)
        {
            var discoveryMethods = vocabularies.FirstOrDefault(v => v.Id == VocabularyId.DiscoveryMethod)?.Values;
            if ((discoveryMethods?.Count() ?? 0) == 0)
            {
                _logger.LogWarning("No discovery methods found when adding info to markdown.");
            }
            else
            {
                TryAddList(sb, ("Discovery Methods", 3), discoveryMethods.Where(dmi => filter.DiscoveryMethodIds.Contains(dmi.Id)).Select(dmi => dmi.Value));
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
                TryAddList(sb, ("Exclude Verification Status", 3), verificationStatuses.Where(vs => filter.ExcludeVerificationStatusIds.Contains(vs.Id)).Select(vs => vs.Value));
            }
        }

        TryAddList(sb, ("Field DiaryGroup Id`s", 3), filter.FieldDiaryGroupIds?.Select(fdgi => fdgi.ToString()));
        if (filter.NotPresentFilter != SightingNotPresentFilterDto.NoFilter)
        {
            TryAddParagraph(sb, "Not Present Filter", filter.NotPresentFilter switch
            {
                SightingNotPresentFilterDto.DontIncludeNotPresent => "Only present",
                SightingNotPresentFilterDto.IncludeNotPresent => "Present and not present",
                SightingNotPresentFilterDto.OnlyNotPresent => "Only not present",
                _ => ""
            });
        }
        
        TryAddParagraph(sb, "Observed by user id", filter.ObservedByUserId.HasValue && filter.ObservedByUserId.Value != 0 ? filter.ObservedByUserId.Value.ToString() : null);
        TryAddParagraph(sb, "Observed by userservice user id", filter.ObservedByUserServiceUserId.HasValue && filter.ObservedByUserServiceUserId.Value != 0 ? filter.ObservedByUserServiceUserId.Value.ToString() : null);
        TryAddParagraph(sb, "Only secondhand information", filter.OnlySecondHandInformation ? "Yes" : null);
        TryAddParagraph(sb, "Only with barcode", filter.OnlyWithBarcode ? "Yes" : null);
        TryAddParagraph(sb, "Only with media", filter.OnlyWithMedia ? "Yes" : null);
        TryAddParagraph(sb, "Only with notes", filter.OnlyWithNotes ? "Yes" : null);
        TryAddParagraph(sb, "Only with notes of interest", filter.OnlyWithNotesOfInterest ? "Yes" : null);
        TryAddParagraph(sb, "Only with user comments", filter.OnlyWithUserComments ? "Yes" : null);
        TryAddParagraph(sb, "Private collection", filter.PrivateCollection);
        TryAddParagraph(sb, "Public collection", filter.PublicCollection);
        TryAddList(sb, ("Publish type id`s", 3), filter.PublishTypeIdsFilter?.Select(pti => pti.ToString()));

        if (filter.Quantity.HasValue)
        {
            TryAddParagraph(sb, "Quantity", filter.Quantity.Value.ToString());
            TryAddParagraph(sb, "Quantity", filter.QuantityOperator);
        }
        TryAddParagraph(sb, "Reported by user id", filter.ReportedByUserId.HasValue && filter.ReportedByUserId != 0 ? filter.ReportedByUserId.ToString() : null);
        TryAddParagraph(sb, "Reported by userservice user id", filter.ReportedByUserServiceUserId.HasValue && filter.ReportedByUserServiceUserId.Value != 0 ? filter.ReportedByUserServiceUserId.Value.ToString() : null);
        TryAddParagraph(sb, "Reported date from", filter.ReportedDateFrom.HasValue ? filter.ReportedDateFrom.Value.ToString("yyyy-MM-dd HH:mm:ss") : null);
        TryAddParagraph(sb, "Reported date to", filter.ReportedDateTo.HasValue ? filter.ReportedDateTo.Value.ToString("yyyy-MM-dd HH:mm:ss") : null);
        if (filter.SexIds?.Count() > 0)
        {
            var sexes = vocabularies.FirstOrDefault(v => v.Id == VocabularyId.Sex)?.Values;
            if ((sexes?.Count() ?? 0) == 0)
            {
                _logger.LogWarning("No sexes found when adding info to markdown.");
            }
            else
            {
                TryAddList(sb, ("Sexes", 3), sexes.Where(dmi => filter.SexIds.Contains(dmi.Id)).Select(dmi => dmi.Value));
            }
        }
        TryAddList(sb, ("Site id`s", 3), filter.SiteIds?.Select(s => s.ToString()));
        TryAddList(sb, ("Site project id`s", 3), filter.SiteProjectIds?.Select(spi => spi.ToString()));
        TryAddParagraph(sb, "Species collection label", filter.SpeciesCollectionLabel);
        TryAddList(sb, ("Species facts id`s", 3), filter.SpeciesFactsIds?.Select(sfi => sfi.ToString()));

        if (filter.SubstrateId.HasValue)
        {
            var substrates = vocabularies.FirstOrDefault(v => v.Id == VocabularyId.Substrate)?.Values;
            if ((substrates?.Count() ?? 0) == 0)
            {
                _logger.LogWarning("No substrates found when adding info to markdown.");
            }
            else
            {
                TryAddParagraph(sb, "Substrate", substrates.First(s => s.Id == filter.SubstrateId.Value).Value);
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
                TryAddParagraph(sb, "Substrate species", taxon?.ScientificName ?? filter.SubstrateSpeciesId.Value.ToString());
            }
        }
        TryAddList(sb, ("Triggered Observation Rule Frequency Id`s", 3), filter.TriggeredObservationRuleFrequencyIds?.Select(torfi => torfi.ToString()));
        TryAddList(sb, ("Triggered Observation Rule Reproduction Id`s", 3), filter.TriggeredObservationRuleReproductionIds?.Select(torri => torri.ToString()));
        /*TryAddParagraph(sb, "Not Present Filter", filter.TypeFilter switch
        {
            SightingTypeFilterDto.DoNotShowMergedIncludeReplacementChilds => "Do Not Show Merged Include Replacement Childs",
            SightingTypeFilterDto.DoNotShowSightingsInMerged => "Do Not Show Sightings In Merged",
            SightingTypeFilterDto.ShowBoth => "Show Both",
            SightingTypeFilterDto.ShowChildrenAndReplacements => "Show Children And Replacements",
            SightingTypeFilterDto.ShowOnlyMerged => "Show Only Merged",
            _ => "Do Not Show Merged"
        });*/
        TryAddParagraph(sb, "Unspontaneous Filter", filter.UnspontaneousFilter switch
        {
            SightingUnspontaneousFilterDto.NotUnspontaneous => "Spontaneous",
            SightingUnspontaneousFilterDto.Unspontaneous => "Unspontaneous",
            _ => ""
        });
        TryAddParagraph(sb, "Use Period For All Years", filter.UsePeriodForAllYears ? "Yes" : null);
        if (filter.VerificationStatusIds?.Count() > 0)
        {
            var verificationStatuses = vocabularies.FirstOrDefault(v => v.Id == VocabularyId.VerificationStatus)?.Values;
            if ((verificationStatuses?.Count() ?? 0) == 0)
            {
                _logger.LogWarning("No verification statuses found when adding info to markdown.");
            }
            else
            {
                TryAddList(sb, ("Verification Status", 3), verificationStatuses.Where(vs => filter.VerificationStatusIds.Contains(vs.Id)).Select(vs => vs.Value));
            }
        }
        if (filter.Weight.HasValue)
        {
            TryAddParagraph(sb, "Weight", filter.Weight.Value.ToString());
            TryAddParagraph(sb, "Weight operator", filter.WeightOperator);
        }
        if ((filter.Years?.Count() ?? 0) != 0)
        {
            TryAddList(sb, ("Years", 3), filter.Years.Select(y => y.ToString()));
            TryAddParagraph(sb, "Years comparison", filter.YearsComparison switch
            {
                DateFilterComparisonDto.BothStartDateAndEndDate => "Both start date and end date",
                DateFilterComparisonDto.EndDate => "End date",
                DateFilterComparisonDto.StartDateEndDateMonthRange => "Start date end date month range",
                _ => "Start date"
            });
        }
    }

    /// <summary>
    /// Attempts to add data stewardship information to the specified Markdown syntax tree using the provided filter
    /// criteria. Adds sections for dataset title and identifiers if the corresponding filters are present.
    /// </summary>
    /// <remarks>If neither the dataset title filter nor dataset identifiers are specified in the filter, the
    /// method does not modify the Markdown syntax tree.</remarks>
    /// <param name="sb">The MarkdownSyntaxTree instance to which data stewardship information will be added.</param>
    /// <param name="filter">The DataStewardshipFilterDto containing filter criteria for dataset title and identifiers. If both are not
    /// provided, no changes are made.</param>
    private async Task TryAddDataStewardshipInfoAsync(StringBuilder sb, DataStewardshipFilterDto filter)
    {
        if (string.IsNullOrEmpty(filter?.DatasetTitleFilter) && (filter?.DatasetIdentifiers?.Count() ?? 0) == 0)
        {
            return;
        }
        AddHeader(sb, "Data Stewardship", 2);
        TryAddParagraph(sb, "Title Filter", filter.DatasetTitleFilter);
        if (filter.DatasetIdentifiers?.Count() > 0)
        {
            var datasets = await _datasetRepository.GetDatasetsByIds(filter.DatasetIdentifiers);
            if ((datasets?.Count() ?? 0) == 0)
            {
                _logger.LogWarning("No datasets found when adding info to markdown.");
                return;
            }
            TryAddList(sb, ("Identifiers", 3), datasets.Select(ds => ds.Title));
        }
    }

    private async Task TryAddDataProjectInfoAsync(StringBuilder sb, IEnumerable<int> projectIds)
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
        TryAddList(sb, ("Projects", 2), projects
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
    /// <param name="sb">The MarkdownSyntaxTree instance to which data provider information will be added.</param>
    /// <param name="filter">The SearchFilterBaseDto containing the data provider identifiers to process.</param>
    /// <returns>A task that represents the asynchronous operation of adding data provider information to the Markdown syntax
    /// tree.</returns>
    private async Task TryAddDataProviderInfoAsync(StringBuilder sb, DataProviderFilterDto filter)
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

        TryAddList(sb, ("Data Providers", 2), dataProviders
            .Where(dp => filter.Ids.Contains(dp.Id))
            .Select(dp => dp.Identifier)
        );
    }

    /// <summary>
    /// Attempts to add event information to the specified Markdown syntax tree based on the provided filter criteria.
    /// </summary>
    /// <remarks>If the filter's Ids collection is null or empty, no event information will be added to the
    /// Markdown syntax tree.</remarks>
    /// <param name="sb">The MarkdownSyntaxTree instance to which the event information will be added.</param>
    /// <param name="filter">The EventFilterDto containing the filter criteria, including a list of event IDs to be processed.</param>
    private void TryAddEventInfo(StringBuilder sb, EventFilterDto filter)
    {
        if ((filter?.Ids?.Count() ?? 0) == 0)
        {
            return;
        }

        AddHeader(sb, "Event", 2);
        TryAddList(sb, ("Id´s", 3), filter.Ids);
    }

    /// <summary>
    /// Attempts to add exclusion information to the specified Markdown syntax tree based on the provided filter.
    /// </summary>
    /// <remarks>If the filter does not contain any occurrence IDs, no action is taken. The method adds a
    /// header and a list of occurrence IDs to the Markdown syntax tree.</remarks>
    /// <param name="sb">The MarkdownSyntaxTree instance to which the exclusion information will be added.</param>
    /// <param name="filter">The ExcludeFilterDto containing the occurrence IDs to be excluded. Must not be null and should contain at least
    /// one occurrence ID.</param>
    private void TryAddExcludeInfo(StringBuilder sb, ExcludeFilterDto filter)
    {
        if ((filter?.OccurrenceIds?.Count() ?? 0) == 0)
        {
            return;
        }

        AddHeader(sb, "Exclude", 2);
        TryAddList(sb, ("Occurrence Id´s", 3), filter.OccurrenceIds);
    }

    /// <summary>
    /// Attempts to add date-related information to the specified Markdown syntax tree based on the provided date
    /// filter.
    /// </summary>
    /// <remarks>If the filter is null or does not contain any valid date information, no changes are made to
    /// the Markdown syntax tree. The method adds headers for the start and end dates if they are present, and includes
    /// a list of time ranges if provided.</remarks>
    /// <param name="sb">The MarkdownSyntaxTree instance to which the date information will be added.</param>
    /// <param name="filter">The DateFilterDto containing the start date, end date, and any time ranges to be included in the Markdown
    /// output.</param>
    private void TryAddDateInfo(StringBuilder sb, DateFilterDto filter)
    {
        if (filter == null || (!filter.StartDate.HasValue && !filter.EndDate.HasValue && (filter.TimeRanges?.Count() ?? 0) == 0))
        {
            return;
        }

        AddHeader(sb, "Date Filter", 2);
        TryAddParagraph(sb, "Start Date", filter.StartDate.HasValue ? filter.StartDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : null);
        TryAddParagraph(sb, "End Date", filter.EndDate.HasValue ? filter.EndDate.Value.ToString("yyyy-MM-dd HH:mm:ss") : null);
        TryAddList(sb, ("Time Ranges", 3), filter.TimeRanges?.Select(tr => tr.ToString()));
    }

    private void TryAddGeneralizationInfo(StringBuilder sb, GeneralizationFilterDto filter)
    {
        if (!(filter?.PublicGeneralizationFilter.HasValue ?? false) && !(filter?.SensitiveGeneralizationFilter.HasValue ?? false))
        {
            return;
        }
        AddHeader(sb, "Generalization Filter", 2);
        TryAddParagraph(sb, "Public Generalization Filter", filter.PublicGeneralizationFilter.HasValue && filter.PublicGeneralizationFilter != PublicGeneralizationFilterDto.NoFilter ? filter.PublicGeneralizationFilter.Value switch
        {
            PublicGeneralizationFilterDto.DontIncludeGeneralized => "Don't Include Generalized",
            PublicGeneralizationFilterDto.OnlyGeneralized => "Only Generalized",
            _ => "No filter"
        } : null);
        TryAddParagraph(sb, "Sensitive Generalization Filter", filter.SensitiveGeneralizationFilter.HasValue ? filter.SensitiveGeneralizationFilter.Value switch
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
    /// <param name="sb">The MarkdownSyntaxTree instance to which geographic filter details will be appended.</param>
    /// <param name="filter">The GeographicsFilterDto containing criteria that determine which geographic information is included. If null,
    /// no information is added.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task TryAddGeographicsInfoAsync(StringBuilder sb, GeographicsFilterDto filter)
    {
        if (filter == null)
        {
            return;
        }

        AddHeader(sb, "Geographics Filter", 2);
        TryAddParagraph(sb, "Consider Authorization Buffer", filter.ConsiderAuthorizationBuffer.HasValue ? filter.ConsiderAuthorizationBuffer.Value ? "Yes" : null : null);
        TryAddParagraph(sb, "Consider Disturbance Radius", filter.ConsiderDisturbanceRadius ? "Yes" : null);
        TryAddParagraph(sb, "Consider Observation Accuracy", filter.ConsiderObservationAccuracy ? "Yes" : null);
        TryAddParagraph(sb, "Include Observations Outside Sweden", filter.IncludeObservationsOutsideSweden.HasValue ? filter.IncludeObservationsOutsideSweden.Value ? "Yes" : null : null);
        TryAddParagraph(sb, "Location Name Filter", filter.LocationNameFilter.HasValue() ? filter.LocationNameFilter : null);
        TryAddParagraph(sb, "Location Name Filter", filter.MaxAccuracy.HasValue ? filter.MaxAccuracy.Value.ToString() : null);
        TryAddParagraph(sb, "Location Name Filter", filter.MaxDistanceFromPoint.HasValue ? filter.MaxDistanceFromPoint.Value.ToString() : null);

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
                TryAddList(sb, (areaType.ToString(), 3), areas.Select(a => a.Name));
            }
        }
        TryAddParagraph(sb, "Bounding Box", filter.BoundingBox?.BottomRight != null && filter.BoundingBox?.TopLeft != null ? $"Min Latitude: {filter.BoundingBox.BottomRight.Latitude}, Min Longitude: {filter.BoundingBox.BottomRight.Longitude}, Max Latitude: {filter.BoundingBox.TopLeft.Latitude}, Max Longitude: {filter.BoundingBox.TopLeft.Longitude}" : null);
        TryAddList(sb, ("Geometries", 3), filter.Geometries?.Select(g => g.AsText()));
        TryAddList(sb, ("Location Id`s", 3), filter.LocationIds);
    }

    /// <summary>
    /// Attempts to add modified date filter information to the specified Markdown syntax tree based on the provided
    /// filter criteria.
    /// </summary>
    /// <remarks>If the filter is null or both From and To dates are not specified, no modifications are made
    /// to the Markdown syntax tree. When applicable, the method adds a header and paragraphs to represent the date
    /// range in a formatted manner.</remarks>
    /// <param name="sb">The MarkdownSyntaxTree instance to which the modified date filter information will be added.</param>
    /// <param name="filter">The ModifiedDateFilterDto containing the date range for filtering. Must not be null, and at least one of the
    /// From or To properties must be specified.</param>
    private void TryAddModifiedDateInfo(StringBuilder sb, ModifiedDateFilterDto filter)
    {
        if (filter == null || (!filter.From.HasValue && !filter.To.HasValue))
        {
            return;
        }
        AddHeader(sb, "Modified Date Filter", 2);
        TryAddParagraph(sb, "From", filter.From.HasValue ? filter.From.Value.ToString("yyyy-MM-dd HH:mm:ss") : null);
        TryAddParagraph(sb, "To", filter.To.HasValue ? filter.To.Value.ToString("yyyy-MM-dd HH:mm:ss") : null);
    }

    /// <summary>
    /// Attempts to add taxonomic filter information to the specified Markdown syntax tree based on the provided filter
    /// criteria.
    /// </summary>
    /// <remarks>This method generates sections in the Markdown document based on the filter's properties,
    /// including invasive status, taxon categories, taxa, taxon lists, and red list categories. No changes are made if
    /// the filter is null or contains no relevant criteria.</remarks>
    /// <param name="sb">The MarkdownSyntaxTree instance to which taxonomic information will be added.</param>
    /// <param name="filter">The TaxonFilterDto containing criteria for filtering taxonomic information, such as IDs and categories. No
    /// information is added if this parameter is null or contains no filter criteria.</param>
    /// <param name="taxonCategories">An enumerable collection of VocabularyValueInfo representing the available taxon categories to be displayed if
    /// referenced by the filter.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task TryAddTaxaInfoAsync(StringBuilder sb, TaxonFilterDto filter, IEnumerable<VocabularyValueInfo> taxonCategories)
    {
        if (filter == null)
        {
            return;
        }

        var taxonIds = await _filterManager.GetTaxonIdsAsync(filter.Ids,
            filter.IncludeUnderlyingTaxa ?? false,
            filter.TaxonListIds,
            filter.TaxonListOperator switch { TaxonListOperatorDto.Filter => TaxonFilter.TaxonListOp.Filter, _ => TaxonFilter.TaxonListOp.Merge },
            filter.TaxonCategories,
            false);

        if (
            (taxonIds?.Count() ?? 0) == 0 &&
            (filter.TaxonListIds?.Count() ?? 0) == 0 &&
            (filter.RedListCategories?.Count() ?? 0) == 0 &&
            (filter.TaxonCategories?.Count() ?? 0) == 0 &&
            !filter.IsInvasiveInSweden.HasValue
        )
        {
            return;
        }
        AddHeader(sb, "Taxon Filter", 2);
        TryAddParagraph(sb, "Is Invasive In Sweden", filter.IsInvasiveInSweden.HasValue ? filter.IsInvasiveInSweden.Value ? "Yes" : null : null);
        TryAddParagraph(sb, "Include Underlying Taxa", filter.IncludeUnderlyingTaxa.HasValue ? filter.IncludeUnderlyingTaxa.Value ? "Yes" : null : null);

        if ((filter.TaxonCategories?.Count() ?? 0) != 0)
        {
            TryAddList(sb, ("Taxon Categories", 3), taxonCategories?.Where(tc => filter.TaxonCategories.Contains(tc.Id)).Select(tc => tc.Value));
        }

        if ((taxonIds?.Count() ?? 0) != 0)
        {
            var taxa = await _taxonCache.GetAllAsync();
            TryAddList(sb, ("Taxa", 3), taxa?.Where(t => taxonIds.Contains(t.Id)).Select(t => $"{t.ScientificName} ({t.TaxonId})"));
        }

        if ((filter.TaxonListIds?.Count() ?? 0) != 0)
        {
            var taxonLists = await _taxonListCache.GetAllAsync();
            TryAddList(sb, ("Taxon Lists", 3), taxonLists?.Where(tl => filter.TaxonListIds.Contains(tl.Id)).Select(tl => $"{tl.Names.FirstOrDefault(n => n.CultureCode.Contains("en")).Value} ({tl.Id})"));
            TryAddParagraph(sb, "Taxon List Operator", (filter.Ids?.Count() ?? 0) != 0 ? filter.TaxonListOperator.ToString() : null);
        }

        TryAddList(sb, ("Red List Categories", 3), filter.RedListCategories);
    }
    #endregion Filter

    public MarkdownHelper(IAreaCache areaCache,
        IDataProviderCache dataProviderCache,
        IDatasetRepository datasetRepository,
        IFilterManager filterManager,
        ICache<int, ProjectInfo> projectCache,
        ICache<int, Taxon> taxonCache,
        ICache<int, TaxonList> taxonListCache,
        IProcessInfoRepository processInfoRepository,
        IProcessedObservationCoreRepository processedObservationCoreRepository,
        IVocabularyRepository vocabularyRepository,
        ILogger<MarkdownHelper> logger)
    {
        _areaCache = areaCache ?? throw new ArgumentNullException(nameof(areaCache));
        _dataProviderCache = dataProviderCache ?? throw new ArgumentNullException(nameof(dataProviderCache));
        _datasetRepository = datasetRepository ?? throw new ArgumentNullException(nameof(datasetRepository));
        _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
        _projectCache = projectCache ?? throw new ArgumentNullException(nameof(projectCache));
        _taxonCache = taxonCache ?? throw new ArgumentNullException(nameof(taxonCache));
        _taxonListCache = taxonListCache ?? throw new ArgumentNullException(nameof(taxonListCache));
        _processInfoRepository = processInfoRepository ?? throw new ArgumentNullException(nameof(processInfoRepository));
        _processedObservationCoreRepository = processedObservationCoreRepository ?? throw new ArgumentNullException(nameof(processedObservationCoreRepository));
        _vocabularyRepository = vocabularyRepository ?? throw new ArgumentNullException(nameof(vocabularyRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<MemoryStream> GetCitationMarkDownAsync(string? authorizationApplicationIdentifier, IEnumerable<int> dataProviderIds, CancellationToken cancellationToken)
    {
        try
        {
            if (!dataProviderIds?.Any() ?? true)
            {
                dataProviderIds ??= Array.Empty<int>();
            }

            var dataProviders = (await _dataProviderCache.GetAllAsync()).Where(dp => dataProviderIds.Contains(dp.Id));
            var sb = new StringBuilder();
            AddHeader(sb, $"Citeringsanvisning {DateTime.Now:yyyy-MM-dd HH:mm}", 1);

            var assebly = Assembly.GetEntryAssembly();
            var publishYear = assebly.GetBuildDate().Year;
            string version = assebly.GetVersionNumber();

            var ingress = "I vetenskapliga publikationer och andra rapporter där data hämtad via Species Observation System använts, bör man referera enligt följande:";
            var materialAndReferense = $"Data använda i den här studien har laddats ned från Species Observation System API (SLU Artdatabanken {publishYear}) den {DateTime.Now:yyyy-MM-dd}. Följande dataset har använts: ";
            var referenceList = new List<string> { $"SLU Artdatabanken ({publishYear}). Species Observation System API (Version {version}) [API]. SLU Artdatabanken. https://api-portal.artdatabanken.se/product#product=sos [åtkomst {DateTime.Now:yyyy-MM-dd}]" };

            if (authorizationApplicationIdentifier?.ToLower() == "printobs")
            {
                ingress = "I vetenskapliga publikationer och andra rapporter där verktyget Fynddata använts, bör man referera enligt följande:";
                materialAndReferense = $"Data använda i den här studien har laddats ned från SLU Artdatabankens verktyg Fynddata (SLU Artdatabanken [utgivningsår]) den {DateTime.Now:yyyy-MM-dd}. Följande dataset har använts: ";
                referenceList = new List<string> { @$"SLU Artdatabanken (utgivningsår). Fynddata (Version #) [Webbapplikation]. SLU Artdatabanken. https://fynddata.artdatabanken.se [åtkomstdatum {DateTime.Now:yyyy-MM-dd}]",
                        $"fynddata.artdatabanken.se ({DateTime.Now:yyyy-MM-dd}) SOS fynd export. [Filnamn]." };
            }

            if (dataProviders?.Any() ?? false)
            {
                var firstItteration = true;
                const string DEFAULT_TYPE_OF_DATASET = "Occurrence";
                foreach (var dataProvider in dataProviders)
                {
                    var typeOfDataset = DEFAULT_TYPE_OF_DATASET;
                    var xml = await _dataProviderCache.GetEmlAsync(dataProvider.Id);

                    if (xml != null)
                    {
                        XNamespace eml = "eml://ecoinformatics.org/eml-2.1.1";
                        var keyWords = new[]
                        {
                            "GBIF Dataset Type Vocabulary: http://rs.gbif.org/vocabulary/gbif/dataset_type.xml",
                            "GBIF Dataset Type Vocabulary: https://rs.gbif.org/vocabulary/gbif/dataset_type.xml"
                        };

                        typeOfDataset = xml
                            .Descendants("keywordSet")
                            .Select(ks => ks.Element("keywordThesaurus"))
                            .Where(e => e != null && keyWords.Contains(e.Value))
                            .FirstOrDefault()?.Parent?.Element("keyword")?.Value ?? DEFAULT_TYPE_OF_DATASET;
                    }
                        
                    referenceList.Add($"{dataProvider.ContactPerson?.LastName} {dataProvider.ContactPerson?.FirstName?.Substring(0, 1)} ({DateTime.Now.Year}). {dataProvider.Names?.FirstOrDefault(n => n.CultureCode == "sv-SE")?.Value}. {dataProvider.Organizations?.FirstOrDefault(n => n.CultureCode == "sv-SE")?.Value}. {typeOfDataset?.ToCamelCase()} hämtat via Species Observation System API version {version} {DateTime.Now:yyyy-MM-dd}");
                    materialAndReferense += $"{(firstItteration ? "" : ", ")}{dataProvider.Identifier}";
                    firstItteration = false;
                }
            }
            
            TryAddParagraph(sb, null, ingress);
            TryAddParagraph(sb, "Material och metoder", materialAndReferense, true);
            TryAddParagraph(sb, "Referenslista", string.Join("  \n", referenceList), true);
            //TryAddParagraph(sb, "Till exempel", @$"SLU Artdatabanken ({publishYear}). SOS (Version {version}) [Webbapplikation]. SLU Artdatabanken. https://api-portal.artdatabanken.se/product#product=sos [åtkomst {DateTime.Now:yyyy-MM-dd}]", true);

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
            stream.Position = 0;
            return stream;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Failed to create citation markdown for provider id´s: {string.Join(", ", dataProviderIds)}");
        }
        return new MemoryStream();
    }

    /// <inheritdoc/>
    public async Task<MemoryStream> GetFilterMarkDownAsync(SearchFilterBaseDto filter, CancellationToken cancellationToken)
    {
        try
        {
            if (filter == null)
            {
                return new MemoryStream();
            }
            var vocabularies = await _vocabularyRepository.GetAllAsync();
            var sb = new StringBuilder();
            AddHeader(sb, $"Filter {DateTime.Now:yyyy-MM-dd HH:mm}", 1);

            TryAddParagraph(sb, "Occurrence Status", filter.OccurrenceStatus.HasValue ? filter.OccurrenceStatus.Value switch
            {
                OccurrenceStatusFilterValuesDto.Absent => "Absent",
                OccurrenceStatusFilterValuesDto.Present => "Present",
                OccurrenceStatusFilterValuesDto.BothPresentAndAbsent => "Present and Absent",
                _ => ""
            } : null);
            TryAddParagraph(sb, "Bird nest activity limit", filter.BirdNestActivityLimit.HasValue ? filter.BirdNestActivityLimit.Value.ToString() : null);
            TryAddParagraph(sb, "Determination filter", filter.DeterminationFilter.HasValue && filter.DeterminationFilter != SightingDeterminationFilterDto.NoFilter ? filter.DeterminationFilter.Value switch
            {
                SightingDeterminationFilterDto.NotUnsureDetermination => "Only sure determination",
                SightingDeterminationFilterDto.OnlyUnsureDetermination => "Only unsure determination",
                _ => "No Filter"
            } : null);
            TryAddList(sb, ("Diffusion Statuses", 2), filter.DiffusionStatuses?.Select(ds => ds switch
            {
                DiffusionStatusDto.DiffusedByProvider => "Diffused by provider",
                DiffusionStatusDto.DiffusedBySystem => "Diffused by system",
                _ => "Not Diffused"
            }));
            TryAddParagraph(sb, "Not Recovered Filter", filter.NotRecoveredFilter.HasValue && filter.NotRecoveredFilter != SightingNotRecoveredFilterDto.NoFilter ? filter.NotRecoveredFilter.Value switch
            {
                SightingNotRecoveredFilterDto.DontIncludeNotRecovered => "Only recovered",
                SightingNotRecoveredFilterDto.IncludeNotRecovered => "Include not recovered",
                SightingNotRecoveredFilterDto.OnlyNotRecovered => "Only not recovered",
                _ => "No Filter"
            } : null);
            TryAddParagraph(sb, "Verification Status", filter.VerificationStatus.HasValue ? filter.VerificationStatus.Value switch
            {
                StatusVerificationDto.NotVerified => "Not verified",
                StatusVerificationDto.Verified => "Verified",
                _ => "Both verified and not verified"
            } : null);
            TryAddParagraph(sb, "Observed by me", filter.ObservedByMe.HasValue ? filter.ObservedByMe.Value ? "Yes" : null : null);
            TryAddParagraph(sb, "Reported by me", filter.ReportedByMe.HasValue ? filter.ReportedByMe.Value ? "Yes" : null : null);
            TryAddDateInfo(sb, filter.Date);
            await TryAddTaxaInfoAsync(sb, filter.Taxon, vocabularies.FirstOrDefault(v => v.Id == VocabularyId.TaxonCategory)?.Values);
            await TryAddGeographicsInfoAsync(sb, filter.Geographics);
            await TryAddDataProviderInfoAsync(sb, filter.DataProvider);
            await TryAddDataProjectInfoAsync(sb, filter.ProjectIds);
            TryAddEventInfo(sb, filter.Event);
            TryAddModifiedDateInfo(sb, filter.ModifiedDate);
            TryAddExcludeInfo(sb, filter.ExcludeFilter);
            await TryAddDataStewardshipInfoAsync(sb, filter.DataStewardship);

            if (filter is SearchFilterInternalBaseDto internalBaseFilter)
            {
                TryAddGeneralizationInfo(sb, internalBaseFilter.GeneralizationFilter);
                await AddExtendedInfoAsync(sb, internalBaseFilter.ExtendedFilter, vocabularies);
                TryAddParagraph(sb, "Sensitive", internalBaseFilter.ProtectionFilter.HasValue ? internalBaseFilter.ProtectionFilter.Value switch
                {
                    ProtectionFilterDto.BothPublicAndSensitive => "Both public and sensitive",
                    ProtectionFilterDto.Sensitive => "Sensitive",
                    _ => "Public"
                } : null);
            }

            TryAddParagraph(sb, "Koordinatsystem", "WGS84", onlySinglebreakAfterValue: true);
           
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
            return stream;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create filter markdown");
        }
        return new MemoryStream();
    }

    /// <inheritdoc/>
    public async Task<(MemoryStream FileStream, IEnumerable<int> DataProviderIds)> GetDatasetsMarkDownAsync(SearchFilter searchFilter, CancellationToken cancellationToken)
    {
        try
        {
            var processInfo = await _processInfoRepository.GetAsync(_processedObservationCoreRepository.UniquePublicIndexName);

            if(processInfo == null)
            {
                _logger.LogWarning("No process info found when creating dataset markdown.");
                return (new MemoryStream(), Array.Empty<int>());
            }

            if (!processInfo.ProvidersInfo?.Any() ?? true)
            {
                _logger.LogWarning("No providers info found when creating dataset markdown.");
                return (new MemoryStream(), Array.Empty<int>());
            }

            var sb = new StringBuilder();
            AddHeader(sb, $"Datasets {DateTime.Now:yyyy-MM-dd HH:mm}", 1);

            IEnumerable<int> dataProviderIds = Array.Empty<int>();
            var providerCount = new Dictionary<int, int>();
            if (searchFilter != null)
            {
                var result = await _processedObservationCoreRepository.GetAggregationItemsAsync(searchFilter, "dataProviderId", 0, 100);
                if (result?.Records?.Any() ?? false)
                {
                    dataProviderIds = result.Records.Select(r => int.Parse(r.AggregationKey));
                    providerCount = result.Records.ToDictionary(r => int.Parse(r.AggregationKey), r => r.DocCount);
                }
            }

            TryAddTable(sb, null, ["Dataset", "Antal observationer", "Publika observationer", "I denna fil"], processInfo.ProvidersInfo.OrderBy(pi => pi.DataProviderIdentifier).Select(pi => new string[] { pi.DataProviderIdentifier, $"{pi.PublicProcessCount + pi.ProtectedProcessCount}", $"{pi.PublicProcessCount}", $"{(providerCount.TryGetValue(pi.DataProviderId ?? 0, out var pc) ? pc.ToString() : 0) }" }));

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
            return (stream, dataProviderIds);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create datasets markdown");
        }
        return (new MemoryStream(), Array.Empty<int>());
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
                return new MemoryStream();
            }

            var sb = new StringBuilder();
            AddHeader(sb, "Analysis", 1);

            // Add analysis parameters
            AddHeader(sb, "Analysis Parameters", 2);

            if ((alphaValues?.Length ?? 0) > 0)
            {
                TryAddList(sb, ("Alpha Values", 3), alphaValues.Select(a => a.ToString()));
            }

            if (gridCellSizeInMeters.HasValue)
            {
                TryAddParagraph(sb, null, $"Grid Cell Size In Meters: {gridCellSizeInMeters.Value}");
            }

            if (useCenterPoint.HasValue)
            {
                TryAddParagraph(sb, null, $"Use Center Point: {(useCenterPoint.Value ? "Yes" : "No")}");
            }

            if (useEdgeLengthRatio.HasValue)
            {
                TryAddParagraph(sb, null, $"Use Edge Length Ratio: {(useEdgeLengthRatio.Value ? "Yes" : "No")}");
            }

            if (allowHoles.HasValue)
            {
                TryAddParagraph(sb, null, $"Allow Holes: {(allowHoles.Value ? "Yes" : "No")}");
            }

            if (returnGridCells.HasValue)
            {
                TryAddParagraph(sb, null, $"Return Grid Cells: {(returnGridCells.Value ? "Yes" : "No")}");
            }

            if (includeEmptyCells.HasValue)
            {
                TryAddParagraph(sb, null, $"Include Empty Cells: {(includeEmptyCells.Value ? "Yes" : "No")}");
            }

            if (metricCoordinateSys.HasValue)
            {
                TryAddParagraph(sb, null, $"Metric Coordinate System: {metricCoordinateSys.Value}");
            }

            if (coordinateSystem.HasValue)
            {
                TryAddParagraph(sb, null, $"Coordinate System: {coordinateSystem.Value}");
            }

            // Add search filter information if provided
            if (searchFilter != null)
            {
                var vocabularies = await _vocabularyRepository.GetAllAsync();

                AddHeader(sb, "Search Filter", 2);

                TryAddDateInfo(sb, searchFilter.Date);
                await TryAddTaxaInfoAsync(sb, searchFilter.Taxon, vocabularies.FirstOrDefault(v => v.Id == VocabularyId.TaxonCategory)?.Values);
                await TryAddGeographicsInfoAsync(sb, searchFilter.Geographics);
                await TryAddDataProviderInfoAsync(sb, searchFilter.DataProvider);
                await TryAddDataProjectInfoAsync(sb, searchFilter.ProjectIds);
                TryAddEventInfo(sb, searchFilter.Event);
                TryAddModifiedDateInfo(sb, searchFilter.ModifiedDate);
                TryAddExcludeInfo(sb, searchFilter.ExcludeFilter);
                await TryAddDataStewardshipInfoAsync(sb, searchFilter.DataStewardship);
            }

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
            return stream;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create filter markdown");
        }
        return new MemoryStream();
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
                return new MemoryStream();
            }

            var sb = new StringBuilder();
            AddHeader(sb, "Analysis", 1);
            // Add analysis parameters
            AddHeader(sb, "Analysis Parameters", 2);

            if (maxDistance.HasValue)
            {
                TryAddParagraph(sb, null, $"Max Distance: {maxDistance.Value}");
            }

            if (gridCellSizeInMeters.HasValue)
            {
                TryAddParagraph(sb, null, $"Grid Cell Size In Meters: {gridCellSizeInMeters.Value}");
            }

            if (metricCoordinateSys.HasValue)
            {
                TryAddParagraph(sb, null, $"Metric Coordinate System: {metricCoordinateSys.Value}");
            }

            if (coordinateSystem.HasValue)
            {
                TryAddParagraph(sb, null, $"Coordinate System: {coordinateSystem.Value}");
            }

            // Add search filter information if provided
            if (searchFilter != null)
            {
                var vocabularies = await _vocabularyRepository.GetAllAsync();

                AddHeader(sb, "Search Filter", 2);

                TryAddDateInfo(sb, searchFilter.Date);
                await TryAddTaxaInfoAsync(sb, searchFilter.Taxon, vocabularies.FirstOrDefault(v => v.Id == VocabularyId.TaxonCategory)?.Values);
                await TryAddGeographicsInfoAsync(sb, searchFilter.Geographics);
                await TryAddDataProviderInfoAsync(sb, searchFilter.DataProvider);
                await TryAddDataProjectInfoAsync(sb, searchFilter.ProjectIds);
                TryAddEventInfo(sb, searchFilter.Event);
                TryAddModifiedDateInfo(sb, searchFilter.ModifiedDate);
                TryAddExcludeInfo(sb, searchFilter.ExcludeFilter);
                await TryAddDataStewardshipInfoAsync(sb, searchFilter.DataStewardship);
            }

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
            return stream;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to create filter markdown for Article 17");
        }
        return new MemoryStream();
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
