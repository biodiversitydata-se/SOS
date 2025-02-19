using Microsoft.AspNetCore.Http;
using SOS.DataStewardship.Api.Contracts.Enums;
using SOS.DataStewardship.Api.Contracts.Models;

namespace SOS.DataStewardship.Api.IntegrationTests.Tests.Datasets.Search;

[Collection(Constants.IntegrationTestsCollectionName)]
public class DatasetsBadRequestsTests : TestBase
{
    public DatasetsBadRequestsTests(TestFixture testFixture, ITestOutputHelper output) : base(testFixture, output)
    {
    }

    [Fact]
    public async Task DatasetsBySearch_ReturnsBadRequest_GivenEndDateIsBeforeStartDateInDateFilter()
    {
        // Arrange        
        var searchFilter = new DatasetFilter
        {
            DateFilter = new DateFilter
            {
                StartDate = DateTime.Now,                
                EndDate = DateTime.Now - TimeSpan.FromDays(1),
                DateFilterType = DateFilterType.OverlappingStartDateAndEndDate
            }
        };
        
        // Act
        var response = await ApiClient.PostAsJsonAsync<DatasetFilter>(
            $"datasets?skip=0&take=0", searchFilter, jsonSerializerOptions);
        var errors = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>(jsonSerializerOptions);        

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        errors.Status.Should().Be(400);
        errors.Title.Should().NotBeEmpty();
    }
}