using CSharpFunctionalExtensions;
using NetTopologySuite.Geometries;
using SOS.Lib.Cache;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Shared.Api.Configuration;
using SOS.Shared.Api.Dtos.Enum;
using SOS.Shared.Api.Dtos.Filter;
using SOS.Shared.Api.Validators.Interfaces;
using System.Globalization;
using System.Text.RegularExpressions;
using Result = CSharpFunctionalExtensions.Result;

namespace SOS.Shared.Api.Validators;

/// <summary>
/// Validate input
/// </summary>
public class InputValidator : IInputValidator
{
    private readonly IAreaCache _areaCache;
    private readonly SortableFieldsCache _sortableFieldsCache;
    private readonly ITaxonManager _taxonManager;
    
    private readonly double _countFactor;
    private readonly int _elasticSearchMaxRecordsInternal;
    private readonly int _elasticSearchMaxRecordsPublic;
    private readonly int _maxBatchSize;
    private readonly int _maxNrElasticSearchAggregationBuckets;
    private readonly IEnumerable<int>? _signalSearchTaxonListIds;
    private readonly int _tilesLimitInternal;
    private readonly int _tilesLimitPublic;

    /// <summary>
    /// Validate tiles limit
    /// </summary>
    /// <param name="tilesLimit"></param>
    /// <param name="maxTilesTot"></param>
    /// <param name="countTask"></param>
    /// <returns></returns>
    private async Task<Result> TryValidateTilesLimitAsync(int tilesLimit,
        double maxTilesTot,
        Task<long> countTask)
    {
        if (maxTilesTot > tilesLimit)
        {
            var count = await countTask;

            if (count > tilesLimit * _countFactor)
            {
                return Result.Failure($"The number of cells that can be returned is too large. The limit is {tilesLimit} cells. Your query results in {maxTilesTot} possible cells and {count} observations. Try using larger grid cell size or a smaller bounding box.");
            }
        }

        return Result.Success();
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="areaCache"></param>
    /// <param name="sortableFieldsCache"></param>
    /// <param name="taxonManager"></param>
    /// <param name="configuration"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public InputValidator(
        IAreaCache areaCache,
        SortableFieldsCache sortableFieldsCache,
        ITaxonManager taxonManager,
        InputValaidationConfiguration configuration
    )
    {
        _areaCache = areaCache ?? throw new ArgumentNullException(nameof(areaCache));
        _sortableFieldsCache = sortableFieldsCache ?? throw new ArgumentNullException(nameof(sortableFieldsCache));
        _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }
        _countFactor = configuration.CountFactor > 0 ? configuration.CountFactor : throw new ArgumentException("CountFactor must be greater than 0");
        _elasticSearchMaxRecordsInternal = configuration.ElasticSearchMaxRecordsInternal > 0 ? configuration.ElasticSearchMaxRecordsInternal : throw new ArgumentException("ElasticSearchMaxRecordsInternal must be greater than 0");
        _elasticSearchMaxRecordsPublic = configuration.ElasticSearchMaxRecordsPublic > 0 ? configuration.ElasticSearchMaxRecordsPublic : throw new ArgumentException("ElasticSearchMaxRecordsPublic must be greater than 0");
        _maxBatchSize = configuration.MaxBatchSize > 0 ? configuration.MaxBatchSize : throw new ArgumentException("MaxBatchSize must be greater than 0");
        _maxNrElasticSearchAggregationBuckets = configuration.MaxNrElasticSearchAggregationBuckets > 0 ? configuration.MaxNrElasticSearchAggregationBuckets : throw new ArgumentException("MaxNrElasticSearchAggregationBuckets must be greater than 0");
        _signalSearchTaxonListIds = configuration.SignalSearchTaxonListIds?.Any() ?? false ? configuration.SignalSearchTaxonListIds : throw new ArgumentException("SignalSearchTaxonListIds don't have any values");
        _tilesLimitInternal = configuration.TilesLimitInternal > 0 ? configuration.TilesLimitInternal : throw new ArgumentException("TilesLimitInternal must be greater than 0");
        _tilesLimitPublic = configuration.TilesLimitPublic > 0 ? configuration.TilesLimitPublic : throw new ArgumentException("TilesLimitPublic must be greater than 0");
    }

    /// <inheritdoc/>
    public async Task<Result> ValidateAreasAsync(IEnumerable<AreaFilterDto>? areaKeys)
    {
        if (!areaKeys?.Any() ?? true)
        {
            return Result.Success();
        }

        var existingAreaIds = (await _areaCache.GetAreasAsync(areaKeys!.Select(a => ((AreaType)a.AreaType, a.FeatureId))))
            .Select(a => new AreaFilterDto { AreaType = (AreaTypeDto)a.AreaType, FeatureId = a.FeatureId });

        var missingAreas = areaKeys?
            .Where(aid => !existingAreaIds.Any(a => a.AreaType.Equals(aid.AreaType) && (a?.FeatureId?.Equals(aid.FeatureId, StringComparison.CurrentCultureIgnoreCase) ?? false)))
            .Select(aid => $"Area doesn't exist (AreaType: {aid.AreaType}, FeatureId: {aid.FeatureId})");

        return missingAreas?.Any() ?? false ?
            Result.Failure(string.Join(". ", missingAreas))
            :
            Result.Success();
    }

    /// <inheritdoc/>
    public Result ValidateAggregationPagingArguments(int skip, int? take, bool handleInfinityTake = false)
    {
        if (skip < 0) return Result.Failure("Skip must be 0 or greater.");

        if (handleInfinityTake && take == -1)
        {
            return Result.Success();
        }

        if (take <= 0) return Result.Failure("Take must be greater than 0");
        if (take != null && skip + take > _maxNrElasticSearchAggregationBuckets)
            return Result.Failure($"Skip+Take={skip + take}. Skip+Take must be less than or equal to {_maxNrElasticSearchAggregationBuckets}.");

        return Result.Success();
    }

    /// <inheritdoc/>
    public Result ValidateBoundingBox(
        LatLonBoundingBoxDto? boundingBox,
        bool mandatory = false)
    {
        if (boundingBox == null)
        {
            return mandatory ? Result.Failure("Bounding box is missing.") : Result.Success();
        }

        if (boundingBox.TopLeft == null || boundingBox.BottomRight == null)
        {
            return Result.Failure("Bounding box is incomplete.");
        }

        if (boundingBox.TopLeft.Longitude >= boundingBox.BottomRight.Longitude)
        {
            return Result.Failure("Bounding box left longitude value is >= right longitude value.");
        }

        if (boundingBox.BottomRight.Latitude >= boundingBox.TopLeft.Latitude)
        {
            return Result.Failure("Bounding box bottom latitude value is >= top latitude value.");
        }

        return Result.Success();
    }

    /// <inheritdoc/>
    public Result ValidateDouble(double value, double minLimit, double maxLimit, string paramName)
    {
        if (value < minLimit || value > maxLimit)
        {
            return Result.Failure($"{paramName} must be between {minLimit.ToString(CultureInfo.InvariantCulture)} and {maxLimit.ToString(CultureInfo.InvariantCulture)}");
        }

        return Result.Success();
    }


    /// <inheritdoc/>
    public Result ValidateEmail(string email)
    {
        var errors = new List<string>();
        if (string.IsNullOrEmpty(email))
        {
            errors.Add("Could not find a e-mail address");
        }

        var emailRegex =
            new Regex(
                @"^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$");
        if (!emailRegex.IsMatch(email))
        {
            errors.Add("Not a valid e-mail");
        }

        if (errors.Count > 0)
        {
            return Result.Failure(string.Join(". ", errors));
        }

        return Result.Success();
    }

    /// <inheritdoc/>
    public Result ValidateEncryptPassword(string password, string confirmPassword, ProtectionFilterDto protectionFilter)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(password) && !protectionFilter.Equals(ProtectionFilterDto.Public))
        {
            errors.Add("You need to state a encrypt password when you are requesting sensitive observations");
        }

        if ((password?.Any() ?? false) && (password?.Trim().Length ?? 0) < 10)
        {
            errors.Add("Password must contain at least 10 characters");
        }

        if (password != confirmPassword)
        {
            errors.Add("Confirmed password is not equal to password");
        }

        if (errors.Any())
        {
            return Result.Failure(string.Join(". ", errors));
        }

        return Result.Success();
    }

    /// <inheritdoc/>
    public Result ValidateFields(IEnumerable<string> fields)
    {
        var errors = new List<string>();

        if (fields?.Any() ?? false)
        {
            errors.AddRange(fields
                // .Where(f => !ObservationPropertyFieldDescriptionHelper.FieldByPropertyPath.ContainsKey(f.ToLower()))
                .Where(f => !typeof(Observation).HasProperty(f))
                .Select(f => $"Field doesn't exist ({f})"));
        }

        return errors.Any() ? Result.Failure(string.Join(". ", errors)) : Result.Success();
    }

    /// <inheritdoc/>
    public Result ValidateGeographicalAreaExists(GeographicsFilterDto filter)
    {
        if ((!filter?.Areas?.Any() ?? true) && (!filter?.Geometries?.Any() ?? true) && filter?.BoundingBox?.TopLeft == null && filter?.BoundingBox?.BottomRight == null)
        {
            return Result.Failure("You must provide area/s, geometry or bounding box");
        }

        return Result.Success();
    }

    /// <inheritdoc/>
    public Result ValidateGeogridZoomArgument(int zoom, int minLimit, int maxLimit)
    {
        if (zoom < minLimit || zoom > maxLimit)
        {
            return Result.Failure($"Zoom must be between {minLimit} and {maxLimit}");
        }

        return Result.Success();
    }

    /// <inheritdoc/>
    public Result ValidateGeometries(
        IEnumerable<Geometry> geometries)
    {
        if (!geometries?.Any() ?? true)
        {
            return Result.Success();
        }

        try
        {
            foreach (var geoemtry in geometries)
            {
                if (!geoemtry?.IsValid ?? true)
                {
                    throw new Exception("Invalid geometry");
                }
            }
        }
        catch
        {
            return Result.Failure($"Invalid geometry");
        }

        return Result.Success();
    }

    /// <inheritdoc/>
    public Result ValidateGridCellSizeInMetersArgument(int gridCellSizeInMeters, int minLimit, int maxLimit)
    {
        if (gridCellSizeInMeters < minLimit || gridCellSizeInMeters > maxLimit)
        {
            return Result.Failure($"Grid cell size in meters must be between {minLimit} and {maxLimit}");
        }

        return Result.Success();
    }

    /// <inheritdoc/>
    public Result ValidateInt(int value, int minLimit, int maxLimit, string paramName)
    {
        if (value < minLimit || value > maxLimit)
        {
            return Result.Failure($"{paramName} must be between {minLimit.ToString(CultureInfo.InvariantCulture)} and {maxLimit.ToString(CultureInfo.InvariantCulture)}");
        }

        return Result.Success();
    }

    /// <inheritdoc/>
    public Result ValidatePropertyExists(string name, string property, bool mandatory = false)
    {
        if (string.IsNullOrEmpty(property))
        {
            return mandatory ? Result.Failure($"You must state {name}") : Result.Success();
        }

        if (typeof(Observation).HasProperty(property))
        {
            return Result.Success();
        }

        return Result.Failure($"Missing property ({property}) used for {name}");
    }

    /// <inheritdoc/>
    public virtual async Task<Result> ValidateSearchFilterAsync(SearchFilterBaseDto filter, bool allowObjectInOutputFields = true, bool bboxMandatory = false)
    {
        var errors = new List<string>();

        var searchFilter = filter as SearchFilterDto;

        if (filter == null)
        {
            errors.Add("You must provide a filter");
        }

        var areaValidationResult = ValidateAreasAsync(filter?.Geographics?.Areas).Result;
        if (areaValidationResult.IsFailure)
        {
            errors.Add(areaValidationResult.Error);
        }

        if (filter.Geographics != null && filter.Geographics.IsGeometryInvalid)
        {
            errors.Add("Invalid geometry");
        }
        else
        {
            var bboxResult = ValidateBoundingBox(filter?.Geographics?.BoundingBox, bboxMandatory);
            if (bboxResult.IsFailure)
            {
                errors.Add(bboxResult.Error);
            }
        }

        if (filter?.ModifiedDate?.From > filter?.ModifiedDate?.To)
        {
            errors.Add("Modified from date can't be greater tha to date");
        }

        if (searchFilter?.Output?.Fields?.Any() ?? false)
        {
            errors.AddRange(searchFilter.Output.Fields
                .Where(of => !typeof(Observation).HasProperty(of, allowObjectInOutputFields))
                .Select(of => allowObjectInOutputFields ? $"Output field doesn't exist ({of})" : $"Output field doesn't exist ({of}), or if it's root object. Specify object properties"));
        }

        if (filter?.Taxon?.RedListCategories?.Any() ?? false)
        {
            errors.AddRange(filter.Taxon.RedListCategories
                .Where(rc => !new[] { "DD", "EX", "RE", "CR", "EN", "VU", "NT", "LC", "NA", "NE" }.Contains(rc, StringComparer.CurrentCultureIgnoreCase))
                .Select(rc => $"Red list category doesn't exist ({rc})"));
        }
        if (filter?.Date?.DateFilterType == DateFilterTypeDto.OnlyStartDate && (filter.Date?.StartDate == null || filter.Date?.EndDate == null))
        {
            errors.Add("When using OnlyStartDate as filter both StartDate and EndDate need to be specified");
        }
        if (filter?.Date?.DateFilterType == DateFilterTypeDto.OnlyEndDate && (filter.Date?.StartDate == null || filter.Date?.EndDate == null))
        {
            errors.Add("When using OnlyEndDate as filter both StartDate and EndDate need to be specified");
        }

        var taxaValidationResult = await ValidateTaxaAsync(filter.Taxon?.Ids!);
        if (taxaValidationResult.IsFailure)
        {
            errors.Add(taxaValidationResult.Error);
        }

        return errors.Any() ? Result.Failure(string.Join(". ", errors)) : Result.Success();
    }

    /// <inheritdoc/>
    public Result ValidateSearchPagingArguments(int skip, int take)
    {
        var errors = new List<string>();

        if (skip < 0)
        {
            errors.Add("Skip must be positive.");
        }

        if (take <= 0)
        {
            errors.Add("You must take at least 1 observation.");
        }

        if (take > _maxBatchSize)
        {
            errors.Add($"You can't take more than {_maxBatchSize} at a time.");
        }

        if (skip + take > _elasticSearchMaxRecordsPublic)
        {
            errors.Add($"Skip + take can't be greater than {_elasticSearchMaxRecordsPublic}");
        }

        if (errors.Count > 0) return Result.Failure(string.Join(". ", errors));
        return Result.Success();
    }

    /// <inheritdoc/>
    public Result ValidateSearchPagingArgumentsInternal(int skip, int take)
    {
        var errors = new List<string>();

        if (skip + take > _elasticSearchMaxRecordsInternal)
        {
            errors.Add($"Skip + take can't be greater than {_elasticSearchMaxRecordsInternal}");
        }

        if (errors.Count > 0) return Result.Failure(string.Join(". ", errors));
        return Result.Success();
    }
    /// <inheritdoc/>
    public async Task<Result> ValidateSignalSearchAsync(SignalFilterDto filter, bool validateSearchFilter, int areaBuffer)
    {
        Result validateTaxonLists(TaxonFilterBaseDto filter)
        {
            if (!filter?.TaxonListIds?.Any() ?? true)
            {
                return Result.Failure("You have to provide at least one taxon list id");
            }

            var containsMandatoryTaxonList = filter.TaxonListIds.Any(tlid => _signalSearchTaxonListIds.Contains(tlid));
            if (!containsMandatoryTaxonList)
            {
                return Result.Failure("You have to provide at least one mandatory signal search taxon list");
            }

            return Result.Success();
        }

        return Result.Combine(
            validateSearchFilter ? (await ValidateTaxaAsync(filter?.Taxon?.Ids)) : Result.Success(),
            ValidateGeographicalAreaExists(filter?.Geographics),
            areaBuffer < 0 || areaBuffer > 100
                ? Result.Failure("areaBuffer must be between 0 and 100")
                : Result.Success(),
            filter?.StartDate > DateTime.Now.AddYears(-1) ? Result.Failure("Start date must be at least one year back in time") : Result.Success(),
            validateTaxonLists(filter?.Taxon)
        );
    }

    /// <inheritdoc/>
    public async Task<Result<List<string>>> ValidateSortFieldsAsync(IEnumerable<string> sortFields)
    {            
        if ((sortFields?.Count() ?? 0) == 0)
        {
            return Result.Success<List<string>>(null);
        }

        var sortableFieldsSet = await _sortableFieldsCache.GetSortableFieldsAsync();
        var errors = new List<string>();
        var validFields = new List<string>();

        foreach (var sortField in sortFields)
        {
            string sortableField = null;
            if (!sortableFieldsSet?.TryGetValue(sortField, out sortableField) ?? false)
            {
                errors.Add($"{sortField} is not a sortable field");
            }
            else
            {
                validFields.Add(sortableField);
            }
        }

        if (errors.Count > 0)
        {
            return Result.Failure<List<string>>(string.Join(", ", errors));
        }

        return Result.Success(validFields);
    }

    /// <inheritdoc/>
    public async Task<Result> ValidateTaxaAsync(IEnumerable<int> taxonIds)
    {
        var taxonTree = await _taxonManager.GetTaxonTreeAsync();
        var missingTaxa = taxonIds?
            .Where(tid => !taxonTree.TreeNodeById.ContainsKey(tid))
            .Select(tid => $"TaxonId doesn't exist ({tid})");

        return missingTaxa?.Any() ?? false ?
            Result.Failure(string.Join(". ", missingTaxa))
            :
            Result.Success();
    }

    /// <inheritdoc/>
    public Result ValidateTaxonExists(SearchFilterBaseDto filter)
    {
        var taxonCount = filter?.Taxon?.Ids?.Count() ?? 0;
        return taxonCount == 0
            ? Result.Failure("You must provide taxon id's")
            : Result.Success();
    }

    /// <inheritdoc/>
    public Result ValidateTaxonAggregationPagingArguments(int? skip, int? take)
    {
        const int maxPagingRecords = 1000;
        if (skip < 0) return Result.Failure("Skip must be 0 or greater.");
        if (take <= 0) return Result.Failure("Take must be greater than 0");
        if (take != null && skip != null && skip + take > maxPagingRecords)
            return Result.Failure($"Skip+Take={skip + take}. Skip+Take must be less than or equal to {maxPagingRecords}. Set Skip & Take to null if you want to retrieve all records.");

        return Result.Success();
    }

    /// <inheritdoc/>
    public Result ValidateTranslationCultureCode(string translationCultureCode)
    {
        // No culture code, set default
        if (string.IsNullOrEmpty(translationCultureCode))
        {
            translationCultureCode = "sv-SE";
        }

        if (!new[] { "sv-SE", "en-GB" }.Contains(translationCultureCode,
            StringComparer.CurrentCultureIgnoreCase))
        {
            return Result.Failure("Unknown FieldTranslationCultureCode. Supported culture codes, sv-SE, en-GB");
        }

        return Result.Success();
    }

    /// <inheritdoc/>
    public async Task<Result> ValidateTilesLimitAsync(
        Envelope envelope,
        int zoom,
        Task<long> countTask,
        bool internalLimit = false)
    {
        if (envelope == null)
        {
            return Result.Success();
        }

        var maxTilesTot = envelope.CalculateNumberOfTiles(zoom);
        var tilesLimit = internalLimit ? _tilesLimitInternal : _tilesLimitPublic;
        return await TryValidateTilesLimitAsync(tilesLimit, maxTilesTot, countTask);
    }

    /// <inheritdoc/>
    public async Task<Result> ValidateTilesLimitMetricAsync(
        Envelope envelope,
        int gridCellSizeInMeters,
        Task<long> countTask,
        bool internalLimit = false,
        double internalLimitFactor = 1.0)
    {
        if (envelope == null)
        {
            return Result.Success();
        }

        var maxLonTiles = Math.Ceiling((envelope.MaxX - envelope.MinX) / gridCellSizeInMeters);
        var maxLatTiles = Math.Ceiling((envelope.MaxY - envelope.MinY) / gridCellSizeInMeters);
        var maxTilesTot = maxLonTiles * maxLatTiles;
        var tilesLimit = internalLimit ? (int)(_tilesLimitInternal*internalLimitFactor) : _tilesLimitPublic;

        return await TryValidateTilesLimitAsync(tilesLimit, maxTilesTot, countTask);
    }
}
