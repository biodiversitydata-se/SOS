using Hangfire;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Lib.Dtos.Enum;
using SOS.Lib.Dtos.Filter;
using SOS.Lib.Enums;
using SOS.Lib.Helpers.Interfaces;
using SOS.Lib.IO.Csv.Interfaces;
using SOS.Lib.IO.DwcArchive.Interfaces;
using SOS.Lib.IO.Excel.Interfaces;
using SOS.Lib.IO.GeoJson.Interfaces;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Search.Filters;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Observations.Api.Managers;
using SOS.Observations.Api.Repositories.Interfaces;
using Xunit;

namespace SOS.Observations.Api.UnitTests.Services;

public class ExportPaginationTests
{
    [Theory]
    [InlineData(ExportFormat.Csv, false)]
    [InlineData(ExportFormat.Csv, true)]
    [InlineData(ExportFormat.Excel, false)]
    [InlineData(ExportFormat.Excel, true)]
    [InlineData(ExportFormat.GeoJson, false)]
    [InlineData(ExportFormat.GeoJson, true)]
    [InlineData(ExportFormat.DwC, false)]
    [InlineData(ExportFormat.DwC, true)]
    [InlineData(ExportFormat.DwCEvent, false)]
    [InlineData(ExportFormat.DwCEvent, true)]
    public async Task DownloadPagingChoiceReachesWriterAndDoesNotLeakToNextRequest(ExportFormat format, bool internalFilter)
    {
        var csv = new Mock<ICsvFileWriter>();
        var excel = new Mock<IExcelFileWriter>();
        var geoJson = new Mock<IGeoJsonFileWriter>();
        var dwc = new Mock<IDwcArchiveFileWriter>();
        var dwcEvent = new Mock<IDwcArchiveEventFileWriter>();
        var filters = new Mock<IFilterManager>();
        var manager = new ExportManager(csv.Object, dwc.Object, dwcEvent.Object, excel.Object, geoJson.Object,
            Mock.Of<IProcessedObservationRepository>(), Mock.Of<IProcessInfoRepository>(), filters.Object,
            Mock.Of<IMarkdownHelper>(), NullLogger<ExportManager>.Instance);
        SearchFilterBaseDto dto = internalFilter ? new SearchFilterInternalDto() : new SearchFilterDto();

        await manager.CreateExportFileInMemoryAsZipStreamAsync(123, 42, "app", dto, ProtectionFilterDto.Public,
            format, "sv-SE", true, PropertyLabelType.PropertyName, false, false, true,
            JobCancellationToken.Null, useSearchAfterWithoutPit: true);
        await manager.CreateExportFileInMemoryAsZipStreamAsync(123, 42, "app", dto, ProtectionFilterDto.Public,
            format, "sv-SE", true, PropertyLabelType.PropertyName, false, false, true,
            JobCancellationToken.Null);

        Mock[] writers = [csv, excel, geoJson, dwc, dwcEvent];
        var exportedFilters = writers.SelectMany(w => w.Invocations)
            .SelectMany(i => i.Arguments.OfType<SearchFilter>()).ToArray();
        Assert.Equal(2, exportedFilters.Length);
        Assert.True(exportedFilters[0].UseSearchAfterWithoutPit);
        Assert.False(exportedFilters[1].UseSearchAfterWithoutPit);
        Assert.NotSame(exportedFilters[0], exportedFilters[1]);
        Assert.All(exportedFilters, filter =>
        {
            Assert.Equal(internalFilter, filter is SearchFilterInternal);
            Assert.Equal(123, filter.ExtendedAuthorization.UserId);
            Assert.Contains(filters.Invocations, i => i.Method.Name == nameof(IFilterManager.PrepareFilterAsync)
                && ReferenceEquals(i.Arguments[2], filter));
        });
    }
}
