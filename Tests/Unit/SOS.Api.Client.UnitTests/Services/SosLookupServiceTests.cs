using FluentAssertions;
using Moq;
using SOS.Api.Client.Abstractions;
using SOS.Api.Client.Requests;
using SOS.Api.Client.Services;
using SOS.Api.Contracts.Areas.Requests;
using SOS.Api.Contracts.Areas.Responses;
using SOS.Api.Contracts.Common.Geo;
using SOS.Api.Contracts.Common.Responses;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using System.Text;
using Xunit;

namespace SOS.Api.Client.UnitTests.Services;

public class SosLookupServiceTests
{
    [Fact]
    public async Task GetAreaWithGeometryAsync_GetsOneAreaAndPreservesBoundingBox()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var boundingBox = new LatLonBoundingBoxDto
        {
            TopLeft = new LatLonCoordinateDto { Latitude = 60, Longitude = 17 },
            BottomRight = new LatLonCoordinateDto { Latitude = 59, Longitude = 18 }
        };
        var areasClient = new Mock<ISosAreasClient>();
        areasClient.Setup(client => client.GetAreaAsync(
                AreaTypeDto.Province,
                "SE-1",
                cancellationToken))
            .ReturnsAsync(new AreaBaseDto
            {
                AreaType = AreaTypeDto.Province,
                FeatureId = "SE-1",
                Name = "Area one",
                BoundingBox = boundingBox
            });
        areasClient.Setup(client => client.GetAreaExportAsync(
                AreaTypeDto.Province,
                "SE-1",
                AreaExportFormatDto.Json,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateZip("""
            {
              "areaType": "Province",
              "featureId": "SE-1",
              "name": "Area one",
              "geometry": { "type": "Point", "coordinates": [18.0, 59.0] }
            }
            """));
        var service = new SosLookupService(
            areasClient.Object,
            new MemoryCache(new MemoryCacheOptions()),
            Options.Create(new SosClientOptions
            {
                ObservationsApiBaseAddress = new Uri("https://example.test/")
            }));

        var result = await service.GetAreaWithGeometryAsync(
            AreaTypeDto.Province,
            "SE-1",
            cancellationToken);

        result.Should().NotBeNull();
        result!.BoundingBox.Should().BeSameAs(boundingBox);
        result.Geometry.Should().NotBeNull();
    }

    [Fact]
    public async Task GetAreasWithGeometryAsync_UnpacksZipAndCachesGeometry()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var areasClient = new Mock<ISosAreasClient>();
        var area = new AreaBaseDto
        {
            AreaType = AreaTypeDto.Province,
            FeatureId = "SE-1",
            Name = "Area one"
        };
        areasClient.Setup(client => client.GetAreasAsync(It.IsAny<AreasOptions>(), cancellationToken))
            .ReturnsAsync(new PagedResultDto<AreaBaseDto>
            {
                Skip = 0,
                Take = 1,
                TotalCount = 1,
                Records = [area]
            });
        areasClient.Setup(client => client.GetAreaExportAsync(
                AreaTypeDto.Province,
                "SE-1",
                AreaExportFormatDto.Json,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateZip("""
            {
              "areaType": "Province",
              "featureId": "SE-1",
              "name": "Area one",
              "geometry": {
                "type": "Point",
                "coordinates": [18.0, 59.0]
              }
            }
            """));
        var service = new SosLookupService(
            areasClient.Object,
            new MemoryCache(new MemoryCacheOptions()),
            Options.Create(new SosClientOptions
            {
                ObservationsApiBaseAddress = new Uri("https://example.test/"),
                AreaGeometryCacheSlidingExpiration = TimeSpan.FromMinutes(15),
                AreaGeometryMaxConcurrency = 2
            }));

        var first = await service.GetAreasWithGeometryAsync(new AreasOptions(), cancellationToken);
        var second = await service.GetAreasWithGeometryAsync(new AreasOptions(), cancellationToken);

        first.Records.Should().ContainSingle();
        first.Records[0].Geometry.Should().NotBeNull();
        second.Records[0].Geometry.Should().NotBeNull();
        areasClient.Verify(client => client.GetAreaExportAsync(
            AreaTypeDto.Province,
            "SE-1",
            AreaExportFormatDto.Json,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAreasWithGeometryAsync_ConcurrentMissesShareOneExport()
    {
        var areasClient = new Mock<ISosAreasClient>();
        var area = new AreaBaseDto
        {
            AreaType = AreaTypeDto.Province,
            FeatureId = "SE-1",
            Name = "Area one"
        };
        areasClient.Setup(client => client.GetAreasAsync(
                It.IsAny<AreasOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResultDto<AreaBaseDto>
            {
                Records = [area],
                TotalCount = 1,
                Take = 1
            });
        var releaseExport = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        var exportStarted = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);
        areasClient.Setup(client => client.GetAreaExportAsync(
                AreaTypeDto.Province,
                "SE-1",
                AreaExportFormatDto.Json,
                It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                exportStarted.TrySetResult();
                await releaseExport.Task;
                return CreateZip("""
                {
                  "areaType": "Province",
                  "featureId": "SE-1",
                  "geometry": { "type": "Point", "coordinates": [18.0, 59.0] }
                }
                """);
            });
        var service = new SosLookupService(
            areasClient.Object,
            new MemoryCache(new MemoryCacheOptions()),
            Options.Create(new SosClientOptions
            {
                ObservationsApiBaseAddress = new Uri("https://example.test/")
            }));

        var first = service.GetAreasWithGeometryAsync(
            new AreasOptions(),
            TestContext.Current.CancellationToken);
        await exportStarted.Task.WaitAsync(TestContext.Current.CancellationToken);
        var second = service.GetAreasWithGeometryAsync(
            new AreasOptions(),
            TestContext.Current.CancellationToken);
        releaseExport.SetResult();

        await Task.WhenAll(first, second);
        areasClient.Verify(client => client.GetAreaExportAsync(
            AreaTypeDto.Province,
            "SE-1",
            AreaExportFormatDto.Json,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAreasWithGeometryAsync_CancelledLastWaiterDoesNotRetainInflightExport()
    {
        var firstArea = new AreaBaseDto
        {
            AreaType = AreaTypeDto.Province,
            FeatureId = "SE-1",
            Name = "Area one"
        };
        var secondArea = new AreaBaseDto
        {
            AreaType = AreaTypeDto.Province,
            FeatureId = "SE-2",
            Name = "Area two"
        };
        var areaRequests = 0;
        var firstAreaExports = 0;
        var firstExportStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirstExport = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondExportStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseSecondExport = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var areasClient = new Mock<ISosAreasClient>();
        areasClient.Setup(client => client.GetAreasAsync(
                It.IsAny<AreasOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var area = Interlocked.Increment(ref areaRequests) == 2 ? secondArea : firstArea;
                return new PagedResultDto<AreaBaseDto>
                {
                    Records = [area],
                    TotalCount = 1,
                    Take = 1
                };
            });
        areasClient.Setup(client => client.GetAreaExportAsync(
                AreaTypeDto.Province,
                "SE-1",
                AreaExportFormatDto.Json,
                It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                if (Interlocked.Increment(ref firstAreaExports) == 1)
                {
                    firstExportStarted.SetResult();
                    await releaseFirstExport.Task;
                }

                return CreateZip("""
                    {
                      "areaType": "Province",
                      "featureId": "SE-1",
                      "geometry": { "type": "Point", "coordinates": [18.0, 59.0] }
                    }
                    """);
            });
        areasClient.Setup(client => client.GetAreaExportAsync(
                AreaTypeDto.Province,
                "SE-2",
                AreaExportFormatDto.Json,
                It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                secondExportStarted.SetResult();
                await releaseSecondExport.Task;
                return CreateZip("""
                    {
                      "areaType": "Province",
                      "featureId": "SE-2",
                      "geometry": { "type": "Point", "coordinates": [19.0, 60.0] }
                    }
                    """);
            });
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new SosLookupService(
            areasClient.Object,
            memoryCache,
            Options.Create(new SosClientOptions
            {
                ObservationsApiBaseAddress = new Uri("https://example.test/"),
                AreaGeometryMaxConcurrency = 1
            }));
        using var cancellation = new CancellationTokenSource();

        var cancelledRequest = service.GetAreasWithGeometryAsync(new AreasOptions(), cancellation.Token);
        await firstExportStarted.Task.WaitAsync(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => cancelledRequest);

        var secondRequest = service.GetAreasWithGeometryAsync(
            new AreasOptions(),
            TestContext.Current.CancellationToken);
        releaseFirstExport.SetResult();
        await secondExportStarted.Task.WaitAsync(TestContext.Current.CancellationToken);

        memoryCache.Remove("Sos.Api.Client:Area:Province:SE-1");
        var retriedRequest = service.GetAreasWithGeometryAsync(
            new AreasOptions(),
            TestContext.Current.CancellationToken);
        releaseSecondExport.SetResult();

        await Task.WhenAll(secondRequest, retriedRequest);
        firstAreaExports.Should().Be(2);
    }

    private static byte[] CreateZip(string json)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry("area.json");
            using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
            writer.Write(json);
        }

        return stream.ToArray();
    }
}
