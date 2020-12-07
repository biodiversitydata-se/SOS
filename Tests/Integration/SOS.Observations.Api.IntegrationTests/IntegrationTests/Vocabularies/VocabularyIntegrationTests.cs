using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using SOS.Observations.Api.Dtos.Vocabulary;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.Vocabularies
{
    //public class VocabularyIntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
    //{
    //    private readonly HttpClient _httpClient;

    //    public VocabularyIntegrationTests(WebApplicationFactory<Startup> factory)
    //    {
    //        System.Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "local");
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
