using AutoBogus;
using FluentAssertions;
using SOS.Api.Contracts.Observations.Requests;
using SOS.Api.Contracts.Lookups.Responses;
using SOS.Lib.Helpers;
using SOS.Lib.Models;
using SOS.Lib.Models.Processed.Observation;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace SOS.Lib.UnitTests.Models.Processed.Observation;

public class FlatObservationTests
{
    [Theory]
    [InlineData("2026-01-15T10:00:00Z", "2026-01-15T11:00:00+01:00")]
    [InlineData("2026-07-15T10:00:00Z", "2026-07-15T12:00:00+02:00")]
    public void FlatDates_SerializeSwedishTimeWithOffset_AndPreserveInstant(string utc, string expected)
    {
        var instant = DateTimeOffset.Parse(utc).UtcDateTime;
        var observation = new Lib.Models.Processed.Observation.Observation
        {
            Created = instant,
            Modified = instant,
            Event = new Event(instant),
            Occurrence = new Occurrence { ReportedDate = instant }
        };
        var flat = new FlatObservation(observation);

        foreach (var field in new[] { "created", "modified", "event.startdate", "event.enddate", "occurrence.reporteddate" })
        {
            var property = new PropertyFieldDescription { PropertyPath = field };
            using var json = JsonDocument.Parse(JsonSerializer.Serialize(flat.GetValue(property)));
            Assert.Equal(expected, json.RootElement.GetString());
            Assert.Equal(instant, json.RootElement.GetDateTimeOffset().UtcDateTime);
        }
    }

    [Fact]
    public void Test_FlatObservation_Mappings()
    {
        //-----------------------------------------------------------------------------------------------------------
        // Arrange
        //-----------------------------------------------------------------------------------------------------------            
        var propertyFields = ObservationPropertyFieldDescriptionHelper.FieldsByFieldSet[OutputFieldSet.All];
        var observation = AutoFaker.Generate<Lib.Models.Processed.Observation.Observation>();
        var flatObservation = new FlatObservation(observation);

        //-----------------------------------------------------------------------------------------------------------
        // Act
        //-----------------------------------------------------------------------------------------------------------
        var stringValueByProperty = new Dictionary<string, string>();
        var propertiesWithErrors = new List<PropertyFieldDescription>();
        foreach (var field in propertyFields)
        {
            try
            {
                string stringValue = flatObservation.GetStringValue(field);
                stringValueByProperty.Add(field.PropertyPath, stringValue);
            }
            catch
            {
                propertiesWithErrors.Add(field);
            }
        }

        //-----------------------------------------------------------------------------------------------------------
        // Assert
        //-----------------------------------------------------------------------------------------------------------            
        //string mappingCode = string.Join(",\r\n", propertiesWithErrors.Select(m => $"\"{m.PropertyPath.ToLower()}\" => \"N/A\""));
        propertiesWithErrors.Should().BeEmpty();
        stringValueByProperty.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("2025-12-31T23:30:00Z", "2026-01-01")]
    [InlineData("2026-07-15T22:30:00Z", "2026-07-16")]
    public void TextExportDates_KeepTheSwedishDateOnlyFormat(string timestamp, string swedishDate)
    {
        var instant = DateTimeOffset.Parse(timestamp).UtcDateTime;
        var flat = new FlatObservation(new Lib.Models.Processed.Observation.Observation
        {
            Created = instant,
            Modified = instant,
            Event = new Event(instant),
            Occurrence = new Occurrence { ReportedDate = instant }
        });

        foreach (var field in new[] { "created", "modified", "event.startdate", "event.enddate", "occurrence.reporteddate" })
        {
            var property = new PropertyFieldDescription { PropertyPath = field, DataTypeEnum = PropertyFieldDataType.DateTime };
            Assert.Equal(DateTime.Parse(swedishDate).ToShortDateString(), flat.GetStringValue(property));
        }
    }
}
