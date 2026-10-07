using Hangfire;
using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Api.Contracts.Exports.Requests;
using SOS.Api.Contracts.Observations;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.GeoJson;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Models.Search.Result;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;
using System.IO.Compression;
using System.Text.Json;
using Xunit;

namespace SOS.Lib.UnitTests.IO;

public class GeoJsonDiffusionStatusTests
{
    [Theory]
    [InlineData(DiffusionStatus.NotDiffused, 0)]
    [InlineData(DiffusionStatus.DiffusedBySystem, 1)]
    [InlineData(DiffusionStatus.DiffusedByProvider, 2)]
    public async Task Export_PreservesNumericDiffusionStatus(DiffusionStatus status, int expected)
    {
        var observation = new Observation
        {
            DiffusionStatus = status,
            Occurrence = new Occurrence { OccurrenceId = "observation-1" },
            Location = new Location { DecimalLongitude = 18, DecimalLatitude = 59 }
        };
        var filter = new SearchFilter();
        filter.Output.Fields = ["diffusionStatus", "occurrence.occurrenceId"];
        var repository = new Mock<IProcessedObservationCoreRepository>();
        repository.Setup(r => r.GetMatchCountAsync(It.IsAny<SearchFilter>(), It.IsAny<bool>())).ReturnsAsync(1);
        repository.Setup(r => r.GetChunkAsync<Observation>(
            It.IsAny<SearchFilter>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SearchExecutionOptions>()))
            .ReturnsAsync(new PagedResult<Observation> { Records = [observation], TotalCount = 1 });
        repository.SetupSequence(r => r.GetObservationsBySearchAfterAsync<Observation>(
            It.IsAny<SearchFilter>(), It.IsAny<string>(), It.IsAny<ICollection<FieldValue>>()))
            .ReturnsAsync(new SearchAfterResult<Observation, ICollection<FieldValue>> { Records = [observation] })
            .ReturnsAsync(new SearchAfterResult<Observation, ICollection<FieldValue>> { Records = [] });
        var writer = new GeoJsonFileWriter(repository.Object, Mock.Of<IFileService>(),
            Mock.Of<IVocabularyValueResolver>(), Mock.Of<IGeneralizationResolver>(), NullLogger<GeoJsonFileWriter>.Instance);

        var (stream, _) = await writer.CreateFileInMemoryAsZipStreamAsync(filter, null, "sv-SE", false,
            PropertyLabelType.PropertyName, false, JobCancellationToken.Null);

        using (stream)
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
        using (var content = archive.Entries.Single(entry => entry.Name.EndsWith(".geojson")).Open())
        using (var document = await JsonDocument.ParseAsync(content, cancellationToken: TestContext.Current.CancellationToken))
        {
            Assert.Equal("FeatureCollection", document.RootElement.GetProperty("type").GetString());
            var feature = Assert.Single(document.RootElement.GetProperty("features").EnumerateArray());
            var property = feature.GetProperty("properties").EnumerateObject()
                .Single(property => property.Name.Equals("diffusionStatus", StringComparison.OrdinalIgnoreCase));
            Assert.Equal(expected, property.Value.GetInt32());
            Assert.Equal(new[] { 18d, 59d }, feature.GetProperty("geometry").GetProperty("coordinates")
                .EnumerateArray().Select(coordinate => coordinate.GetDouble()));
        }
    }
}
