using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Gis;
using SOS.Lib.Models.Search.Filters;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;
using System.IO;
using System;
using FeatureCollection = NetTopologySuite.Features.FeatureCollection;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.Diffusion
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class CreateDiffusedFiles
    {
        private readonly ApiIntegrationTestFixture _fixture;
        private JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()
            }
        };

        /// <summary>
        /// Get grid cells
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="gridCellsInMeters"></param>
        /// <returns></returns>
        private async Task<IEnumerable<GridCell>> GetGridCellAsync(SearchFilter filter, int gridCellsInMeters)
        {
            var gridCells = new List<GridCell>();
            const int pageSize = 10000;
            var result = await _fixture.ProcessedObservationRepository.GetMetricGridAggregationAsync(filter, gridCellsInMeters, MetricCoordinateSys.SWEREF99_TM, true, pageSize);
            while ((result?.GridCellCount ?? 0) > 0)
            {
                gridCells.AddRange(result!.GridCells);
                result = await _fixture.ProcessedObservationRepository.GetMetricGridAggregationAsync(filter, gridCellsInMeters, MetricCoordinateSys.SWEREF99_TM, true, pageSize, result.AfterKey);
            }

            return gridCells;
        }

        private async Task SaveFileAsync(FeatureCollection featureCollection, int sensitivityCategory)
        {
            var geoJson = JsonSerializer.Serialize(featureCollection, _jsonSerializerOptions);
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            using var outputFile = new StreamWriter(Path.Combine(path, $"DiffusedObservations_{(sensitivityCategory == 0 ? "all" : sensitivityCategory)}.json"));
            await outputFile.WriteAsync(geoJson);
            outputFile.Close();
        }

        public CreateDiffusedFiles(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task CreateDifussedFiles()
        {
            var featureCollectionAll = new FeatureCollection();
            for (var i = 3; i < 6; i++)
            {
                var searchFilter = new SearchFilter(-1, ProtectionFilter.Sensitive)
                {
                    SensitivityCategories = new []{ i }
                };

                var gridCellsInMeters = i switch
                {
                    3 => 5000,
                    4 => 25000,
                    _ => 50000
                };

                var gridCells = await GetGridCellAsync(searchFilter, gridCellsInMeters);

                var gridCellFeatures = gridCells.Select(gc => gc.MetricBoundingBox
                    .ToPolygon()
                    .ToFeature(new Dictionary<string, object>()
                    {
                        {  "id", GeoJsonHelper.GetGridCellId(gridCellsInMeters, (int)gc.MetricBoundingBox.TopLeft.X, (int)gc.MetricBoundingBox.BottomRight.Y) }
                    })
                );

                var featureCollection = new FeatureCollection();

                // Add all grid cells features
                foreach (var gridCellFeature in gridCellFeatures.OrderBy(gc => gc.Attributes["id"]))
                {
                    gridCellFeature.Geometry = gridCellFeature.Geometry.Transform(CoordinateSys.SWEREF99_TM, CoordinateSys.WGS84);
                    featureCollectionAll.Add(gridCellFeature);
                    featureCollection.Add(gridCellFeature);

                    
                }
                await SaveFileAsync(featureCollection, i);
            }
            await SaveFileAsync(featureCollectionAll, 0);
        }
    }
}