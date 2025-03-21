using SOS.DataStewardship.Api.Contracts.Models;
using SOS.Lib.JsonConverters;
using System.Text;

namespace SOS.DataStewardship.Api.IntegrationTests.Tests.Tools;
public class CreateDatasetsMarkdownTool
{
    [Fact(Skip = "Intended to run on demand")]
    [Trait("Category", "DataUtil")]
    public async Task Create_data_providers_markdown()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------
        string outputPath = @"C:\temp\";
        var sosClient = new DataStewardshipClient("https://sos-datastewardship.artdata.slu.se/");
        var datasetInfos = new List<DatasetInfo>();

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------
        var datasetPaging = await sosClient.GetDatasets();
        foreach (var dataset in datasetPaging.Records) 
        {
            var eventsPaging = await sosClient.GetEventsByDatasetId(dataset.Identifier, 0);
            var occurrencesPaging = await sosClient.GetOccurrencesByDatasetId(dataset.Identifier, 0);

            datasetInfos.Add(new DatasetInfo
            {
                Identifier = dataset.Identifier,
                Title = dataset.Title,
                Description = dataset.Description,
                Creator = string.Join(", ", dataset.Creator.Select(m => m.OrganisationCode)),
                Owner = dataset.OwnerinstitutionCode.OrganisationCode,
                EventCount = eventsPaging.TotalCount,
                OccurrenceCount = occurrencesPaging.TotalCount
            });
        }
        
        var markDown = CreateMarkdown(datasetInfos);
        File.WriteAllText(Path.Join(outputPath, "datasets.md"), markDown);
    }

    private class DatasetInfo
    {
        public string Identifier { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Creator { get; set; }
        public string Owner { get; set; }
        public int EventCount { get; set; }
        public int OccurrenceCount { get; set; }
    }

    private class DataStewardshipClient
    {
        private readonly HttpClient _client;
        private readonly string _apiUrl;
        public DataStewardshipClient(string apiUrl)
        {
            _client = new HttpClient();
            _apiUrl = apiUrl;
        }

        public async Task<PagedResult<Contracts.Models.Dataset>> GetDatasets()
        {
            DatasetFilter datasetFilter = new DatasetFilter();
            var response = await _client.PostAsync($"{_apiUrl}datasets?take=100", JsonContent.Create(datasetFilter));
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PagedResult<Contracts.Models.Dataset>>(JsonSerializerOptions);
                return result;
            }
            else
            {
                throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
            }
        }

        public async Task<PagedResult<Contracts.Models.Event>> GetEventsByDatasetId(string datasetIdentifier, int take = 0)
        {
            var filter = new EventsFilter()
            {
                DatasetIds = new List<string> { datasetIdentifier }
            };

            var response = await _client.PostAsync($"{_apiUrl}events?take={take}", JsonContent.Create(filter));
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PagedResult<Contracts.Models.Event>>(JsonSerializerOptions);
                return result;
            }
            else
            {
                throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
            }
        }

        public async Task<PagedResult<Contracts.Models.Occurrence>> GetOccurrencesByDatasetId(string datasetIdentifier, int take = 0)
        {
            var filter = new OccurrenceFilter()
            {
                DatasetIds = new List<string> { datasetIdentifier }
            };

            var response = await _client.PostAsync($"{_apiUrl}occurrences?take={take}", JsonContent.Create(filter));
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PagedResult<Contracts.Models.Occurrence>>(JsonSerializerOptions);
                return result;
            }
            else
            {
                throw new Exception("Call to API failed, responseCode:" + response.StatusCode);
            }
        }


        protected readonly JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            Converters =
        {
            new JsonStringEnumConverter(),
            new NetTopologySuite.IO.Converters.GeoJsonConverterFactory()
        }
        };
    }


    private string CreateMarkdown(List<DatasetInfo> datasetInfos)
    {
        var sb = new StringBuilder();
        sb.AppendLine("| Identifier 	| Title 	| Creator | #Events | #Occurrences |");
        sb.AppendLine("|:---	|:---	|:--- |:---  |---:	|");
        foreach (var dataset in datasetInfos)
        {
            sb.AppendLine($"| {dataset.Identifier} | {dataset.Title} | {dataset.Creator} | {dataset.EventCount:N0} | {dataset.OccurrenceCount:N0} |");
            //totalCount += dataProvider.PublicObservations + dataProvider.ProtectedObservations;
        }
        //sb.AppendLine($"|  |  |  | **{totalCount:N0}** |");

        return sb.ToString();
    }
}
