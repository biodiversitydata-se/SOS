using SOS.Api.Contracts.Observations;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Observations.Responses;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport.Extensions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Helpers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace SOS.Api.Contracts.UnitTests;

public class DiffusionStatusContractTests
{
    private static readonly ElasticsearchClient ElasticClient =
        new(ElasticSearchHelper.GetDefaultSettings(new Uri("http://localhost:9200")));

    [Theory]
    [InlineData(DiffusionStatus.NotDiffused, 0)]
    [InlineData(DiffusionStatus.DiffusedBySystem, 1)]
    [InlineData(DiffusionStatus.DiffusedByProvider, 2)]
    public void DiffusionStatus_PreservesNumericStorageAndObservationStringJson(DiffusionStatus value, int number)
    {
        Assert.Equal(number, (int)value);

        var observation = new SOS.Lib.Models.Processed.Observation.Observation { DiffusionStatus = value };
        var apiOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        apiOptions.Converters.Add(new JsonStringEnumConverter());
        using var serverJson = JsonDocument.Parse(JsonSerializer.Serialize(observation, apiOptions));
        Assert.Equal(value.ToString(), serverJson.RootElement.GetProperty("diffusionStatus").GetString());
        Assert.Equal(value, JsonSerializer.Deserialize<SOS.Lib.Models.Processed.Observation.Observation>(
            serverJson.RootElement.GetRawText(), apiOptions)!.DiffusionStatus);

        using var elasticJson = JsonDocument.Parse(ElasticClient.SourceSerializer.SerializeToString(observation));
        Assert.Equal(number, elasticJson.RootElement.GetProperty("diffusionStatus").GetInt32());

        var bson = observation.ToBsonDocument();
        Assert.Equal(number, bson["DiffusionStatus"].AsInt32);
        Assert.Equal(value, BsonSerializer.Deserialize<SOS.Lib.Models.Processed.Observation.Observation>(bson).DiffusionStatus);

        using var newtonsoftJson = JsonDocument.Parse(Newtonsoft.Json.JsonConvert.SerializeObject(observation));
        Assert.Equal(number, newtonsoftJson.RootElement.GetProperty("DiffusionStatus").GetInt32());

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var json = JsonSerializer.Serialize(new Observation { DiffusionStatus = value }, options);
        using var document = JsonDocument.Parse(json);
        Assert.Equal(value.ToString(), document.RootElement.GetProperty("diffusionStatus").GetString());
        Assert.Equal(value, JsonSerializer.Deserialize<Observation>(json, options)!.DiffusionStatus);
        Assert.Equal(value, JsonSerializer.Deserialize<Observation>($"{{\"diffusionStatus\":{number}}}", options)!.DiffusionStatus);
    }

    [Theory]
    [InlineData(DiffusionStatus.NotDiffused)]
    [InlineData(DiffusionStatus.DiffusedBySystem)]
    [InlineData(DiffusionStatus.DiffusedByProvider)]
    public void SearchFilter_UsesSharedEnumAndPreservesNullAndEmptyCollections(DiffusionStatus value)
    {
        var request = new SearchFilterInternalDto { DiffusionStatuses = [value] };
        var filter = request.ToSearchFilterInternal(0, "sv-SE");

        Assert.Equal(value, Assert.Single(filter.DiffusionStatuses));
        Assert.Null(new SearchFilterInternalDto().ToSearchFilterInternal(0, "sv-SE").DiffusionStatuses);
        Assert.Empty(new SearchFilterInternalDto { DiffusionStatuses = [] }
            .ToSearchFilterInternal(0, "sv-SE").DiffusionStatuses);
    }
}
