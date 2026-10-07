using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Observations.Api.Controllers;
using SOS.Observations.Api.Managers.Interfaces;
using SOS.Observations.Api.Repositories.Interfaces;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SOS.Observations.Api.UnitTests.Controllers;

public class DataProvidersControllerTests
{
    [Theory]
    [InlineData("application/json", "application/json")]
    [InlineData("APPLICATION/JSON", "application/json")]
    [InlineData("Application/Json", "application/json")]
    [InlineData("application/xml", "application/xml")]
    [InlineData("APPLICATION/XML", "application/xml")]
    public async Task GetEML_AcceptHeader_IsCaseInsensitive(string accept, string contentType)
    {
        const string eml = "<eml><title>Test data</title></eml>";
        var manager = new Mock<IDataProviderManager>();
        manager.Setup(value => value.GetEmlFileAsync(99)).ReturnsAsync(Encoding.UTF8.GetBytes(eml));
        var controller = new DataProvidersController(
            manager.Object, Mock.Of<IObservationManager>(), Mock.Of<IProcessInfoManager>(),
            Mock.Of<IProcessedObservationRepository>(), NullLogger<DataProvidersController>.Instance)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        controller.Request.Headers.Accept = accept;

        var result = Assert.IsType<ContentResult>(await controller.GetEMLAsync(99));

        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        Assert.Equal(contentType, result.ContentType);
        if (contentType == "application/json")
        {
            using var json = JsonDocument.Parse(result.Content!);
            Assert.Equal("Test data", json.RootElement.GetProperty("eml").GetProperty("title").GetString());
        }
        else
        {
            Assert.Equal(eml, result.Content);
        }
    }
}
