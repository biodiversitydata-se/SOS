using System;
using System.Collections.Generic;
using FluentAssertions;
using SOS.Lib.Helpers;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers
{
    public class DwcTimeParserTests
    {
        [Theory]
        [MemberData(nameof(DwcEventTimeTestData))]
        public void Parse_DwC_EventTime_fields(
            string eventTime,
            DateTime date,
            DateTime? expected)
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = DwcParser.ParseTime(eventTime, date, null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().Be(expected);
        }

        public static IEnumerable<object[]> DwcEventTimeTestData =>
            new List<object[]>
            {
                new object[] {null, new DateTime(2010, 6, 13), new DateTime(2010,6, 13) },
                new object[] {"", new DateTime(2010, 6, 13), new DateTime(2010, 6, 13) },
                new object[] {"   ", new DateTime(2010, 6, 13), new DateTime(2010, 6, 13) },
                new object[] {"a", new DateTime(2010, 6, 13), new DateTime(2010, 6, 13) },
                new object[] { "12:50:00", new DateTime(2010,6, 13), new DateTime(2010,6,13,12,50,0)},
                new object[] { "12:50:00+02", new DateTime(2010,6, 13), DateTime.SpecifyKind(new DateTime(2010,6,13,12,50,0), DateTimeKind.Utc)
                    .Subtract(TimeSpan.FromHours(2)).ToLocalTime()},
                new object[] { "12:50:00+0200", new DateTime(2010,6, 13), DateTime.SpecifyKind(new DateTime(2010,6,13,12,50,0), DateTimeKind.Utc)
                    .Subtract(TimeSpan.FromHours(2)).ToLocalTime()},
                new object[] { "12:50:00+03", new DateTime(2010,6, 13), DateTime.SpecifyKind(new DateTime(2010,6,13,12,50,0), DateTimeKind.Utc)
                    .Subtract(TimeSpan.FromHours(3)).ToLocalTime()},
                new object[] { "14:07-0600", new DateTime(2021,1,1), DateTime.SpecifyKind(new DateTime(2021, 1, 1, 14, 7, 0), DateTimeKind.Utc)
                    .Add(TimeSpan.FromHours(6)).ToLocalTime()},
                new object[] { "08:40:21Z", new DateTime(2021,1,1), DateTime.SpecifyKind(new DateTime(2021, 1, 1, 8, 40, 21), DateTimeKind.Utc)
                    .ToLocalTime()},
                
            };

        [Theory]
        [MemberData(nameof(DwcEventDateTestData))]
        public void Parse_DwC_EventDate_fields(
            string eventDate,
            string year,
            string month,
            string day,
            string eventTime,
            bool expected,
            DateTime? expectedStartDate,
            DateTime? expectedEndDate)
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = DwcParser.TryParseEventDate(eventDate, year, month, day, eventTime, out var startDate, out var endDate);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().Be(expected);
            startDate.Should().Be(expectedStartDate);
            endDate.Should().Be(expectedEndDate);
        }

        public static IEnumerable<object[]> DwcEventDateTestData =>
            new List<object[]>
            {
                new object[] {null, null, null, null, null, false, null, null},
                new object[] {"a", "b", "c", "d", "time", false, null, null},
                new object[]
                {
                    "1971", "1971", "", "", "time", true, new DateTime(1971, 1, 1), new DateTime(1971, 12, 31)
                }, // 1971 (some time in the year 1971)
                new object[]
                {
                    "", "1971", "", "", "time", true, new DateTime(1971, 1, 1), new DateTime(1971, 12, 31)
                }, // 1971 (some time in the year 1971)
                new object[]
                {
                    null, "1971", "", "", "time", true, new DateTime(1971, 1, 1), new DateTime(1971, 12, 31)
                }, // 1971 (some time in the year 1971)
                new object[]
                {
                    "1906-06", "1906", "6", "", "time", true, new DateTime(1906, 6, 1), new DateTime(1906, 6, 30)
                }, // 1906-06 (some time in June 1906)
                new object[]
                {
                    "", "1906", "6", "", "time", true, new DateTime(1906, 6, 1), new DateTime(1906, 6, 30)
                }, // 1906-06 (some time in June 1906)
                new object[]
                {
                    "1809-02-12", "1809", "02", "12", "time", true, new DateTime(1809, 2, 12), new DateTime(1809, 2, 12)
                }, // 1809-02-12 (some time during 12 February 1809)
                new object[]
                {
                    "1809-02-12", "1809", "02", "12", "18:35", true, new DateTime(1809, 2, 12, 18, 35, 0), new DateTime(1809, 2, 12, 18, 35, 0)
                }, // 1809-02-12 18:35
                new object[]
                {
                    "2010-06-13", "", "", "", "12:50:00+02/13:35:00+02", true, DateTime.SpecifyKind(new DateTime(2010,6,13,12,50,0), DateTimeKind.Utc)
                        .Subtract(TimeSpan.FromHours(2)).ToLocalTime(), DateTime.SpecifyKind(new DateTime(2010,6,13,13,35,0), DateTimeKind.Utc)
                        .Subtract(TimeSpan.FromHours(2)).ToLocalTime()
                }, // 2010-06-13 12:50:00+02/13:35:00+02
                new object[]
                {
                    "12/02/1809", "1809", "02", "12", "time", true, new DateTime(1809, 2, 12), new DateTime(1809, 2, 12)
                }, // 1809-02-12 (some time during 12 February 1809)
                new object[]
                {
                    "12/02/1809", "", "", "", "time", false, null, null
                }, // Can't decide if the format is MM/dd/yyyy or dd/MM/yyyy.
                new object[]
                {
                    "", "1809", "02", "12", "time", true, new DateTime(1809, 2, 12), new DateTime(1809, 2, 12)
                }, // 1809-02-12 (some time during 12 February 1809)
                new object[]
                {
                    "2018-08-29T15:19", "2018", "8", "29", "time", true, new DateTime(2018, 8, 29, 15, 19, 0),
                    new DateTime(2018, 8, 29, 15, 19, 0)
                }, // 2018-08-29T15:19 (3:19pm local time on 29 August 2018).
                new object[]
                {
                    "2009-02-20T08:40Z", "2009", "2", "20", "time", true,
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20, 8, 40, 0), DateTimeKind.Utc).ToLocalTime(),
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20, 8, 40, 0), DateTimeKind.Utc).ToLocalTime()
                }, // 2009-02-20T08:40Z (20 February 2009 8:40am UTC).
                new object[]
                {
                    "1963-03-08T14:07-0600", "1963", "3", "8", "time", true,
                    DateTime.SpecifyKind(new DateTime(1963, 3, 8, 14, 7, 0), DateTimeKind.Utc)
                        .Add(TimeSpan.FromHours(6)).ToLocalTime(),
                    DateTime.SpecifyKind(new DateTime(1963, 3, 8, 14, 7, 0), DateTimeKind.Utc)
                        .Add(TimeSpan.FromHours(6)).ToLocalTime()
                }, // 1963-03-08T14:07-0600 (8 Mar 1963 at 2:07pm in the time zone six hours earlier than UTC).
                new object[]
                {
                    "1900/1909", "", "", "", "time", true, new DateTime(1900, 1, 1), new DateTime(1909, 12, 31)
                }, // 1900/1909 (some time during the interval between the beginning of the year 1900 and the end of the year 1909).
                new object[]
                {
                    "2007-11-13/15", "", "", "", "time", true, new DateTime(2007, 11, 13), new DateTime(2007, 11, 15)
                }, // 2007-11-13/15 (some time in the interval between 13 November 2007 and 15 November 2007).
                new object[]
                {
                    "2007-03-01T13:00:00Z/2008-05-11T15:30:00Z", "", "", "", "time", true,
                    DateTime.SpecifyKind(new DateTime(2007, 3, 1, 13, 0, 0), DateTimeKind.Utc).ToLocalTime(),
                    DateTime.SpecifyKind(new DateTime(2008, 5, 11, 15, 30, 0), DateTimeKind.Utc).ToLocalTime()
                } // 2007-03-01T13:00:00Z/2008-05-11T15:30:00Z (some time during the interval between 1 March 2007 1pm UTC and 11 May 2008 3:30pm UTC).
            };
    }
}