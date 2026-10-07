using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Services;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Services.Interfaces;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SOS.Import.UnitTests.Services;

public class SharkObservationServiceTests
{
    [Fact]
    public async Task GetAsync_PreservesCaseInsensitiveFieldsUtf7CharactersAndNumericStrings()
    {
        var service = CreateService("""
            {"HEADER":["Art","Antal"],"rOwS":[["Räka",3],["Krabba",null]],"ID":"SHARK-1"}
            """);

        var result = await service.GetAsync("test");

        Assert.NotNull(result);
        Assert.Equal("SHARK-1", result.Id);
        Assert.Equal(new[] { "Art", "Antal" }, result.Header);
        var rows = result.Rows.ToArray();
        Assert.Equal(2, rows.Length);
        Assert.Equal(new[] { "Räka", "3" }, rows[0]);
        Assert.Equal(new string?[] { "Krabba", null }, rows[1]);
    }

    [Fact]
    public async Task GetDataSetsAsync_PreservesNullAndEmptyCollections()
    {
        var service = CreateService("""
            {"header":null,"rows":[]}
            """);

        var result = await service.GetDataSetsAsync();

        Assert.NotNull(result);
        Assert.Null(result.Header);
        Assert.Empty(result.Rows);
    }

    [Fact]
    public async Task GetAsync_RejectsInvalidJson()
    {
        var service = CreateService("{invalid}");

        await Assert.ThrowsAsync<JsonException>(() => service.GetAsync("test"));
    }

    private static SharkObservationService CreateService(string json)
    {
        var httpClient = new Mock<IHttpClientService>();
#pragma warning disable SYSLIB0001 // Exercise the existing SHARK file encoding without changing it.
        httpClient.Setup(client => client.GetFileStreamAsync(It.IsAny<Uri>(), null))
            .ReturnsAsync(() => new MemoryStream(Encoding.UTF7.GetBytes(json)));
#pragma warning restore SYSLIB0001
        return new SharkObservationService(httpClient.Object,
            new SharkServiceConfiguration { BaseAddress = "https://shark.example" },
            NullLogger<SharkObservationService>.Instance);
    }
}
