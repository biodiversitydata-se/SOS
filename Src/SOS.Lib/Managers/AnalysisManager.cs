using AgileObjects.AgileMapper.Extensions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Nest;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NReco.Csv;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Export;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search.Enums;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.IO;
using SOS.Lib.Models.Analysis;
using System.Diagnostics;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace SOS.Lib.Managers
{
    public class AnalysisManager : IAnalysisManager
    {
        private readonly IProcessedObservationCoreRepository _processedObservationRepository;
        private readonly ITaxonManager _taxonManager;
        private readonly IFilterManager _filterManager;
        private readonly IAreaCache _areaCache;
        private readonly IFileService _fileService;
        private readonly ILogger<AnalysisManager> _logger;

        /// <summary>
        /// Calculate some metadata for grid cells
        /// </summary>
        /// <param name="gridCells"></param>
        /// <returns></returns>
        private (long ObservationsCount, double MinX, double MaxX, double MinY, double MaxY) CalculateMetadata(IEnumerable<GridCell> gridCells)
        {
            double? minX = null, minY = null, maxX = null, maxY = null;
            var observationsCount = 0L;
            foreach (var gridCell in gridCells)
            {
                observationsCount += gridCell.ObservationsCount ?? 0;
                if (minY == null || gridCell.MetricBoundingBox.BottomRight.Y < minY)
                {
                    minY = gridCell.MetricBoundingBox.BottomRight.Y;
                }
                if (maxX == null || gridCell.MetricBoundingBox.BottomRight.X > maxX)
                {
                    maxX = gridCell.MetricBoundingBox.BottomRight.X;
                }
                if (maxY == null || gridCell.MetricBoundingBox.TopLeft.Y > maxY)
                {
                    maxY = gridCell.MetricBoundingBox.TopLeft.Y;
                }
                if (minX == null || gridCell.MetricBoundingBox.TopLeft.X < minX)
                {
                    minX = gridCell.MetricBoundingBox.TopLeft.X;
                }
            }

            return (observationsCount, minX ?? 0.0, maxX ?? 0.0, minY ?? 0.0, maxY ?? 0.0);
        }

        /// <summary>
        /// Get grid cells
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="authorizationApplicationIdentifier"></param>
        /// <param name="filter"></param>
        /// <param name="gridCellsInMeters"></param>
        /// <param name="metricCoordinateSys"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private async Task<IEnumerable<GridCell>> GetGridCellsAync(int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int gridCellsInMeters,
            MetricCoordinateSys metricCoordinateSys,
            TimeSpan? timeout = null)
        {            
            await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
            var gridCells = new List<GridCell>();
            const int pageSize = 10000;
            var stopwatch = Stopwatch.StartNew();
            var result = await _processedObservationRepository.GetMetricGridAggregationAsync(filter, gridCellsInMeters, metricCoordinateSys, false, pageSize, null, timeout);
            
            while ((result?.GridCellCount ?? 0) > 0)
            {
                stopwatch.Stop();
                var delayMs = stopwatch.ElapsedMilliseconds / 10;                
                await Task.Delay(TimeSpan.FromMilliseconds(delayMs));                
                stopwatch = Stopwatch.StartNew();
                gridCells.AddRange(result!.GridCells);
                result = await _processedObservationRepository.GetMetricGridAggregationAsync(filter, gridCellsInMeters, metricCoordinateSys, false, pageSize, result.AfterKey, timeout);
            }

            return gridCells;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filterManager"></param>
        /// <param name="processedObservationRepository"></param>
        /// <param name="areaCache"></param>
        /// <param name="fileService"></param>
        /// <param name="taxonManager"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AnalysisManager(
            IFilterManager filterManager,
            IProcessedObservationCoreRepository processedObservationRepository,
            IAreaCache areaCache,
            IFileService fileService,
            ITaxonManager taxonManager,
            ILogger<AnalysisManager> logger)
        {
            _filterManager = filterManager ?? throw new ArgumentNullException(nameof(filterManager));
            _processedObservationRepository = processedObservationRepository ?? throw new ArgumentNullException(nameof(processedObservationRepository));
            _areaCache = areaCache ?? throw new ArgumentNullException(nameof(areaCache));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _taxonManager = taxonManager ?? throw new ArgumentNullException(nameof(taxonManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<PagedAggregationResult<UserAggregationResponse>?> AggregateByUserFieldAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            string aggregationField,
            int? precisionThreshold,
            string afterKey,
            int? take)
        {
            await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
            var result = await _processedObservationRepository.AggregateByUserFieldAsync(filter, aggregationField, precisionThreshold, afterKey, take);


            return new PagedAggregationResult<UserAggregationResponse>
            {
                AfterKey = result.SearchAfter,
                Records = result?.Records?.Select(r => new UserAggregationResponse
                {
                    AggregationField = r.AggregationField,
                    Count = (int)r.DocCount,
                    UniqueTaxon = (int)r.UniqueTaxon
                })!
            };
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AggregationItem>?> AggregateByUserFieldAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            string aggregationField,
            int? precisionThreshold,
            int take,
            AggregationSortOrder sortOrder = AggregationSortOrder.CountDescending)
        {
            await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
            var result = await _processedObservationRepository.GetAggregationItemsAsync(filter, aggregationField, precisionThreshold ?? 40000, take, sortOrder);

            return result?.Select(i => new AggregationItem { AggregationKey = i.AggregationKey, DocCount = i.DocCount })!;
        }

        /// <inheritdoc/>
        public async Task<FeatureCollection> AtlasAggregateAsync(
        int? roleId,
        string authorizationApplicationIdentifier,
        SearchFilter filter,
        AtlasAreaSize atlasSize)
        {
            await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
            var aggregationField = $"location.{(atlasSize == AtlasAreaSize.Km5x5 ? "atlas5x5" : "atlas10x10")}.featureId";

            var result = await _processedObservationRepository.AggregateByUserFieldAsync(filter, aggregationField, precisionThreshold: 40000, afterKey: null, 10000);
            var futureCollection = new FeatureCollection();
            while (result?.Records?.Any() ?? false)
            {
                foreach (var record in result.Records)
                {
                    var area = (IGeoShape)await _areaCache.GetGeometryAsync(atlasSize switch { AtlasAreaSize.Km5x5 => AreaType.Atlas5x5, _ => AreaType.Atlas10x10 }, record.AggregationField);
                    futureCollection.Add(
                        area.ToFeature(new Dictionary<string, object> {
                            { "observationCount", (int)record.DocCount },
                            { "taxonCount", (int)record.UniqueTaxon }
                        })
                    );
                }

                result = await _processedObservationRepository.AggregateByUserFieldAsync(filter, aggregationField, precisionThreshold: 40000, afterKey: (string)result.SearchAfter?.FirstOrDefault()!, 10000);
            }

            return futureCollection;
        }

        /// <inheritdoc/>
        public async Task<(FeatureCollection FeatureCollection, List<AooEooItem> AooEooItems)?> CalculateAooAndEooAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int gridCellsInMeters,
            bool useCenterPoint,
            IEnumerable<double> alphaValues,
            bool useEdgeLengthRatio,
            bool allowHoles,
            bool returnGridCells,
            bool includeEmptyCells,
            MetricCoordinateSys metricCoordinateSys,
            CoordinateSys coordinateSystem,
            TimeSpan? timeout = null)
        {
            try
            {
                var aooEooItems = new List<AooEooItem>();
                var gridCells = await GetGridCellsAync(roleId, authorizationApplicationIdentifier, filter, gridCellsInMeters, metricCoordinateSys, timeout);
                if (!gridCells.Any())
                {
                    return null!;
                }

                var metaData = CalculateMetadata(gridCells);
                var featureCollection = new FeatureCollection
                {
                    BoundingBox = new Envelope(new Coordinate(metaData.MinX, metaData.MaxY), new Coordinate(metaData.MaxX, metaData.MinY)).Transform((CoordinateSys)metricCoordinateSys, coordinateSystem)
                };

                // We need features to return later so we create them now 
                var gridCellFeaturesMetric = gridCells.Select(gc => gc.MetricBoundingBox
                    .ToPolygon()
                    .ToFeature(new Dictionary<string, object>()
                    {
                        {  "id", GeoJsonHelper.GetGridCellId(gridCellsInMeters, (int)gc.MetricBoundingBox.TopLeft.X, (int)gc.MetricBoundingBox.BottomRight.Y) },
                        {  "observationsCount", gc.ObservationsCount! },
                        {  "taxaCount", gc.TaxaCount! }
                    })
                ).ToDictionary(f => (string)f.Attributes["id"], f => f);

                var metricEooGeometries = new Dictionary<double, Geometry>();
                foreach (var alphaValue in alphaValues)
                {
                    Geometry eooGeometry = null;
                    try
                    {
                        eooGeometry = gridCellFeaturesMetric
                            .Select(f => f.Value.Geometry as Polygon)
                                .ToArray()
                                    .ConcaveHull(useCenterPoint, alphaValue, useEdgeLengthRatio, allowHoles);
                    }
                    catch (Exception ex)
                    {
                        // There is a bug in Nettopologysuite, try useCentPoint=false instead.
                        _logger.LogError(ex, $"Error when calculating ConcaveHull(). Taxa filter: {string.Join(", ", filter?.Taxa?.Ids ?? Enumerable.Empty<int>())}");
                        if (useCenterPoint)
                        {
                            _logger.LogInformation("Try calculate ConcaveHull() with useCenterPoint=false");
                            try
                            {
                                eooGeometry = gridCellFeaturesMetric
                                    .Select(f => f.Value.Geometry as Polygon)
                                        .ToArray()
                                            .ConcaveHull(false, alphaValue, useEdgeLengthRatio, allowHoles);
                            }
                            catch(Exception e)
                            {
                                _logger.LogError(e, $"Error when calculating ConcaveHull() with useCenterPoint=false.");
                            }
                        }
                    }


                    if (eooGeometry == null)
                    {
                        return null!;
                    }
                    metricEooGeometries.Add(alphaValue, eooGeometry);

                    var gridCellArea = gridCellsInMeters * gridCellsInMeters / 1000000; //Calculate area in km2
                    var aoo = Math.Round((double)gridCells.Count() * gridCellArea, 0);
                    var eoo = Math.Round(eooGeometry.Area / 1000000, 0);
                    var aooEooItem = new AooEooItem
                    {
                        Id = $"eoo-{alphaValue.ToString(CultureInfo.InvariantCulture).Replace(',', '.')}",
                        AlphaValue = alphaValue,
                        Aoo = (int)aoo,
                        Eoo = (int)eoo,
                        GridCellArea = gridCellArea,
                        GridCellAreaUnit = "km2",
                        ObservationsCount = (int)metaData.ObservationsCount
                    };
                    aooEooItems.Add(aooEooItem);

                    featureCollection.Add(new Feature(
                        eooGeometry.Transform((CoordinateSys)metricCoordinateSys, coordinateSystem),
                        new AttributesTable(new KeyValuePair<string, object>[] {
                                new KeyValuePair<string, object>("id", aooEooItem.Id),
                                new KeyValuePair<string, object>("aoo", aooEooItem.Aoo),
                                new KeyValuePair<string, object>("eoo", aooEooItem.Eoo),
                                new KeyValuePair<string, object>("gridCellArea", aooEooItem.GridCellArea),
                                new KeyValuePair<string, object>("gridCellAreaUnit", aooEooItem.GridCellAreaUnit),
                                new KeyValuePair<string, object>("observationsCount", aooEooItem.ObservationsCount)
                            }
                        )
                    ));
                }

                if (!returnGridCells)
                {
                    return (featureCollection, aooEooItems);
                }

                // Add empty grid cell where no observation was found too complete grid
                if (includeEmptyCells && !useEdgeLengthRatio)
                {
                    GeoJsonHelper.FillInBlanks(
                        gridCellFeaturesMetric,
                        metricEooGeometries,
                        gridCellsInMeters, new[] {
                            new KeyValuePair<string, object>("observationsCount", 0),
                            new KeyValuePair<string, object>("taxaCount", 0)
                        },
                        useCenterPoint
                    );
                }

                // Add all grid cells features
                foreach (var gridCellFeatureMetric in gridCellFeaturesMetric.OrderBy(gc => gc.Key).Select(f => f.Value))
                {
                    gridCellFeatureMetric.Geometry = gridCellFeatureMetric.Geometry.Transform((CoordinateSys)metricCoordinateSys, coordinateSystem);
                    featureCollection.Add(gridCellFeatureMetric);
                }

                return (featureCollection, aooEooItems);
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.LogError(e, $"Failed to calculate AOO/EOO. To many buckets. Taxa filter: {string.Join(", ", filter?.Taxa?.Ids ?? Enumerable.Empty<int>())}");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to calculate AOO/EOO. Taxa filter: {string.Join(", ", filter?.Taxa?.Ids ?? Enumerable.Empty<int>())}");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<(FeatureCollection FeatureCollection, AooEooItem AooEooItem)?> CalculateAooAndEooArticle17Async(
           int? roleId,
           string authorizationApplicationIdentifier,
           SearchFilter filter,
           int gridCellsInMeters,
           int maxDistance,
           MetricCoordinateSys metricCoordinateSys,
           CoordinateSys coordinateSystem,
           TimeSpan? timeout = null)
        {
            try
            {
                var gridCellsMetric = await GetGridCellsAync(roleId, authorizationApplicationIdentifier, filter, gridCellsInMeters, metricCoordinateSys, timeout);
                if (!gridCellsMetric.Any())
                {
                    return null!;
                }

                // We need features to return later so we create them now and don't need to create the polygon more than once
                var gridCellFeaturesMetric = gridCellsMetric.Select(gc => gc.MetricBoundingBox
                    .ToPolygon()
                    .ToFeature(new Dictionary<string, object>()
                    {
                        {  "id", GeoJsonHelper.GetGridCellId(gridCellsInMeters, (int)gc.MetricBoundingBox.TopLeft.X, (int)gc.MetricBoundingBox.BottomRight.Y) },
                        {  "observationsCount", gc.ObservationsCount! },
                        {  "taxaCount", gc.TaxaCount! }
                    })
                ).ToDictionary(f => (string)f.Attributes["id"], f => f);

                var metaData = CalculateMetadata(gridCellsMetric);
                var featureCollection = new FeatureCollection
                {
                    BoundingBox = new Envelope(new Coordinate(metaData.MinX, metaData.MaxY), new Coordinate(metaData.MaxX, metaData.MinY)).Transform((CoordinateSys)metricCoordinateSys, coordinateSystem)
                };

                var triangels = gridCellsMetric
                    .Select(gc => new XYBoundingBox()
                    {
                        BottomRight = new XYCoordinate(gc.MetricBoundingBox.BottomRight.X, gc.MetricBoundingBox.BottomRight.Y),
                        TopLeft = new XYCoordinate(gc.MetricBoundingBox.TopLeft.X, gc.MetricBoundingBox.TopLeft.Y)
                    }
                    .ToPolygon())
                        .ToArray()
                            .CalculateTraiangels(true);

                var polygonsInRange = new HashSet<Polygon>();
                if (triangels != null)
                {
                    foreach (var triangle in triangels)
                    {
                        foreach (var corrdinate in triangle.Coordinates)
                        {
                            // Save triangels and sides shorter than alpha value
                            var sidesAdded = 0;
                            if (triangle.Coordinates[0].Distance(triangle.Coordinates[1]) <= maxDistance)
                            {
                                polygonsInRange.Add(new Polygon(new LinearRing(new Coordinate[] { triangle.Coordinates[0], triangle.Coordinates[1], triangle.Coordinates[0] })));
                                sidesAdded++;
                            }
                            if (triangle.Coordinates[1].Distance(triangle.Coordinates[2]) <= maxDistance)
                            {
                                polygonsInRange.Add(new Polygon(new LinearRing(new Coordinate[] { triangle.Coordinates[1], triangle.Coordinates[2], triangle.Coordinates[1] })));
                                sidesAdded++;
                            }
                            if (triangle.Coordinates[2].Distance(triangle.Coordinates[3]) <= maxDistance)
                            {
                                polygonsInRange.Add(new Polygon(new LinearRing(new Coordinate[] { triangle.Coordinates[2], triangle.Coordinates[3], triangle.Coordinates[2] })));
                                sidesAdded++;
                            }
                            if (sidesAdded == 3)
                            {
                                polygonsInRange.Add((Polygon)triangle);
                            }
                        }
                    }
                }

                // Create a multipolygon showing matching polygons
                var inRangeGeometry = new MultiPolygon(polygonsInRange.ToArray());

                // Add empty cells
                GeoJsonHelper.FillInBlanks(
                   gridCellFeaturesMetric,
                   gridCellsInMeters, new[] {
                            new KeyValuePair<string, object>("observationsCount", 0),
                            new KeyValuePair<string, object>("taxaCount", 0)
                   }
               );

                // Add all intersections gridcells to feature collection. Add nagative buffer when intersect to prevent gridcell touching corner match
                var eooGridCellFeaturesMetric = gridCellFeaturesMetric.Where(gc =>
                    long.Parse(gc.Value?.Attributes["observationsCount"]?.ToString() ?? "0") > 0 ||
                    gc.Value!.Geometry.Intersects(inRangeGeometry)).Select(f => f.Value);
                var eooGeometry = new MultiPolygon(eooGridCellFeaturesMetric.Select(f => f.Geometry as Polygon).ToArray());

                var gridCellArea = gridCellsInMeters * (long)gridCellsInMeters / 1000000; //Calculate area in km2
                var aoo = Math.Round((double)gridCellsMetric.Count() * gridCellArea, 0);
                var eoo = Math.Round(eooGeometry.Area / 1000000, 0);

                featureCollection.Add(new Feature(
                    eooGeometry,
                    new AttributesTable(new KeyValuePair<string, object>[] {
                        new KeyValuePair<string, object>("id", "eoo"),
                        new KeyValuePair<string, object>("aoo", (int)aoo),
                        new KeyValuePair<string, object>("eoo", (int)eoo),
                        new KeyValuePair<string, object>("gridCellArea", gridCellArea),
                        new KeyValuePair<string, object>("gridCellAreaUnit", "km2"),
                        new KeyValuePair<string, object>("observationsCount", metaData.ObservationsCount)
                        }
                    )
                ));

                // Add aoo features to collection
                eooGridCellFeaturesMetric
                    .Where(f => long.Parse(f.Attributes["observationsCount"]?.ToString() ?? "0") > 0)
                        .ForEach(f => featureCollection.Add(f!));

                // Make sure all geometries is in requested coordinate system
                foreach (var feature in featureCollection)
                {
                    feature.Geometry = feature.Geometry.Transform((CoordinateSys)metricCoordinateSys, coordinateSystem);
                }

                var aooEooItem = new AooEooItem
                {
                    Id = $"eoo",                    
                    Aoo = (int)aoo,
                    Eoo = (int)eoo,
                    GridCellArea = (int)gridCellArea,
                    GridCellAreaUnit = "km2",
                    ObservationsCount = (int)metaData.ObservationsCount
                };
                return (featureCollection, aooEooItem);
            }
            catch (ArgumentOutOfRangeException e)
            {
                _logger.LogError(e, "Failed to calculate AOO/EOO. To many buckets");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to calculate AOO/EOO.");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<long> GetMatchCountAsync(int? roleId, string authorizationApplicationIdentifier, SearchFilterBase filter)
        {
            await _filterManager.PrepareFilterAsync(roleId, authorizationApplicationIdentifier, filter);
            return await _processedObservationRepository.GetMatchCountAsync(filter);
        }

        public async Task<int> GetNumberOfTaxaInFilterAsync(SearchFilterBase filter)
        {
            await _filterManager.PrepareFilterAsync(null, null, filter);
            return filter?.Taxa?.Ids != null ? filter.Taxa.Ids.Count() : 0;
        }

        public async Task<FileExportResult> CreateAooEooExportAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int gridCellsInMeters,
            bool useCenterPoint,
            IEnumerable<double> alphaValues,
            bool useEdgeLengthRatio,
            bool allowHoles,
            bool returnGridCells,
            bool includeEmptyCells,
            MetricCoordinateSys metricCoordinateSys,
            CoordinateSys coordinateSystem,
            string exportPath,
            string fileName,            
            IJobCancellationToken cancellationToken)
        {
            var temporaryZipExportFolderPath = Path.Combine(exportPath, "zip");
            TimeSpan timeout = TimeSpan.FromMinutes(10);

            try
            {
                var geoJsonWriter = new GeoJsonWriter();
                var originalTaxaList = filter.Taxa.Ids.ToList();
                var aooEooItems = new List<AooEooItem>();
                _fileService.CreateDirectory(temporaryZipExportFolderPath);
                await StoreFilterAsync(temporaryZipExportFolderPath, filter);
                await StoreSettingsAsync(temporaryZipExportFolderPath, new
                {
                    GridcellsInMeters = gridCellsInMeters,
                    UseCenterPoint = useCenterPoint,
                    AlphaValues = alphaValues,
                    UseEdgeLengthRatio = useEdgeLengthRatio,
                    AllowHoles = allowHoles,
                    IncludeEmptyCells = includeEmptyCells,
                    MetricCoordinateSys = metricCoordinateSys,
                    CoordinateSystem = coordinateSystem,
                    RoleId = roleId,
                    AuthorizationApplicationIdentifier = authorizationApplicationIdentifier,
                });
                var totalFilePath = Path.Combine(temporaryZipExportFolderPath, "Total.geojson");

                var aooEooResult = await CalculateAooAndEooAsync(
                    roleId,
                    authorizationApplicationIdentifier, 
                    filter, 
                    gridCellsInMeters, 
                    useCenterPoint, 
                    alphaValues, 
                    useEdgeLengthRatio,
                    allowHoles, 
                    returnGridCells, 
                    includeEmptyCells, 
                    metricCoordinateSys, 
                    coordinateSystem,
                    timeout);

                if (aooEooResult == null)
                {
                    aooEooItems.Add(new AooEooItem
                    {             
                        TaxonIdCaption = "-",
                        TaxonNameCaption = "Total"
                    });
                }
                else
                {
                    foreach (var item in aooEooResult.Value.AooEooItems)
                    {
                        item.TaxonIdCaption = "-";
                        item.TaxonNameCaption = "Total";
                    }

                    aooEooItems.AddRange(aooEooResult.Value.AooEooItems);
                    string geoJson = geoJsonWriter.Write(aooEooResult);
                    File.WriteAllText(totalFilePath, geoJson);
                }

                if (originalTaxaList != null && originalTaxaList.Count > 1)
                {
                    foreach (var taxonId in originalTaxaList)
                    {
                        filter.Taxa.Ids = new List<int>() { taxonId };
                        var taxonTree = await _taxonManager.GetTaxonTreeAsync();
                        var treeNode = taxonTree.GetTreeNode(taxonId);
                        if (treeNode == null)
                        {
                            _logger.LogWarning("Can't find taxon tree node with TaxonId={@taxonId}", taxonId);
                            continue;
                        }
                        Models.Interfaces.IBasicTaxon taxon = treeNode.Data;
                        string taxonName = !string.IsNullOrEmpty(taxon.VernacularName) ? $"{taxon.ScientificName} ({taxon.VernacularName})" : taxon.ScientificName;
                        string taxonFileName = FilenameHelper.GetSafeFileName(taxonName, '-');
                        var taxonFilePath = Path.Combine(temporaryZipExportFolderPath, $"{taxon.Id}-{taxonFileName}.geojson");

                        aooEooResult = await CalculateAooAndEooAsync(
                            roleId,
                            authorizationApplicationIdentifier,
                            filter,
                            gridCellsInMeters,
                            useCenterPoint,
                            alphaValues,
                            useEdgeLengthRatio,
                            allowHoles,
                            returnGridCells,
                            includeEmptyCells,
                            metricCoordinateSys,
                            coordinateSystem,
                            timeout);

                        if (aooEooResult == null)
                        {
                            aooEooItems.Add(new AooEooItem
                            {
                                TaxonId = taxon.Id,
                                VernacularName = taxon.VernacularName,
                                ScientificName = taxon.ScientificName,
                                TaxonIdCaption = taxon.Id.ToString(),
                                TaxonNameCaption = taxonName
                            });
                        }
                        else
                        {
                            foreach (var item in aooEooResult.Value.AooEooItems)
                            {
                                item.TaxonId = taxon.Id;
                                item.ScientificName = taxon.ScientificName;
                                item.VernacularName = taxon.VernacularName;
                                item.TaxonIdCaption = taxon.Id.ToString();
                                item.TaxonNameCaption = taxonName;
                                aooEooItems.Add(item);
                            }

                            string geoJson = geoJsonWriter.Write(aooEooResult);
                            geoJson = geoJsonWriter.Write(aooEooResult);
                            File.WriteAllText(taxonFilePath, geoJson);
                        }

                        await Task.Delay(200); // wait 200ms
                    }
                }

                await CreateAooEooCsvFile(Path.Combine(temporaryZipExportFolderPath, "AooEooExport.csv"), aooEooItems, alphaValues.Count() > 1);
                var zipFilePath = Path.Join(exportPath, $"{fileName}.zip");
                _fileService.CompressDirectory(temporaryZipExportFolderPath, zipFilePath);
                return new FileExportResult
                {
                    NrObservations = 0,
                    FilePath = zipFilePath
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create AOO EOO File.");
                throw;
            }
            finally
            {
                _fileService.DeleteDirectory(temporaryZipExportFolderPath);
            }
        }

        public async Task<FileExportResult> CreateAooAndEooArticle17ExportAsync(
            int? roleId,
            string authorizationApplicationIdentifier,
            SearchFilter filter,
            int gridCellsInMeters,
            int maxDistance,
            MetricCoordinateSys metricCoordinateSys,
            CoordinateSys coordinateSystem,           
            string exportPath,
            string fileName,
            IJobCancellationToken cancellationToken)
        {
            var temporaryZipExportFolderPath = Path.Combine(exportPath, "zip");
            TimeSpan timeout = TimeSpan.FromMinutes(10);

            try
            {
                var geoJsonWriter = new GeoJsonWriter();
                var originalTaxaList = filter.Taxa.Ids.ToList();
                var aooEooItems = new List<AooEooItem>();
                _fileService.CreateDirectory(temporaryZipExportFolderPath);
                await StoreFilterAsync(temporaryZipExportFolderPath, filter);
                await StoreSettingsAsync(temporaryZipExportFolderPath, new
                {
                    GridcellsInMeters = gridCellsInMeters,
                    MaxDistance = maxDistance,                    
                    MetricCoordinateSys = metricCoordinateSys,
                    CoordinateSystem = coordinateSystem,
                    RoleId = roleId,
                    AuthorizationApplicationIdentifier = authorizationApplicationIdentifier,
                });
                var totalFilePath = Path.Combine(temporaryZipExportFolderPath, "Total.geojson");

                var aooEooResult = await CalculateAooAndEooArticle17Async(
                    roleId,
                    authorizationApplicationIdentifier,
                    filter,
                    gridCellsInMeters,
                    maxDistance,
                    metricCoordinateSys,
                    coordinateSystem,
                    timeout);

                if (aooEooResult == null)
                {
                    aooEooItems.Add(new AooEooItem
                    {
                        TaxonIdCaption = "-",
                        TaxonNameCaption = "Total"
                    });
                }
                else
                {
                    aooEooResult.Value.AooEooItem.TaxonIdCaption = "-";
                    aooEooResult.Value.AooEooItem.TaxonNameCaption = "Total";
                    aooEooItems.Add(aooEooResult.Value.AooEooItem);
                    string geoJson = geoJsonWriter.Write(aooEooResult);
                    File.WriteAllText(totalFilePath, geoJson);
                }

                if (originalTaxaList != null && originalTaxaList.Count > 1)
                {
                    foreach (var taxonId in originalTaxaList)
                    {
                        filter.Taxa.Ids = new List<int>() { taxonId };
                        var taxonTree = await _taxonManager.GetTaxonTreeAsync();
                        var treeNode = taxonTree.GetTreeNode(taxonId);
                        if (treeNode == null)
                        {
                            _logger.LogWarning("Can't find taxon tree node with TaxonId={@taxonId}", taxonId);
                            continue;
                        }
                        Models.Interfaces.IBasicTaxon taxon = treeNode.Data;
                        string taxonName = !string.IsNullOrEmpty(taxon.VernacularName) ? $"{taxon.ScientificName} ({taxon.VernacularName})" : taxon.ScientificName;
                        string taxonFileName = FilenameHelper.GetSafeFileName(taxonName, '-');
                        var taxonFilePath = Path.Combine(temporaryZipExportFolderPath, $"{taxon.Id}-{taxonFileName}.geojson");
                        aooEooResult = await CalculateAooAndEooArticle17Async(
                            roleId,
                            authorizationApplicationIdentifier,
                            filter,
                            gridCellsInMeters,
                            maxDistance,
                            metricCoordinateSys,
                            coordinateSystem,
                            timeout);

                        if (aooEooResult == null)
                        {
                            aooEooItems.Add(new AooEooItem
                            {
                                TaxonId = taxon.Id,
                                VernacularName = taxon.VernacularName,
                                ScientificName = taxon.ScientificName,
                                TaxonIdCaption = taxon.Id.ToString(),
                                TaxonNameCaption = taxonName
                            });
                        }
                        else
                        {
                            aooEooResult.Value.AooEooItem.TaxonId = taxon.Id;
                            aooEooResult.Value.AooEooItem.ScientificName = taxon.ScientificName;
                            aooEooResult.Value.AooEooItem.VernacularName = taxon.VernacularName;
                            aooEooResult.Value.AooEooItem.TaxonIdCaption = taxon.Id.ToString();
                            aooEooResult.Value.AooEooItem.TaxonNameCaption = taxonName;
                            aooEooItems.Add(aooEooResult.Value.AooEooItem);
                            string geoJson = geoJsonWriter.Write(aooEooResult);
                            geoJson = geoJsonWriter.Write(aooEooResult);
                            File.WriteAllText(taxonFilePath, geoJson);
                        }

                        await Task.Delay(200); // wait 200ms
                    }
                }

                await CreateAooEooCsvFile(Path.Combine(temporaryZipExportFolderPath, "AooEooExport.csv"), aooEooItems, false);
                var zipFilePath = Path.Join(exportPath, $"{fileName}.zip");
                _fileService.CompressDirectory(temporaryZipExportFolderPath, zipFilePath);
                return new FileExportResult
                {
                    NrObservations = 0,
                    FilePath = zipFilePath
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create AOO EOO Article 17 File.");
                throw;
            }
            finally
            {
                _fileService.DeleteDirectory(temporaryZipExportFolderPath);
            }
        }

        private async Task CreateAooEooCsvFile(string filePath, IEnumerable<AooEooItem> aooEooItems, bool writeAlphaValue)
        {
            using var streamWriter = new StreamWriter(filePath, false, new UTF8Encoding(true));
            var csvWriter = new CsvWriter(streamWriter, ";");

            // Header
            csvWriter.WriteField("Id");
            csvWriter.WriteField("Caption");
            csvWriter.WriteField("Observation count");
            csvWriter.WriteField("AOO (km2)");
            csvWriter.WriteField("EOO (km2)");
            if (writeAlphaValue) csvWriter.WriteField("Alpha value");
            csvWriter.NextRecord();

            foreach (var item in aooEooItems)
            {
                csvWriter.WriteField(item.TaxonIdCaption);
                csvWriter.WriteField(item.TaxonNameCaption);
                csvWriter.WriteField(item.ObservationsCount.ToString());
                csvWriter.WriteField(item.Aoo.ToString());
                csvWriter.WriteField(item.Eoo.ToString());
                if (writeAlphaValue) csvWriter.WriteField(item.AlphaValue.ToString());

                csvWriter.NextRecord();
            }
            
            await streamWriter.FlushAsync();
        }

        /// <summary>
        /// Store filter in folder o zip
        /// </summary>
        /// <param name="temporaryZipExportFolderPath"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        protected async Task StoreFilterAsync(string temporaryZipExportFolderPath, SearchFilter filter)
        {
            try
            {
                await using var fileStream = File.Create(Path.Combine(temporaryZipExportFolderPath, "filter.json"));
                await using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
                var serializeOptions = new JsonSerializerOptions {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement), // Display å,ä,ö e.t.c. properly
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull 
                };
                serializeOptions.Converters.Add(new JsonStringEnumConverter());

                var filterString = JsonSerializer.Serialize(filter, serializeOptions);
                await streamWriter.WriteAsync(filterString);
                streamWriter.Close();
                fileStream.Close();
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// Store settings in folder o zip
        /// </summary>
        /// <param name="temporaryZipExportFolderPath"></param>
        /// <param name="settings"></param>
        /// <returns></returns>
        protected async Task StoreSettingsAsync(string temporaryZipExportFolderPath, object settings)
        {
            try
            {
                await using var fileStream = File.Create(Path.Combine(temporaryZipExportFolderPath, "settings.json"));
                await using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);                
                var serializeOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement), // Display å,ä,ö e.t.c. properly
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };
                serializeOptions.Converters.Add(new JsonStringEnumConverter());

                var settingsString = JsonSerializer.Serialize(settings, serializeOptions);
                await streamWriter.WriteAsync(settingsString);
                streamWriter.Close();
                fileStream.Close();
            }
            catch
            {
                return;
            }
        }
    }
}
