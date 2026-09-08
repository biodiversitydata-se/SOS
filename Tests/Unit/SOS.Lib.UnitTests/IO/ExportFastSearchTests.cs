using Hangfire;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.Csv;
using SOS.Lib.IO.Excel;
using SOS.Lib.IO.GeoJson;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;
using Xunit;

namespace SOS.Lib.UnitTests.IO;

public class ExportFastSearchTests
{
    [Theory]
    [InlineData(ExportFormat.Csv, 17)]
    [InlineData(ExportFormat.Csv, 2500)]
    [InlineData(ExportFormat.Csv, 2501)]
    [InlineData(ExportFormat.Excel, 17)]
    [InlineData(ExportFormat.Excel, 2500)]
    [InlineData(ExportFormat.Excel, 2501)]
    [InlineData(ExportFormat.GeoJson, 17)]
    [InlineData(ExportFormat.GeoJson, 2500)]
    [InlineData(ExportFormat.GeoJson, 2501)]
    public async Task WritersUseBoundedSingleSearchWithKnownCountOrPageLargerResults(ExportFormat format, int count)
    {
        var repository = new Mock<IProcessedObservationCoreRepository>(MockBehavior.Strict);
        var filter = new SearchFilter { UseSearchAfterWithoutPit = true };
        filter.Output.Fields = new List<string> { "occurrence.occurrenceId" };
        repository.Setup(r => r.GetMatchCountAsync(filter, false)).ReturnsAsync(count);
        if (format == ExportFormat.Excel)
            repository.Setup(r => r.GetProjectIdsAsync(filter)).ReturnsAsync(Array.Empty<int>());

        // Stop at retrieval to test the writer's paging decision independently of file serialization.
        var stop = new InvalidOperationException("Retrieval reached");
        if (count <= 2500)
        {
            if (format == ExportFormat.GeoJson)
                repository.Setup(r => r.GetChunkAsync<Observation>(filter, 0, 2500, false, count)).ThrowsAsync(stop);
            else
                repository.Setup(r => r.GetChunkAsync<object>(filter, 0, 2500, false, count)).ThrowsAsync(stop);
        }
        else
        {
            repository.Setup(r => r.GetObservationsBySearchAfterAsync<Observation>(filter, null, null)).ThrowsAsync(stop);
        }

        var fileService = Mock.Of<IFileService>();
        var vocabularyResolver = Mock.Of<IVocabularyValueResolver>();
        var generalizationResolver = Mock.Of<IGeneralizationResolver>();
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            switch (format)
            {
                case ExportFormat.Csv:
                    var csv = new CsvFileWriter(repository.Object, fileService, Mock.Of<IFilterManager>(),
                        vocabularyResolver, generalizationResolver, Mock.Of<IAreaCache>(),
                        Mock.Of<ICache<int, Taxon>>(), NullLogger<CsvFileWriter>.Instance);
                    await csv.CreateFileInMemoryAsZipStreamAsync(filter, null, "sv-SE", PropertyLabelType.PropertyName, JobCancellationToken.Null);
                    break;
                case ExportFormat.Excel:
                    var excel = new ExcelFileWriter(repository.Object, Mock.Of<IProjectManager>(), fileService,
                        vocabularyResolver, generalizationResolver, NullLogger<ExcelFileWriter>.Instance);
                    await excel.CreateFileInMemoryAsync(filter, null, "sv-SE", PropertyLabelType.PropertyName, false, true, JobCancellationToken.Null);
                    break;
                case ExportFormat.GeoJson:
                    var geoJson = new GeoJsonFileWriter(repository.Object, fileService,
                        vocabularyResolver, generalizationResolver, NullLogger<GeoJsonFileWriter>.Instance);
                    await geoJson.CreateFileInMemoryAsZipStreamAsync(filter, null, "sv-SE", true, PropertyLabelType.PropertyName, false, JobCancellationToken.Null);
                    break;
            }
        });

        Assert.Same(stop, exception);
        repository.VerifyAll();
        Assert.Equal(format == ExportFormat.Excel ? 3 : 2, repository.Invocations.Count);
    }
}
