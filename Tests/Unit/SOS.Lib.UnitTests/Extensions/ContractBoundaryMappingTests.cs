using SOS.Api.Contracts.Status.Responses;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Models.Status;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using System.Text.Json;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions;

public class ContractBoundaryMappingTests
{
    static ContractBoundaryMappingTests()
    {
        // Apply the API's MongoDB conventions only to the read models used by these tests.
        ConventionRegistry.Register(
            "Status response read models",
            new ConventionPack { new IgnoreExtraElementsConvention(true), new IgnoreIfNullConvention(true) },
            type => type == typeof(ActiveInstanceInfoDto) || type == typeof(HarvestInfoDto)
                || type == typeof(MongoDbProcessInfoDto) || type == typeof(MongoDbProcessInfoDto.Provider));
    }

    [Fact]
    public void HangfireJob_PreservesDatabaseIdentifierJson()
    {
        var source = new HangfireJob
        {
            Id = MongoDB.Bson.ObjectId.GenerateNewId(),
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            StateName = "Succeeded",
            InvocationData = "{}"
        };
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var result = source.ToHangfireJobDto();
        var identifier = Assert.IsType<SOS.Api.Contracts.Status.Responses.ObjectId>(result.Id);
        Assert.Equal(source.Id.Timestamp, identifier.Timestamp);
        Assert.Equal(source.Id.CreationTime, identifier.CreationTime);
        using var expected = JsonDocument.Parse(JsonSerializer.Serialize(source, options));
        using var actual = JsonDocument.Parse(JsonSerializer.Serialize(result, options));

        Assert.True(JsonElement.DeepEquals(expected.RootElement, actual.RootElement));
        var deserialized = JsonSerializer.Deserialize<HangfireJobDto>(actual.RootElement.GetRawText(), options)!;
        Assert.Equal(identifier.Timestamp, deserialized.Id.Timestamp);
        Assert.Equal(identifier.CreationTime, deserialized.Id.CreationTime);
    }

    [Fact]
    public void ProcessInfo_DeserializesMongoDocumentAndPreservesNestedProviderJson()
    {
        var start = new DateTime(2026, 9, 21, 12, 0, 0, DateTimeKind.Utc);
        var end = start.AddMinutes(30);
        var bson = new BsonDocument
        {
            { "_id", "observation-1" },
            { "ProcessFailCount", 1 },
            { "PublicCount", 3 },
            { "ProtectedCount", 2 },
            { "Start", new BsonDateTime(start) },
            { "End", new BsonDateTime(end) },
            { "Status", "Succeeded" },
            { "UnusedMongoField", true },
            { "ProvidersInfo", new BsonArray
                {
                    new BsonDocument
                    {
                        { "DataProviderId", 7 },
                        { "DataProviderIdentifier", "provider" },
                        { "ProcessStart", new BsonDateTime(start) },
                        { "ProcessEnd", new BsonDateTime(end) },
                        { "ProcessStatus", "Succeeded" },
                        { "ProcessFailCount", 1 },
                        { "PublicProcessCount", 3 },
                        { "ProtectedProcessCount", 2 },
                        { "HarvestStart", new BsonDateTime(start) },
                        { "HarvestEnd", new BsonDateTime(end) },
                        { "HarvestNotes", "notes" },
                        { "HarvestStatus", "Succeeded" },
                        { "HarvestCount", 6 },
                        { "LatestIncrementalStart", new BsonDateTime(start) },
                        { "LatestIncrementalEnd", new BsonDateTime(end) },
                        { "LatestIncrementalStatus", 0 },
                        { "LatestIncrementalPublicCount", 4 },
                        { "LatestIncrementalProtectedCount", 5 },
                        { "DataProviderType", "Artportalen" },
                        { "UnusedProviderField", true }
                    }
                }
            }
        };
        var expected = new MongoDbProcessInfoDto
        {
            Id = "observation-1",
            ProcessFailCount = 1,
            Status = "Succeeded",
            PublicCount = 3,
            ProtectedCount = 2,
            Start = start,
            End = end,
            ProvidersInfo = [new MongoDbProcessInfoDto.Provider
            {
                DataProviderId = 7,
                DataProviderIdentifier = "provider",
                ProcessStart = start,
                ProcessEnd = end,
                ProcessStatus = "Succeeded",
                ProcessFailCount = 1,
                PublicProcessCount = 3,
                ProtectedProcessCount = 2,
                HarvestStart = start,
                HarvestEnd = end,
                HarvestNotes = "notes",
                HarvestStatus = "Succeeded",
                HarvestCount = 6,
                LatestIncrementalStart = start,
                LatestIncrementalEnd = end,
                LatestIncrementalStatus = 0,
                LatestIncrementalPublicCount = 4,
                LatestIncrementalProtectedCount = 5,
                DataProviderType = "Artportalen"
            }]
        };

        var result = BsonSerializer.Deserialize<MongoDbProcessInfoDto>(bson);

        AssertSameJson(expected, result);
    }

    [Fact]
    public void ActiveInstance_DeserializesMongoIdentifierAndPreservesJson()
    {
        var bson = new BsonDocument
        {
            { "_id", "Observation" },
            { "ActiveInstance", 1 },
            { "UnusedMongoField", true }
        };

        var result = BsonSerializer.Deserialize<ActiveInstanceInfoDto>(bson);

        AssertSameJson(new ActiveInstanceInfoDto { Id = "Observation", ActiveInstance = 1 }, result);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HarvestInfo_DeserializesMongoDocumentAndPreservesDateJson(bool hasLastModified)
    {
        var start = new DateTime(2026, 9, 21, 12, 0, 0, DateTimeKind.Utc);
        var end = start.AddMinutes(30);
        var bson = new BsonDocument
        {
            { "_id", "provider:1" },
            { "Count", 42 },
            { "Start", new BsonDateTime(start) },
            { "End", new BsonDateTime(end) },
            { "Status", "Succeeded" },
            { "Notes", "notes" },
            { "UnusedMongoField", true }
        };
        if (hasLastModified)
        {
            bson.Add("DataLastModified", new BsonDateTime(start));
        }

        var result = BsonSerializer.Deserialize<HarvestInfoDto>(bson);

        AssertSameJson(new HarvestInfoDto
        {
            Id = "provider:1",
            Count = 42,
            Start = start,
            End = end,
            Status = "Succeeded",
            Notes = "notes",
            DataLastModified = hasLastModified ? start : null
        }, result);
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("null")]
    [InlineData("empty")]
    public void ProcessInfo_PreservesNullAndEmptyProviders(string collectionState)
    {
        var bson = new BsonDocument { { "_id", "observation-1" } };
        if (collectionState != "missing")
        {
            bson.Add("ProvidersInfo", collectionState == "null" ? BsonNull.Value : new BsonArray());
        }

        var result = BsonSerializer.Deserialize<MongoDbProcessInfoDto>(bson);

        AssertSameJson(new MongoDbProcessInfoDto
        {
            Id = "observation-1",
            ProvidersInfo = collectionState == "empty" ? [] : null
        }, result);
    }

    private static void AssertSameJson<T>(T expected, T actual)
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        Assert.Equal(JsonSerializer.Serialize(expected, options), JsonSerializer.Serialize(actual, options));
    }
}
