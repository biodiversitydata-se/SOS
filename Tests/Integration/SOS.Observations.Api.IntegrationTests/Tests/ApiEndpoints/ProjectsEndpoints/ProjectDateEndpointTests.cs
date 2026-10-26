using FluentAssertions.Execution;
using Moq;
using SOS.Api.Contracts.Lookups.Responses;
using SOS.Lib.Cache.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.IntegrationTests.Setup;

namespace SOS.Observations.Api.IntegrationTests.Tests.ApiEndpoints.ProjectsEndpoints;

[Collection(TestCollection.Name)]
[Trait("Category", "ReadOnlyIntegrationTest")]
public class ProjectDateEndpointTests(TestFixture fixture)
{
    [Fact]
    public async Task ProjectEndpoints_ReturnUtcDates_WhenStoredDatesHaveUnspecifiedKind()
    {
        var date = new DateTime(2026, 9, 30, 12, 34, 0, DateTimeKind.Unspecified);
        var project = new ProjectInfo { Id = 12, Name = "test project", StartDate = DateTime.MinValue, EndDate = date };
        var cache = new Mock<ICache<int, ProjectInfo>>();
        cache.Setup(value => value.GetAllAsync()).ReturnsAsync([project]);
        using var client = fixture.CreateApiClientWithReplacedService(cache.Object);
        var endpoints = new[]
        {
            (Method: "GET", Url: "/Projects/12"),
            (Method: "GET", Url: "/Projects?filter=test"),
            (Method: "POST", Url: "/Projects"),
            (Method: "GET", Url: "/Vocabularies/Projects")
        };

        foreach (var (method, url) in endpoints)
        {
            using var request = new HttpRequestMessage(new HttpMethod(method), url);
            if (method == "POST") request.Content = JsonContent.Create(new[] { project.Id });

            using var response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            response.StatusCode.Should().Be(HttpStatusCode.OK, $"{method} {url}");
            using var scope = new AssertionScope($"{method} {url}");
            var dto = url == "/Projects/12"
                ? await response.Content.ReadFromJsonAsync<ProjectDto>(cancellationToken: TestContext.Current.CancellationToken)
                : (await response.Content.ReadFromJsonAsync<List<ProjectDto>>(
                    cancellationToken: TestContext.Current.CancellationToken))!.Should().ContainSingle().Subject;
            dto.Should().NotBeNull();
            dto!.StartDate.Should().Be(DateTimeOffset.MinValue);
            dto.EndDate.Should().Be(new DateTimeOffset(DateTime.SpecifyKind(date, DateTimeKind.Utc)));
            dto.StartDate!.Value.Offset.Should().Be(TimeSpan.Zero);
            dto.EndDate!.Value.Offset.Should().Be(TimeSpan.Zero);
        }
    }
}
