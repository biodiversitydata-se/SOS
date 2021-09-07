using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SOS.Lib.Enums;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.TestDataTools
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class CreateDataProviderMarkdownTool
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public CreateDataProviderMarkdownTool(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Create_data_providers_markdown()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            string outputPath = @"C:\temp\";
            var sosClient = new SosClient("https://sos-search.artdata.slu.se/");
            
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            //var response = await _fixture.DataProvidersController.GetDataProviders("en-GB");
            //var dataProviders = response.GetResult<List<DataProviderDto>>();
            List<DataProviderDto> dataProviderDtos = await sosClient.GetDataProviders("en-GB");
            var markDown = CreateMarkdown(dataProviderDtos);
            File.WriteAllText(Path.Join(outputPath, "dataproviders.md"), markDown);
        }

        private class SosClient
        {
            private readonly HttpClient _client;
            private readonly string _apiUrl;
            public SosClient(string apiUrl)
            {
                _client = new HttpClient();
                _apiUrl = apiUrl;
            }

            public async Task<List<DataProviderDto>> GetDataProviders(string cultureCode)
            {                
                var response = await _client.GetAsync($"{_apiUrl}DataProviders?cultureCode={cultureCode}");
                if (response.IsSuccessStatusCode)
                {
                    var resultString = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<List<DataProviderDto>>(resultString);
                    return result;
                }
                else
                {
                    throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
                }
            }
        }

        private string CreateMarkdown(List<DataProviderDto> dataProviders)
        {
            var sb = new StringBuilder();
            sb.AppendLine("| Id 	| Name 	| Organization 	| Number of observations 	|");
            sb.AppendLine("|:---	|:---	|:--- |---:	|");
            foreach (var dataProvider in dataProviders)
            {
                sb.AppendLine($"| {dataProvider.Id} | [{dataProvider.Name}]({dataProvider.Url}) | {dataProvider.Organization} | {(dataProvider.PublicObservations + dataProvider.ProtectedObservations):N0} |");
            }

            return sb.ToString();
        }
    }
}