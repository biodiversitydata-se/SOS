using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Managers.Interfaces;
using SOS.Observations.Api.Configuration;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Managers.Interfaces;

namespace SOS.Observations.Api.Controllers
{
    /// <summary>
    /// Base controller for observation related controller
    /// </summary>
    public class ObservationBaseController : SearchBaseController
    {
        private readonly ITaxonManager _taxonManager;
        protected readonly IObservationManager ObservationManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="observationManager"></param>
        /// <param name="areaManager"></param>
        /// <param name="taxonManager"></param>
        /// <param name="observationApiConfiguration"></param>
        /// <exception cref="ArgumentNullException"></exception>
        protected ObservationBaseController(IObservationManager observationManager,
            IAreaManager areaManager, ITaxonManager taxonManager, ObservationApiConfiguration observationApiConfiguration) : base(areaManager, observationApiConfiguration)
        {
            ObservationManager = observationManager ?? throw new ArgumentNullException(nameof(observationManager));
            _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
        }

        /// <summary>
        /// Get bounding box
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="autoAdjustBoundingBox"></param>
        /// <returns></returns>
        protected async Task<LatLonBoundingBoxDto> GetBoundingBoxAsync(
            GeographicsFilterDto filter,
            bool autoAdjustBoundingBox = true)
        {
            // Get geometry of sweden economic zone
            var swedenGeometry = await AreaManager.GetGeometryAsync(AreaType.EconomicZoneOfSweden, "1");

            // Get bounding box of swedish economic zone
            var swedenBoundingBox = swedenGeometry.ToGeometry().EnvelopeInternal;
            var userBoundingBox = new Envelope(new[]
            {
                new Coordinate(filter?.BoundingBox?.TopLeft?.Longitude ?? 0, filter?.BoundingBox?.TopLeft?.Latitude ?? 90),
                new Coordinate(filter?.BoundingBox?.BottomRight?.Longitude ?? 90, filter?.BoundingBox?.TopLeft?.Latitude ?? 90),
                new Coordinate(filter?.BoundingBox?.BottomRight?.Longitude ?? 90, filter?.BoundingBox?.BottomRight?.Latitude ?? 0),
                new Coordinate(filter?.BoundingBox?.TopLeft?.Longitude ?? 0, filter?.BoundingBox?.BottomRight?.Latitude ?? 0),
            });
            var boundingBox = swedenBoundingBox.Intersection(userBoundingBox);

            if (autoAdjustBoundingBox)
            {
                // If areas passed, adjust bounding box to them
                if (filter?.Areas?.Any() ?? false)
                {
                    var areas = await AreaManager.GetAreasAsync(filter.Areas.Select(a => (a.AreaType, a.FeatureId)));
                    var areaGeometries = areas?.Select(a => a.BoundingBox.GetPolygon().ToGeoShape());
                    foreach (var areaGeometry in areaGeometries!)
                    {
                        boundingBox = boundingBox.AdjustByShape(areaGeometry, filter.MaxDistanceFromPoint);
                    }
                }

                // If geometries passed, adjust bounding box to them
                if (filter?.Geometries?.Any() ?? false)
                {
                    foreach (var areaGeometry in filter.Geometries)
                    {
                        boundingBox = boundingBox.AdjustByShape(areaGeometry, filter.MaxDistanceFromPoint);
                    }
                }
            }

            return LatLonBoundingBoxDto.Create(boundingBox);
        }

        protected async Task<SearchFilterBaseDto> InitializeSearchFilterAsyncx(SearchFilterBaseDto filter)
        {

            filter ??= new SearchFilterDto();
            filter.Geographics ??= new GeographicsFilterDto();
            filter.Geographics.BoundingBox = await GetBoundingBoxAsync(filter.Geographics);
            return filter;
        }

        protected async Task<T> InitializeSearchFilterAsync<T>(T filter) where T : SearchFilterBaseDto
        {
            filter ??= new SearchFilterBaseDto() as T;
            filter.Geographics ??= new GeographicsFilterDto();
            filter.Geographics.BoundingBox = await GetBoundingBoxAsync(filter.Geographics);
            return filter;
        }

        protected string ReplaceDomain(string str, string domain, string path)
        {
            // This is a bad solution to fix problems when behind load balancer...
            return Regex.Replace(str, string.Format(@"(https?:\/\/.*?)(\/{0}\/v2)?(\/.*)", path), m => domain + m.Groups[3].Value);
        }

        protected Result ValidateAggregationPagingArguments(int skip, int? take, bool handleInfinityTake = false)
        {
            if (skip < 0) return Result.Failure("Skip must be 0 or greater.");

            if (handleInfinityTake && take == -1)
            {
                return Result.Success();
            }

            if (take <= 0) return Result.Failure("Take must be greater than 0");
            if (take != null && skip + take > ObservationManager.MaxNrElasticSearchAggregationBuckets)
                return Result.Failure($"Skip+Take={skip + take}. Skip+Take must be less than or equal to {ObservationManager.MaxNrElasticSearchAggregationBuckets}.");

            return Result.Success();
        }

        protected override Result ValidateSearchFilter(SearchFilterBaseDto filter, bool bboxMandatory = false)
        {
            var result = base.ValidateSearchFilter(filter, bboxMandatory);

            var taxaValidationResult = ValidateTaxa(filter.Taxon?.Ids);
            if (taxaValidationResult.IsFailure)
            {
                if (result.IsFailure)
                {
                    return Result.Failure($"{result.Error}, {taxaValidationResult.Error}");
                }
                return taxaValidationResult;
            }

            return result;
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
    }
}
