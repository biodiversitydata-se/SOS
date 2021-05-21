using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    /// Base controller for observation related controller
    /// </summary>
    public class ObservationBaseController : ControllerBase
    {
        private readonly ITaxonManager _taxonManager;

        private const int MaxBatchSize = 1000;
        private const int ElasticSearchMaxRecords = 10000;

        protected readonly IObservationManager ObservationManager;

        protected string ReplaceDomain(string str, string domain, string path)
        {
            // This is a bad solution to fix problems when behind load balancer...
            return Regex.Replace(str, string.Format(@"(https?:\/\/.*?)(\/{0}\/v2)?(\/.*)", path), m => domain + m.Groups[3].Value);
        }

        /// <summary>
        /// Current user id
        /// </summary>
        protected int CurrentUserId => int.Parse(User?.Claims?.FirstOrDefault(c => c?.Type == "sub")?.Value ?? "0");

        protected Result ValidateEmail(string email)
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

        protected Result<int> ValidateGeogridZoomArgument(int zoom, int minLimit, int maxLimit)
        {
            if (zoom < minLimit || zoom > maxLimit)
            {
                return Result.Failure<int>($"Zoom must be between {minLimit} and {maxLimit}");
            }

            return Result.Success(zoom);
        }

        protected Result ValidateAggregationPagingArguments(int skip, int? take, bool handleInfinityTake = false)
        {
            if (skip < 0) return Result.Failure("Skip must be 0 or greater.");

            if (handleInfinityTake && take == 0)
            {
                return Result.Success();
            }

            if (take <= 0) return Result.Failure("Take must be greater than 0");
            if (take != null && skip + take > ObservationManager.MaxNrElasticSearchAggregationBuckets)
                return Result.Failure($"Skip+Take={skip + take}. Skip+Take must be less than or equal to {ObservationManager.MaxNrElasticSearchAggregationBuckets}.");

            return Result.Success();
        }

        protected Result ValidateTaxonAggregationPagingArguments(int? skip, int? take)
        {
            const int maxPagingRecords = 1000;
            if (skip < 0) return Result.Failure("Skip must be 0 or greater.");
            if (take <= 0) return Result.Failure("Take must be greater than 0");
            if (take != null && skip != null && skip + take > maxPagingRecords)
                return Result.Failure($"Skip+Take={skip + take}. Skip+Take must be less than or equal to {maxPagingRecords}. Set Skip & Take to null if you want to retrieve all records.");

            return Result.Success();
        }

        protected Result ValidatePropertyExists(string name, string property, bool mandatory = false)
        {
            if (string.IsNullOrEmpty(property))
            {
                return mandatory ? Result.Failure($"You must state { name }") : Result.Success();
            }

            if (typeof(Observation).HasProperty(property))
            {
                return Result.Success();
            }

            return Result.Failure($"Missing property ({ property }) used for { name }");
        }

        protected Result ValidateSearchFilter(SearchFilterBaseDto filter)
        {
            var errors = new List<string>();

            var searchFilter = filter as SearchFilterDto;
            if (searchFilter?.OutputFields?.Any() ?? false)
            {
                errors.AddRange(searchFilter.OutputFields
                    .Where(of => !typeof(Observation).HasProperty(of))
                    .Select(of => $"Output field doesn't exist ({of})"));
            }

            var searchFilterInternal = filter as SearchFilterInternalDto;
            if (searchFilterInternal?.OutputFields?.Any() ?? false)
            {
                errors.AddRange(searchFilterInternal.OutputFields
                    .Where(of => !typeof(Observation).HasProperty(of))
                    .Select(of => $"Output field doesn't exist ({of})"));
            }

            var taxaValidationResult = ValidateTaxa(filter.Taxon?.Ids);
            if (taxaValidationResult.IsFailure)
            {
                errors.Add(taxaValidationResult.Error);
            }

            if (filter.Taxon?.RedListCategories?.Any() ?? false)
            {
                errors.AddRange(filter.Taxon.RedListCategories
                    .Where(rc => !new[] { "DD", "EX", "RE", "CR", "EN", "VU", "NT", "LC", "NA", "NE" }.Contains(rc, StringComparer.CurrentCultureIgnoreCase))
                    .Select(rc => $"Red list category doesn't exist ({rc})"));
            }
            if (filter.Date?.DateFilterType == DateFilterTypeDto.OnlyStartDate && (filter.Date?.StartDate == null || filter.Date?.EndDate == null))
            {
                errors.Add("When using OnlyStartDate as filter both StartDate and EndDate need to be specified");
            }
            if (filter.Date?.DateFilterType == DateFilterTypeDto.OnlyEndDate && (filter.Date?.StartDate == null || filter.Date?.EndDate == null))
            {
                errors.Add("When using OnlyEndDate as filter both StartDate and EndDate need to be specified");
            }
            if (errors.Count > 0) return Result.Failure(string.Join(". ", errors));
            return Result.Success();
        }

        protected Result ValidateSearchPagingArguments(int skip, int take)
        {
            var errors = new List<string>();

            if (skip < 0 || take <= 0 || take > MaxBatchSize)
            {
                errors.Add($"You can't take more than {MaxBatchSize} at a time.");
            }

            if (skip + take > ElasticSearchMaxRecords)
            {
                errors.Add($"Skip + take can't be greater than { ElasticSearchMaxRecords }");
            }

            if (errors.Count > 0) return Result.Failure(string.Join(". ", errors));
            return Result.Success();
        }

        protected Result ValidateSearchPagingArgumentsInternal(int skip, int take)
        {
            var errors = new List<string>();

            if (skip + take > ElasticSearchMaxRecords)
            {
                errors.Add($"Skip + take can't be greater than { ElasticSearchMaxRecords }");
            }

            if (errors.Count > 0) return Result.Failure(string.Join(". ", errors));
            return Result.Success();
        }

        protected Result ValidateTaxa(IEnumerable<int> taxonIds)
        {
            var missingTaxa = taxonIds?
                .Where(tid => !_taxonManager.TaxonTree.TreeNodeById.ContainsKey(tid))
                .Select(tid => $"TaxonId doesn't exist ({tid})");

            return missingTaxa?.Any() ?? false ? 
                Result.Failure(string.Join(". ", missingTaxa))
                :
                Result.Success();
        }

        /// <summary>
        /// Make sure filter contains taxa
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="allowUnderlyingTaxa"></param>
        /// <returns></returns>
        protected Result ValidateTaxonExists(SearchFilterBaseDto filter)
        {
            var taxonCount = filter?.Taxon?.Ids?.Count() ?? 0;
            return taxonCount == 0 
                ? Result.Failure("You must provide taxon id's") 
                : Result.Success();
        }

        /// <summary>
        /// Make sure geographical data 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        protected Result ValidateGeographicalAreaExists(GeographicsFilterDto filter)
        {
            if ((!filter?.Areas?.Any() ?? true) && (!filter?.Geometries?.Any() ?? true) && filter?.BoundingBox?.TopLeft == null && filter?.BoundingBox?.BottomRight == null)
            {
                return Result.Failure("You must provide area/s, geometry or bounding box");
            }

            return Result.Success();
        }

        protected Result ValidateTranslationCultureCode(string translationCultureCode)
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

        protected ObservationBaseController(IObservationManager observationManager,
            ITaxonManager taxonManager)
        {
            ObservationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
            _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
        }
    }
}
