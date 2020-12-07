using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SOS.Lib.Models.Processed.Observation;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Vocabulary;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.Taxon
{
    public class VocabularyIntegrationTests : IClassFixture<ObservationApiIntegrationTestFixture>
    {
        private readonly ObservationApiIntegrationTestFixture _fixture;

        public VocabularyIntegrationTests(ObservationApiIntegrationTestFixture apiTestFixture)
        {
            _fixture = apiTestFixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Get_LifeStage_Vocabulary()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var response = await _fixture.VocabulariesController.GetVocabularyAsync(VocabularyIdDto.LifeStage);
            var vocabulary = response.GetResult<VocabularyDto>();

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            vocabulary.EnumId.Should().Be(VocabularyIdDto.LifeStage);
        }
    }

    //--------------------------------------------------------------------------------------------------
    // This is how you can write integration tests, which also tests IoC setup and model bindings.
    // Unfortunately you can't set user secrets per environment, so I guess this wouldn't be so useful.
    //--------------------------------------------------------------------------------------------------
    //public class VocabularyIntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
    //{
    //    private readonly HttpClient _httpClient;

    //    public VocabularyIntegrationTests(WebApplicationFactory<Startup> factory)
    //    {
    //        System.Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "st");
    //        _httpClient = factory.CreateDefaultClient();
    //    }

    //    public async Task TestLifeStageVocabulary()
    //    {
    //        //-----------------------------------------------------------------------------------------------------------
    //        // Act
    //        //-----------------------------------------------------------------------------------------------------------
    //        var response = await _httpClient.GetAsync("/Vocabularies/LifeStage");
    //        var resultString = response.Content.ReadAsStringAsync().Result;
    //        var vocabulary = JsonConvert.DeserializeObject<VocabularyDto>(resultString);

    //        //-----------------------------------------------------------------------------------------------------------
    //        // Assert
    //        //-----------------------------------------------------------------------------------------------------------
    //        vocabulary.EnumId.Should().Be(VocabularyIdDto.LifeStage);
    //    }
    //}
}