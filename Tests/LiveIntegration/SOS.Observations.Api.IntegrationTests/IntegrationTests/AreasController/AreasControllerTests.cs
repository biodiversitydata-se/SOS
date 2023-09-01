using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using FluentAssertions;
using NLog.Targets;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Search.Result;
using SOS.Observations.Api.Dtos;
using SOS.Observations.Api.Dtos.Enum;
using SOS.Observations.Api.Dtos.Filter;
using SOS.Observations.Api.Dtos.Location;
using SOS.Observations.Api.IntegrationTests.Extensions;
using SOS.Observations.Api.IntegrationTests.Fixtures;
using Xunit;

namespace SOS.Observations.Api.IntegrationTests.IntegrationTests.LocationsController
{
    [Collection(Collections.ApiIntegrationTestsCollection)]
    public class AreasControllerTests
    {
        private readonly ApiIntegrationTestFixture _fixture;

        public AreasControllerTests(ApiIntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        [Trait("Category", "ApiIntegrationTest")]
        public async Task Create_Municipalities_Json_file()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var areaResponse = await _fixture.AreasController.GetAreas(new List<AreaTypeDto> { AreaTypeDto.Municipality }, null, 0, 500);
            var result = areaResponse.GetResult<PagedResult<AreaBaseDto>>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            var municipalities = new List<FeatureIdNameTuple>();
            foreach (var record in result.Records)
            {
                municipalities.Add(new FeatureIdNameTuple()
                {
                    FeatureId = record.FeatureId,
                    Name = record.Name
                });
            }
            municipalities = municipalities.OrderBy(m => int.Parse(m.FeatureId)).ToList();
            municipalities = municipalities.OrderBy(m => m.Name).ToList();

            var jsonSerializerOptions = new JsonSerializerOptions()
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Latin1Supplement) // Display å,ä,ö e.t.c. properly
            };

            string strJson = JsonSerializer.Serialize(municipalities, jsonSerializerOptions);
            var filename = FilenameHelper.CreateFilenameWithDate("municipalities", "json");
            var filePath = System.IO.Path.Combine(@"C:\temp\", filename);            
            await System.IO.File.WriteAllTextAsync(filePath, strJson);            

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            municipalities.Count.Should().Be(290);
        }

        private class FeatureIdNameTuple
        {
            public string FeatureId { get; set; }
            public string Name { get; set; }
        }
    }
}
