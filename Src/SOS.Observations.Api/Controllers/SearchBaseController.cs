using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Mvc;
using SOS.Lib.Configuration.ObservationApi;
using SOS.Lib.Extensions;
using SOS.Lib.Exceptions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    /// Base controller for observation related controller
    /// </summary>
    public class SearchBaseController : ControllerBase
    {
        private const int MaxBatchSize = 1000;
        private const int ElasticSearchMaxRecords = 10000;
        private const int ElasticSearchMaxRecordsInternal = 100000;
        protected readonly IAreaManager AreaManager;
        private ObservationApiConfiguration _observationApiConfiguration;
        

        protected string ReplaceDomain(string str, string domain, string path)
        {
            // This is a bad solution to fix problems when behind load balancer...
            return Regex.Replace(str, string.Format(@"(https?:\/\/.*?)(\/{0}\/v2)?(\/.*)", path), m => domain + m.Groups[3].Value);
        }

        protected void CheckAuthorization(bool sensitiveObservations)
        {
            if (sensitiveObservations && (!User?.HasAccessToScope(_observationApiConfiguration.ProtectedScope) ?? true))
            {
                throw new AuthenticationRequiredException("Not authorized");
            }
        }

        /// <summary>
        /// Get current users e-mail address
        /// </summary>
        protected string UserEmail => User?.Claims?.FirstOrDefault(c => c.Type.Contains("emailaddress", StringComparison.CurrentCultureIgnoreCase))?.Value;

        /// <summary>
        /// Get id of current user
        /// </summary>
        protected int UserId => int.Parse(User?.Claims?.FirstOrDefault(c => c.Type.Contains("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", StringComparison.CurrentCultureIgnoreCase))?.Value ?? "0");

        /// <summary>
        /// Check if passed areas exists
        /// </summary>
        /// <param name="areaIds"></param>
        /// <returns></returns>
        protected async Task<Result> ValidateAreasAsync(IEnumerable<AreaFilterDto> areaIds)
        {
            if (!areaIds?.Any() ?? true)
            {
                return Result.Success();
            }
            
            var existingAreaIds = (await AreaManager.GetAreasAsync(areaIds.Select(a => (a.AreaType, a.FeatureId))))
                .Select(a => new AreaFilterDto { AreaType = a.AreaType, FeatureId = a.FeatureId });

            var missingAreas = areaIds?
                .Where(aid => !existingAreaIds.Any(a => a.AreaType.Equals(aid.AreaType) && a.FeatureId.Equals(aid.FeatureId, StringComparison.CurrentCultureIgnoreCase)))
                .Select(aid => $"Area doesn't exist (AreaType: { aid.AreaType }, FeatureId: {aid.FeatureId})");

            return missingAreas?.Any() ?? false ?
                Result.Failure(string.Join(". ", missingAreas))
                :
                Result.Success();
        }

        /// <summary>
        /// Validate bounding box
        /// </summary>
        /// <param name="boundingBox"></param>
        /// <param name="mandatory"></param>
        /// <returns></returns>
        protected Result ValidateBoundingBox(
            LatLonBoundingBoxDto boundingBox,
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

        protected Result ValidateGeogridZoomArgument(int zoom, int minLimit, int maxLimit)
        {
            if (zoom < minLimit || zoom > maxLimit)
            {
                return Result.Failure($"Zoom must be between {minLimit} and {maxLimit}");
            }

            return Result.Success();
        }

        protected Result ValidateGridCellSizeInMetersArgument(int gridCellSizeInMeters, int minLimit, int maxLimit)
        {
            if (gridCellSizeInMeters < minLimit || gridCellSizeInMeters > maxLimit)
            {
                return Result.Failure($"Grid cell size in meters must be between {minLimit} and {maxLimit}");
            }

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

        protected Result ValidateEncryptPassword(string password, string confirmPassword, bool sensitiveObservations)
        {
            var errors = new List<string>();
            if (!password?.Any() ?? sensitiveObservations)
            {
                errors.Add("You need to state a encrypt password when you are requesting sensitive observations");
            }

            if ((password?.Trim().Length ?? 0) < 10)
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

        protected virtual Result ValidateSearchFilter(SearchFilterBaseDto filter, bool bboxMandatory = false)
        {
            var errors = new List<string>();

            var searchFilter = filter as SearchFilterDto;

            var areaValidationResult = ValidateAreasAsync(filter?.Geographics?.Areas).Result;
            if (areaValidationResult.IsFailure)
            {
                errors.Add(areaValidationResult.Error);
            }

            var bboxResult = ValidateBoundingBox(filter?.Geographics?.BoundingBox, bboxMandatory);
            if (bboxResult.IsFailure)
            {
                errors.Add(bboxResult.Error);
            }

            if (searchFilter?.ModifiedDate?.From > searchFilter?.ModifiedDate?.To)
            {
                errors.Add("Modified from date can't be greater tha to date");
            }

            if (searchFilter?.Output?.Fields?.Any() ?? false)
            {
                errors.AddRange(searchFilter.Output.Fields
                    .Where(of => !typeof(Observation).HasProperty(of))
                    .Select(of => $"Output field doesn't exist ({of})"));
            }

            var searchFilterInternal = filter as SearchFilterInternalDto;
            if (searchFilterInternal?.Output?.Fields?.Any() ?? false)
            {
                errors.AddRange(searchFilterInternal.Output.Fields
                    .Where(of => !typeof(Observation).HasProperty(of))
                    .Select(of => $"Output field doesn't exist ({of})"));
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

            if (skip < 0)
            {
                errors.Add("Skip must be positive.");
            }

            if (take <= 0)
            {
                errors.Add("You must take at least 1 observation.");
            }

            if (take > MaxBatchSize)
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

            if (skip + take > ElasticSearchMaxRecordsInternal)
            {
                errors.Add($"Skip + take can't be greater than { ElasticSearchMaxRecordsInternal }");
            }

            if (errors.Count > 0) return Result.Failure(string.Join(". ", errors));
            return Result.Success();
        }

        /// <summary>
        /// Make sure filter contains taxa
        /// </summary>
        /// <param name="filter"></param>
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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaManager"></param>
        /// <param name="observationApiConfiguration"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected SearchBaseController(IAreaManager areaManager, ObservationApiConfiguration observationApiConfiguration)
        {
            AreaManager = areaManager ?? throw new ArgumentNullException(nameof(areaManager));
            _observationApiConfiguration = observationApiConfiguration ?? throw new ArgumentNullException(nameof(observationApiConfiguration));
        }
    }
}
