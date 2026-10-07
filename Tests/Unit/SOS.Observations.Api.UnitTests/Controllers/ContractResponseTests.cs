using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Api.Contracts.DataStewardship.Responses;
using SOS.Api.Contracts.DataQuality.Responses;
using SOS.Api.Contracts.Exports.Responses;
using SOS.Api.Contracts.Lookups.Responses;
using SOS.Lib.Enums;
using SOS.Lib.Managers.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Repositories.Processed.Interfaces;
using SOS.Lib.Services.Interfaces;
using SOS.Observations.Api.Configuration;
using SOS.Observations.Api.Controllers;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Shared.Api.Validators.Interfaces;
using System.Text.Json;
using Xunit;

namespace SOS.Observations.Api.UnitTests.Controllers;

public class ContractResponseTests
{
    [Fact]
    public async Task DataQualityEndpoint_ReturnsReportValues()
    {
        var report = new DataQualityReport { Records = [] };
        var manager = new Mock<IDataQualityManager>();
        manager.Setup(m => m.GetReportAsync("birds")).ReturnsAsync(report);
        var controller = WithContext(new DataQualityController(manager.Object, NullLogger<DataQualityController>.Instance));

        var result = Assert.IsType<OkObjectResult>(await controller.GetReport("birds"));
        AssertJsonEqual(report, result.Value);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExportDatasetsEndpoint_ReturnsExportFileDtosOrNoContent(bool empty)
    {
        var file = new ExportFileDto { Name = "dataset.zip", Size = 123, Created = DateTime.UnixEpoch, DownloadUrl = "https://example.org/dataset.zip" };
        var files = empty ? Array.Empty<ExportFileDto>() : new[] { file };
        var manager = new Mock<IBlobStorageManager>();
        manager.Setup(m => m.GetExportFilesAsync()).ReturnsAsync(files);
        var controller = WithContext(new ExportsController(
            Mock.Of<IObservationManager>(), manager.Object, Mock.Of<IExportManager>(), Mock.Of<IUserManager>(),
            Mock.Of<ICryptoService>(), Mock.Of<IUserExportRepository>(), Mock.Of<IInputValidator>(),
            new ObservationApiConfiguration(), NullLogger<ExportsController>.Instance));

        var result = await controller.GetDatasetsListAsync();
        if (empty)
        {
            Assert.Equal(StatusCodes.Status204NoContent, Assert.IsType<StatusCodeResult>(result).StatusCode);
        }
        else
        {
            var records = Assert.IsAssignableFrom<IEnumerable<ExportFileDto>>(Assert.IsType<OkObjectResult>(result).Value);
            AssertJsonEqual(file, Assert.Single(records));
        }
    }

    [Fact]
    public async Task ProjectsEndpoints_PreserveMappedContractJson()
    {
        var project = new ProjectInfo
        {
            Id = 42, Name = "Test", IsPublic = true, Category = "Monitoring",
            StartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Description = null!, EndDate = null
        };
        var manager = new Mock<IProjectManager>();
        manager.Setup(m => m.GetAsync("test", It.IsAny<int?>())).ReturnsAsync([project]);
        manager.Setup(m => m.GetAsync(It.IsAny<int[]>(), It.IsAny<int?>())).ReturnsAsync([project]);
        manager.Setup(m => m.GetAsync(42, It.IsAny<int?>())).ReturnsAsync(project);
        manager.Setup(m => m.GetAllAsync(false)).ReturnsAsync([project]);
        var controller = WithContext(new ProjectsController(manager.Object, NullLogger<ProjectsController>.Instance));
        var expected = new ProjectDto
        {
            Id = 42, Name = "Test", IsPublic = true, Category = "Monitoring",
            StartDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Description = null!, EndDate = null
        };

        foreach (var result in new[]
        {
            await controller.GetProjectsAsync("test"),
            await controller.GetProjectsByIdsAsync([42])
        })
        {
            var records = Assert.IsAssignableFrom<IEnumerable<ProjectDto>>(Assert.IsType<OkObjectResult>(result).Value);
            AssertJsonEqual(expected, Assert.Single(records));
        }
        AssertJsonEqual(expected, Assert.IsType<OkObjectResult>(await controller.GetProjectAsync(42)).Value);

        var vocabularies = WithContext(new VocabulariesController(
            Mock.Of<IVocabularyManager>(), manager.Object, NullLogger<VocabulariesController>.Instance));
        var projectRecords = Assert.IsAssignableFrom<IEnumerable<ProjectDto>>(
            Assert.IsType<OkObjectResult>(await vocabularies.GetProjects()).Value);
        AssertJsonEqual(expected, Assert.Single(projectRecords));
    }

    [Fact]
    public async Task VocabularyEndpoints_PreserveNestedCollectionsAndNulls()
    {
        var vocabulary = new Vocabulary
        {
            Id = (VocabularyId)1, Name = "Test vocabulary", Description = null!,
            Values = [new VocabularyValueInfo { Id = 7, Value = "Test", Translations = null!, Category = null! }],
            ExternalSystemsMapping = null!
        };
        var manager = new Mock<IVocabularyManager>();
        manager.Setup(m => m.GetVocabulariesAsync()).ReturnsAsync([vocabulary]);
        var controller = WithContext(new VocabulariesController(
            manager.Object, Mock.Of<IProjectManager>(), NullLogger<VocabulariesController>.Instance));
        var expected = new VocabularyDto
        {
            Id = 1, EnumId = (VocabularyIdDto)1, Name = "Test vocabulary", Description = null!,
            Values = [new VocabularyValueInfoDto { Id = 7, Value = "Test", Translations = null!, Category = null! }],
            ExternalSystemsMapping = null!
        };

        var records = Assert.IsAssignableFrom<IEnumerable<VocabularyDto>>(
            Assert.IsType<OkObjectResult>(await controller.GetVocabularies()).Value);
        AssertJsonEqual(expected, Assert.Single(records));
        AssertJsonEqual(expected, Assert.IsType<OkObjectResult>(
            await controller.GetVocabularyById((VocabularyIdDto)1)).Value);
    }

    [Fact]
    public async Task DatasetEndpoint_PreservesValuesAndNullCollections()
    {
        var dataset = new DsDatasetDto { Identifier = "dataset-1", Title = "Test", Creator = [], Projects = null };
        var manager = new Mock<IDataStewardshipManager>();
        manager.Setup(m => m.GetDatasetByIdAsync("dataset-1")).ReturnsAsync(dataset);
        var controller = WithContext(new DataStewardshipController(
            manager.Object, Mock.Of<IInputValidator>(), NullLogger<DataStewardshipController>.Instance));

        var result = Assert.IsType<OkObjectResult>(await controller.GetDatasetByIdAsync(null, null, "dataset-1"));
        AssertJsonEqual(dataset, result.Value);
    }

    private static T WithContext<T>(T controller) where T : ControllerBase
    {
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        return controller;
    }

    private static void AssertJsonEqual(object expected, object? actual)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        using var expectedJson = JsonDocument.Parse(JsonSerializer.Serialize(expected, options));
        using var actualJson = JsonDocument.Parse(JsonSerializer.Serialize(actual, options));
        Assert.True(JsonElement.DeepEquals(expectedJson.RootElement, actualJson.RootElement));
    }
}
