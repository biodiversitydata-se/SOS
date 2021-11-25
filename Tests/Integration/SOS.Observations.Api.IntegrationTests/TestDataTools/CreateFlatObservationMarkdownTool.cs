using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SOS.Lib.Enums;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Vocabulary;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.TestDataTools
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class CreateFlatObservationMarkdownTool
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public CreateFlatObservationMarkdownTool(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Create_flat_observation_markdown()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            string outputPath = @"C:\temp\";
            var sosClient = new SosClient("https://sos-search.artdata.slu.se/");

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------                        
            var observationProperties = await sosClient.GetObservationProperties();
            var markDown = CreateMarkdown(observationProperties);
            File.WriteAllText(Path.Join(outputPath, "FlatObservation.md"), markDown);
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

            public async Task<List<PropertyFieldDescriptionDto>> GetObservationProperties()
            {                
                var response = await _client.GetAsync($"{_apiUrl}Vocabularies/ObservationProperties");
                if (response.IsSuccessStatusCode)
                {
                    var resultString = response.Content.ReadAsStringAsync().Result;
                    var result = JsonConvert.DeserializeObject<List<PropertyFieldDescriptionDto>>(resultString);
                    return result;
                }
                else
                {
                    throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
                }
            }
        }

        private string CreateMarkdown(List<PropertyFieldDescriptionDto> observationProperties)
        {
            var sb = new StringBuilder();
            sb.AppendLine("## Minimum");
            sb.AppendLine("| Name | PropertyPath | Swedish | English | Data type | Field set |");
            sb.AppendLine("|:---	|:---	|:--- |---:	|");
            foreach (var property in observationProperties.Where(m => m.FieldSet == OutputFieldSet.Minimum))
            {
                sb.AppendLine($"| {property.Name} | {property.PropertyPath} | {property.SwedishTitle} | {property.EnglishTitle} | {property.DataType} | {property.FieldSet} |");
            }

            sb.AppendLine("## Extended");
            sb.AppendLine("| Name | PropertyPath | Swedish | English | Data type | Field set |");
            sb.AppendLine("|:---	|:---	|:--- |---:	|");
            foreach (var property in observationProperties.Where(m => m.FieldSet == OutputFieldSet.Extended))
            {
                sb.AppendLine($"| {property.Name} | {property.PropertyPath} | {property.SwedishTitle} | {property.EnglishTitle} | {property.DataType} | {property.FieldSet} |");
            }

            sb.AppendLine("## AllWithValues");
            sb.AppendLine("| Name | PropertyPath | Swedish | English | Data type | Field set |");
            sb.AppendLine("|:---	|:---	|:--- |---:	|");
            foreach (var property in observationProperties.Where(m => m.FieldSet == OutputFieldSet.AllWithValues))
            {
                sb.AppendLine($"| {property.Name} | {property.PropertyPath} | {property.SwedishTitle} | {property.EnglishTitle} | {property.DataType} | {property.FieldSet} |");
            }

            sb.AppendLine("## All");
            sb.AppendLine("| Name | PropertyPath | Swedish | English | Data type | Field set |");
            sb.AppendLine("|:---	|:---	|:--- |---:	|");
            foreach (var property in observationProperties.Where(m => m.FieldSet == OutputFieldSet.All))
            {
                sb.AppendLine($"| {property.Name} | {property.PropertyPath} | {property.SwedishTitle} | {property.EnglishTitle} | {property.DataType} | {property.FieldSet} |");
            }

            return sb.ToString();
        }
    }
}