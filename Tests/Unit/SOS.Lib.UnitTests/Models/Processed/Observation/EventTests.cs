using System;
using System.Collections.Generic;
using FluentAssertions;
using SOS.Lib.Models.Processed.Observation;
using Xunit;

namespace SOS.Lib.UnitTests.Models.Processed.Observation
{
    public class EventTests
    {
        [Theory]
        [MemberData(nameof(EventDatesTestData))]
        public void Test_Create_Event_With_Different_Dates(
            DateTime? startDate,
            TimeSpan? startTime,
            DateTime? endDate,
            TimeSpan? endTime,            
            DateTime? expectedStartDate,
            DateTime? expectedEndDate,
            string expextedPlainStartDate,
            string expextedPlainStartTime,
            string expextedPlainEndDate,            
            string expextedPlainEndTime,
            string expectedVerbatimEventDate)
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------            
            var ev = new Event(startDate, startTime, endDate, endTime);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------                        
            ev.StartDate.Value.Should().Be(expectedStartDate);
            ev.EndDate.Value.Should().Be(expectedEndDate);
            ev.PlainStartDate.Should().Be(expextedPlainStartDate);
            ev.PlainStartTime.Should().Be(expextedPlainStartTime);
            ev.PlainEndDate.Should().Be(expextedPlainEndDate);            
            ev.PlainEndTime.Should().Be(expextedPlainEndTime);
            ev.VerbatimEventDate.Should().Be(expectedVerbatimEventDate);
        }

        public static IEnumerable<object[]> EventDatesTestData =>
            new List<object[]>
            {
                new object[] // Unspecified date kind - without time
                {
                    new DateTime(2009, 2, 20), // startDate
                    null, // startTime
                    new DateTime(2009, 2, 20, 23, 59, 59), // endDate
                    null, // endTime
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20), DateTimeKind.Local).ToUniversalTime(), // expectedStartDate
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20, 23, 59, 59), DateTimeKind.Local).ToUniversalTime(), // expectedEndDate
                    "2009-02-20", // expextedPlainStartDate
                    null, // expextedPlainStartTime
                    "2009-02-20", // expextedPlainEndDate
                    null, // expextedPlainEndTime
                    "2009-02-20" // expectedVerbatimEventDate
                },
                new object[] // UTC date kind - without time
                {
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20), DateTimeKind.Local).ToUniversalTime(),
                    null,
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20, 23, 59, 59), DateTimeKind.Local).ToUniversalTime(),
                    null,
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20), DateTimeKind.Local).ToUniversalTime(),
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20, 23, 59, 59), DateTimeKind.Local).ToUniversalTime(),
                    "2009-02-20",
                    null,
                    "2009-02-20",
                    null,
                    "2009-02-20"
                },
                new object[] // Local date kind - without time
                {
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20), DateTimeKind.Local),
                    null,
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20, 23, 59, 59), DateTimeKind.Local),
                    null,
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20), DateTimeKind.Local).ToUniversalTime(),
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20, 23, 59, 59), DateTimeKind.Local).ToUniversalTime(),
                    "2009-02-20",
                    null,
                    "2009-02-20",
                    null,
                    "2009-02-20"
                },
                new object[] // Unspecified date kind - with time
                {
                    new DateTime(2009, 2, 20, 9, 40, 0),
                    new TimeSpan(9, 40, 0),
                    new DateTime(2009, 2, 20, 11, 30, 0),
                    new TimeSpan(11, 30, 0),
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20, 9, 40, 0), DateTimeKind.Local).ToUniversalTime(),
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20, 11, 30, 0), DateTimeKind.Local).ToUniversalTime(),
                    "2009-02-20",
                    "09:40",
                    "2009-02-20",
                    "11:30",
                    "2009-02-20T09:40:00+01:00/2009-02-20T11:30:00+01:00"
                },
                new object[] // UTC date kind - with time
                {
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20, 9, 40, 0), DateTimeKind.Local).ToUniversalTime(),
                    new TimeSpan(9, 40, 0),
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20, 11, 30, 0), DateTimeKind.Local).ToUniversalTime(),
                    new TimeSpan(11, 30, 0),
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20, 9, 40, 0), DateTimeKind.Local).ToUniversalTime(),
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20, 11, 30, 0), DateTimeKind.Local).ToUniversalTime(),
                    "2009-02-20",
                    "09:40",
                    "2009-02-20",
                    "11:30",
                    "2009-02-20T09:40:00+01:00/2009-02-20T11:30:00+01:00"
                },
                new object[] // Local date kind - with time
                {
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20, 9, 40, 0), DateTimeKind.Local),
                    new TimeSpan(9, 40, 0),
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20, 11, 30, 0), DateTimeKind.Local),
                    new TimeSpan(11, 30, 0),
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20, 9, 40, 0), DateTimeKind.Local).ToUniversalTime(),
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20, 11, 30, 0), DateTimeKind.Local).ToUniversalTime(),
                    "2009-02-20",
                    "09:40",
                    "2009-02-20",
                    "11:30",
                    "2009-02-20T09:40:00+01:00/2009-02-20T11:30:00+01:00"
                }
            };      
    }
}