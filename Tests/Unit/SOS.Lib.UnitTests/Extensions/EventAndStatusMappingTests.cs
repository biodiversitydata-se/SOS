using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using SOS.Lib.Extensions.Dto;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Status;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Xunit;

namespace SOS.Lib.UnitTests.Extensions;

public class EventAndStatusMappingTests
{
    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static void AssertSameJson(object source, object target)
    {
        var options = CreateOptions();
        var expected = JsonSerializer.SerializeToNode(source, source.GetType(), options);
        var actual = JsonSerializer.SerializeToNode(target, target.GetType(), options);
        JsonNode.DeepEquals(expected, actual).Should().BeTrue($"expected {expected} but got {actual}");
    }

    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void ChecklistEvent_PreservesDatesAndNestedProperties(DateTimeKind kind)
    {
        var source = new Event(new DateTime(2026, 9, 21, 12, 30, 0, kind))
        {
            EventId = "event:1",
            Weather = new Weather { AirTemperature = new Measuring { Value = 12.5 } }
        };
        var mapped = source.ToEventDto();
        AssertSameJson(source, mapped!);
    }

    [Theory]
    [InlineData(DateTimeKind.Utc)]
    [InlineData(DateTimeKind.Local)]
    [InlineData(DateTimeKind.Unspecified)]
    public void Multimedia_PreservesCommentDates(DateTimeKind kind)
    {
        var source = new Multimedia
        {
            Comments = [new MultimediaComment { Comment = "test", Created = new DateTime(2026, 9, 21, 12, 30, 0, kind) }]
        };
        var mapped = source.ToMultimediaDto();
        AssertSameJson(source, mapped!);
    }

    [Fact]
    public void ChecklistEvent_PreservesPopulatedNestedData()
    {
        var source = new Event(
            new DateTime(2026, 9, 21, 12, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 9, 23, 14, 45, 0, DateTimeKind.Utc))
        {
            EventId = "event:1",
            ParentEventId = "parent:1",
            DiscoveryMethod = new VocabularyValue { Id = 42, Value = "discovery method" },
            EventRemarks = "event remarks",
            FieldNotes = "field notes",
            FieldNumber = "field number",
            Habitat = "habitat",
            SamplingEffort = "sampling effort",
            SamplingProtocol = "sampling protocol",
            SampleSizeUnit = "square metre",
            SampleSizeValue = "25",
            Media =
            [
                new Multimedia
                {
                    Type = "image",
                    Format = "image/jpeg",
                    Identifier = "media:1",
                    References = "media references",
                    Title = "media title",
                    Description = "media description",
                    Created = "2026-09-21",
                    Creator = "creator",
                    Contributor = "contributor",
                    Publisher = "publisher",
                    Audience = "audience",
                    Source = "source",
                    License = "license",
                    RightsHolder = "rights holder",
                    DatasetID = "dataset:1",
                    Comments = [new MultimediaComment { Comment = "test", CommentBy = "commenter", Created = DateTime.UnixEpoch }]
                },
                new Multimedia()
            ],
            MeasurementOrFacts =
            [
                new ExtendedMeasurementOrFact
                {
                    MeasurementID = "measurement:1",
                    OccurrenceID = "occurrence:1",
                    MeasurementType = "measurement type",
                    MeasurementTypeID = "type:1",
                    MeasurementValue = "42",
                    MeasurementValueID = "value:1",
                    MeasurementAccuracy = "accuracy",
                    MeasurementUnit = "unit",
                    MeasurementUnitID = "unit:1",
                    MeasurementDeterminedDate = "2026-09-21",
                    MeasurementDeterminedBy = "determined by",
                    MeasurementMethod = "method",
                    MeasurementRemarks = "remarks"
                }
            ],
            Weather = new Weather
            {
                SnowCover = Enums.Weather.SnowCover.SnowFreeGround,
                Sunshine = new Measuring { Value = 50, Unit = Enums.Unit.Percent },
                AirTemperature = new Measuring { Value = 12.5, Unit = Enums.Unit.GraderCelsius },
                WindDirection = Enums.CompassDirection.Southwest,
                WindDirectionDegrees = new Measuring { Value = 225, Unit = Enums.Unit.Kompassgrader },
                WindSpeed = new Measuring { Value = 4.5, Unit = Enums.Unit.Ms },
                WindStrength = Enums.Weather.WindStrength.ModerateBreeze4To7Ms,
                Precipitation = Enums.Weather.Precipitation.LightRain,
                Visibility = Enums.Weather.Visibility.Good10To20Km,
                Cloudiness = Enums.Weather.Cloudiness.Clear0Of8
            }
        };
        AssertSameJson(source, source.ToEventDto()!);
    }

    [Fact]
    public void ChecklistEvent_Null_ReturnsNull()
    {
        Event? source = null;
        source.ToEventDto().Should().BeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ChecklistEvent_PreservesNullAndEmptyCollections(bool emptyCollections)
    {
        var source = new Event
        {
            Media = emptyCollections ? [] : null!,
            MeasurementOrFacts = emptyCollections ? [] : null!
        };

        AssertSameJson(source, source.ToEventDto()!);
    }

    [Theory]
    [InlineData(nameof(Weather.SnowCover))]
    [InlineData(nameof(Weather.WindDirection))]
    [InlineData(nameof(Weather.WindStrength))]
    [InlineData(nameof(Weather.Precipitation))]
    [InlineData(nameof(Weather.Visibility))]
    [InlineData(nameof(Weather.Cloudiness))]
    public void ChecklistEvent_PreservesEveryWeatherEnumValue(string propertyName)
    {
        var property = typeof(Weather).GetProperty(propertyName)!;
        var enumType = Nullable.GetUnderlyingType(property.PropertyType)!;
        foreach (var value in Enum.GetValues(enumType))
        {
            var weather = new Weather();
            property.SetValue(weather, value);
            var source = new Event { Weather = weather };

            AssertSameJson(source, source.ToEventDto()!);
        }
    }

    [Fact]
    public void ChecklistEvent_PreservesMeasuringUnitsAndNullValues()
    {
        foreach (var unit in Enum.GetValues<Enums.Unit>())
        {
            var source = new Event
            {
                Weather = new Weather
                {
                    Sunshine = new Measuring { Value = 0, Unit = unit },
                    AirTemperature = new Measuring()
                }
            };

            AssertSameJson(source, source.ToEventDto()!);
        }
    }

    [Fact]
    public void ChecklistEvent_PreservesNullItemsInMeasurementsAndComments()
    {
        var source = new Event
        {
            Media = [new Multimedia { Comments = [null!, new MultimediaComment { Comment = "test" }] }],
            MeasurementOrFacts = [null!, new ExtendedMeasurementOrFact { MeasurementID = "measurement:1" }]
        };

        AssertSameJson(source, source.ToEventDto()!);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Multimedia_PreservesNullAndEmptyComments(bool emptyComments)
    {
        var source = new Multimedia { Comments = emptyComments ? [] : null! };
        AssertSameJson(source, source.ToMultimediaDto());
    }

    [Fact]
    public void HangfireJob_PreservesMongoMappingAndJson()
    {
        var bson = new BsonDocument
        {
            { "_id", ObjectId.Parse("507f1f77bcf86cd799439011") },
            { "CreatedAt", new BsonDateTime(new DateTime(2026, 9, 21, 12, 30, 0, DateTimeKind.Utc)) },
            { "StateName", "Succeeded" },
            { "InvocationData", "{}" },
            { "UnusedMongoField", true }
        };
        var job = BsonSerializer.Deserialize<HangfireJob>(bson);
        AssertSameJson(job, job.ToHangfireJobDto());
    }
}
