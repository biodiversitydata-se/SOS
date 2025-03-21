using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SOS.Lib.JsonConverters;
using SOS.Observations.Api.Middleware;
using SOS.Shared.Api.Dtos.Filter;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Observations.Api.UnitTests.Middleware;
public class StoreRequestBodyMiddlewareTests
{
    private readonly static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
        Converters = {
                new JsonStringEnumConverter(),
                new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()
            }
    };

    [Fact]
    public async Task Invoke_Should_Store_RequestBody_When_Method_Is_Post_Or_Put()
    {
        // Arrange
        var requestBody = "{\"key\":\"value\"}";
        var requestStream = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Body = requestStream;
        context.Request.Body.Seek(0, SeekOrigin.Begin);

        var nextMock = new Mock<RequestDelegate>();
        var middleware = new StoreRequestBodyMiddleware(nextMock.Object);

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Items.ContainsKey("Request-body").Should().BeTrue();
        requestBody.Should().Be((string)context.Items["Request-body"]);
    }

    [Fact]
    public async Task Invoke_Should_Store_Reduced_RequestBody_When_RequestBody_Is_Too_Large()
    {
        // Arrange
        var searchFilter = new SearchFilterDto
        {
            Taxon = new TaxonFilterDto
            {
                Ids = Enumerable.Range(1, 10000).ToList()
            }
        };

        var requestBody = JsonSerializer.Serialize(searchFilter);
        var requestStream = new MemoryStream(Encoding.UTF8.GetBytes(requestBody));
        var context = new DefaultHttpContext();
        context.Request.Method = "POST";
        context.Request.Body = requestStream;
        context.Request.Body.Seek(0, SeekOrigin.Begin);

        var nextMock = new Mock<RequestDelegate>();
        var middleware = new StoreRequestBodyMiddleware(nextMock.Object);

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Items.ContainsKey("Request-body").Should().BeTrue();
        requestBody.Should().NotBe((string)context.Items["Request-body"]);
        var newRequestBody = JsonSerializer.Deserialize<SearchFilterInternalDto>((string)context.Items["Request-body"], jsonSerializerOptions);
        newRequestBody.Taxon.Ids.Should().ContainSingle().Which.Should().Be(-1);
    }

    [Fact]
    public async Task Invoke_Should_Not_Store_RequestBody_When_Method_Is_Get()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";

        var nextMock = new Mock<RequestDelegate>();
        var middleware = new StoreRequestBodyMiddleware(nextMock.Object);

        // Act
        await middleware.Invoke(context);

        // Assert
        context.Items.ContainsKey("Request-body").Should().BeFalse();
    }
}