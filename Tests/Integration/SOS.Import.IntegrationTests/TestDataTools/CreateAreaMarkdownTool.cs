using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SOS.Lib.Enums;
using SOS.Lib.Models.Search;
using Xunit;

namespace SOS.Import.IntegrationTests.TestDataTools
{
    public class CreateAreaMarkdownTool : TestBase
    {
        public class AreaData
        {
            public List<Lib.Models.Shared.Area> Areas { get; set; }
            public AreaType AreaType { get; set; }            
            public string Title { get; set; }
            public string Markdown { get; set; }
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

            public async Task<List<Lib.Models.Shared.Area>> GetAreas(int skip, int take, AreaType areaType)
            {                
                var response = await _client.GetAsync($"{_apiUrl}Areas?take={take}&skip={skip}&areaTypes={areaType.ToString()}");
                if (response.IsSuccessStatusCode)
                {
                    var resultString = response.Content.ReadAsStringAsync().Result;
                    var pagedResult = JsonConvert.DeserializeObject<PagedResult<Lib.Models.Shared.Area>>(resultString);
                    return pagedResult.Records.OrderBy(m => int.Parse(m.FeatureId)).ToList();                    
                }
                else
                {
                    throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
                }
            }

        }

        [Fact]
        public async Task CreateAreaMarkdowns()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------            
            string outputPath = @"C:\temp\sos-vocabs";
            var sosClient = new SosClient("https://sos-search.artdata.slu.se/");

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            List<AreaData> areaDatas = GetAreaData();
            foreach (var item in areaDatas)
            {
                item.Areas = await sosClient.GetAreas(0, 5000, item.AreaType);                
                item.Markdown = CreateMarkdown(item.Areas);
            }

            ////Create a markdown file for each area type
            //foreach (var areaData in areaDatas)
            //{
            //    string filePath = Path.Join(outputPath, $"{areaData.Title}.md");
            //    var sb = new StringBuilder();
            //    sb.AppendLine($"# {areaData.Title}");
            //    sb.Append(areaData.Markdown);
            //    File.WriteAllText(filePath, sb.ToString());
            //}

            // Add all areas into one file.
            var sbComposite = new StringBuilder();
            sbComposite.AppendLine("# Areas");
            foreach (var areaData in areaDatas)
            {
                sbComposite.AppendLine($"## {areaData.Title}");
                sbComposite.Append(areaData.Markdown);
                sbComposite.AppendLine();
            }
            File.WriteAllText(Path.Join(outputPath, "areas.md"), sbComposite.ToString());
        }

        private List<AreaData> GetAreaData()
        {
            List<AreaData> areaDatas = new List<AreaData>()
            {
                new AreaData
                {
                    AreaType = AreaType.County,
                    Title = "County"
                },
                new AreaData
                {
                    AreaType = AreaType.Municipality,
                    Title = "Municipality"
                },
                new AreaData
                {
                    AreaType = AreaType.Parish,
                    Title = "Parish"
                },
                new AreaData
                {
                    AreaType = AreaType.Province,
                    Title = "Province"
                }
            };

            return areaDatas;
        }
      
        private string CreateMarkdown(List<Lib.Models.Shared.Area> areas)
        {
            var sb = new StringBuilder();            
            sb.AppendLine("| FeatureId | Name |");
            sb.AppendLine("|:---	|:---	|");
            foreach (var area in areas)
            {
                sb.AppendLine($"| {area.FeatureId} | {area.Name} |");
            }            

            return sb.ToString();
        }
    }
}