using System;
using System.Collections.Generic;
using FluentAssertions;
using SOS.Lib.Helpers;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers
{
    public class DwcParserTests
    {
        [Theory]
        [MemberData(nameof(DwcEventDateTestData))]
        public void Parse_DwC_EventDate_fields(
            string eventDate,
            string year,
            string month,
            string day,
            bool expected,
            DateTime? expectedStartDate,
            DateTime? expectedEndDate)
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = DwcParser.TryParseEventDate(eventDate, year, month, day, out var startDate, out var endDate);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().Be(expected);
            startDate.Should().Be(expectedStartDate);
            endDate.Should().Be(expectedEndDate);
        }

        [Theory]
        [MemberData(nameof(DwcEventIntervalDateTestData))]
        public void Parse_DwC_IntervalEventDate_fields(
            string strStartDate,
            string strEndDate,
            bool expected, DateTime?
                expectedStartDate,
            DateTime? expectedEndDate)
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result =
                DwcParser.TryParseIntervalEventDate(strStartDate, strEndDate, out var startDate, out var endDate);

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
                new object[] {null, null, null, null, false, null, null},
                new object[] {"a", "b", "c", "d", false, null, null},
                new object[]
                {
                    "1971", "1971", "", "", true, new DateTime(1971, 1, 1), new DateTime(1971, 12, 31)
                }, // 1971 (some time in the year 1971)
                new object[]
                {
                    "", "1971", "", "", true, new DateTime(1971, 1, 1), new DateTime(1971, 12, 31)
                }, // 1971 (some time in the year 1971)
                new object[]
                {
                    null, "1971", "", "", true, new DateTime(1971, 1, 1), new DateTime(1971, 12, 31)
                }, // 1971 (some time in the year 1971)
                new object[]
                {
                    "1906-06", "1906", "6", "", true, new DateTime(1906, 6, 1), new DateTime(1906, 6, 30)
                }, // 1906-06 (some time in June 1906)
                new object[]
                {
                    "", "1906", "6", "", true, new DateTime(1906, 6, 1), new DateTime(1906, 6, 30)
                }, // 1906-06 (some time in June 1906)
                new object[]
                {
                    "1809-02-12", "1809", "02", "12", true, new DateTime(1809, 2, 12), new DateTime(1809, 2, 12)
                }, // 1809-02-12 (some time during 12 February 1809)
                new object[]
                {
                    "12/02/1809", "1809", "02", "12", true, new DateTime(1809, 2, 12), new DateTime(1809, 2, 12)
                }, // 1809-02-12 (some time during 12 February 1809)
                new object[]
                {
                    "12/02/1809", "", "", "", false, null, null
                }, // Can't decide if the format is MM/dd/yyyy or dd/MM/yyyy.
                new object[]
                {
                    "", "1809", "02", "12", true, new DateTime(1809, 2, 12), new DateTime(1809, 2, 12)
                }, // 1809-02-12 (some time during 12 February 1809)
                new object[]
                {
                    "2018-08-29T15:19", "2018", "8", "29", true, new DateTime(2018, 8, 29, 15, 19, 0),
                    new DateTime(2018, 8, 29, 15, 19, 0)
                }, // 2018-08-29T15:19 (3:19pm local time on 29 August 2018).
                new object[]
                {
                    "2009-02-20T08:40Z", "2009", "2", "20", true,
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20, 8, 40, 0), DateTimeKind.Utc).ToLocalTime(),
                    DateTime.SpecifyKind(new DateTime(2009, 2, 20, 8, 40, 0), DateTimeKind.Utc).ToLocalTime()
                }, // 2009-02-20T08:40Z (20 February 2009 8:40am UTC).
                new object[]
                {
                    "1963-03-08T14:07-0600", "1963", "3", "8", true,
                    DateTime.SpecifyKind(new DateTime(1963, 3, 8, 14, 7, 0), DateTimeKind.Utc)
                        .Add(TimeSpan.FromHours(6)).ToLocalTime(),
                    DateTime.SpecifyKind(new DateTime(1963, 3, 8, 14, 7, 0), DateTimeKind.Utc)
                        .Add(TimeSpan.FromHours(6)).ToLocalTime()
                }, // 1963-03-08T14:07-0600 (8 Mar 1963 at 2:07pm in the time zone six hours earlier than UTC).
                new object[]
                {
                    "1900/1909", "", "", "", true, new DateTime(1900, 1, 1), new DateTime(1909, 12, 31)
                }, // 1900/1909 (some time during the interval between the beginning of the year 1900 and the end of the year 1909).
                new object[]
                {
                    "2007-11-13/15", "", "", "", true, new DateTime(2007, 11, 13), new DateTime(2007, 11, 15)
                }, // 2007-11-13/15 (some time in the interval between 13 November 2007 and 15 November 2007).
                new object[]
                {
                    "2007-03-01T13:00:00Z/2008-05-11T15:30:00Z", "", "", "", true,
                    DateTime.SpecifyKind(new DateTime(2007, 3, 1, 13, 0, 0), DateTimeKind.Utc).ToLocalTime(),
                    DateTime.SpecifyKind(new DateTime(2008, 5, 11, 15, 30, 0), DateTimeKind.Utc).ToLocalTime()
                } // 2007-03-01T13:00:00Z/2008-05-11T15:30:00Z (some time during the interval between 1 March 2007 1pm UTC and 11 May 2008 3:30pm UTC).
            };

        public static IEnumerable<object[]> DwcEventIntervalDateTestData =>
            new List<object[]>
            {
                new object[]
                {
                    "1900", "1909", true, new DateTime(1900, 1, 1), new DateTime(1909, 12, 31)
                }, // 1900/1909 (some time during the interval between the beginning of the year 1900 and the end of the year 1909).
                new object[]
                {
                    "1900", "1909-12-31", true, new DateTime(1900, 1, 1), new DateTime(1909, 12, 31)
                }, // 1900/1909-12-31 (some time during the interval between the beginning of the year 1900 and the end of the year 1909).
                new object[]
                {
                    "2007-11-13", "15", true, new DateTime(2007, 11, 13), new DateTime(2007, 11, 15)
                }, // 2007-11-13/15 (some time in the interval between 13 November 2007 and 15 November 2007).
                new object[] {"2007-11-13", "31", false, null, null}, // 2007-11-13/31 There is only 30 days in november
                new object[]
                {
                    "2007-11-13", "2007-11-15", true, new DateTime(2007, 11, 13), new DateTime(2007, 11, 15)
                }, // 2007-11-13/2007-11-15 (some time in the interval between 13 November 2007 and 15 November 2007).
                new object[]
                {
                    "2007-03-01T13:00:00Z", "2008-05-11T15:30:00Z", true,
                    DateTime.SpecifyKind(new DateTime(2007, 3, 1, 13, 0, 0), DateTimeKind.Utc).ToLocalTime(),
                    DateTime.SpecifyKind(new DateTime(2008, 5, 11, 15, 30, 0), DateTimeKind.Utc).ToLocalTime()
                } // 2007-03-01T13:00:00Z/2008-05-11T15:30:00Z (some time during the interval between 1 March 2007 1pm UTC and 11 May 2008 3:30pm UTC).
            };

        [Fact]
        [Trait("Category", "Unit")]
        public void Date_can_be_formatted_then_parsed_and_then_formatted_again_to_same_representation()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var date = DateTime.UtcNow;
            var originalFormattedDate = DwcFormatter.CreateDateString(date);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            DwcParser.TryParseEventDate(
                originalFormattedDate,
                null,
                null,
                null,
                out var startDate,
                out var endDate);
            var formattedDate = DwcFormatter.CreateDateString(startDate);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            formattedDate.Should().Be(originalFormattedDate);
        }

        [Fact]
        public void ParseDate_parses_ISO8601_dates_and_the_formats_specified_as_argument()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act, Assert
            //-----------------------------------------------------------------------------------------------------------
            DwcParser.ParseDate("1971", "yyyy").Should().Be(new DateTime(1971, 1, 1));
            DwcParser.ParseDate("1906-06", "yyyy-MM").Should().Be(new DateTime(1906, 6, 1));
            DwcParser.ParseDate("1809-02-12").Should().Be(new DateTime(1809, 2, 12));
            DwcParser.ParseDate("12/02/1809").Should().Be(null,
                "because the parser can't decide if we meant the format MM/dd/yyyy or dd/MM/yyyy.");
            DwcParser.ParseDate("12/02/1809", "dd/MM/yyyy").Should().Be(new DateTime(1809, 2, 12));
            DwcParser.ParseDate("2018-08-29T15:19").Should().Be(new DateTime(2018, 8, 29, 15, 19, 0));
            DwcParser.ParseDate("2009-02-20T08:40Z").Should()
                .Be(DateTime.SpecifyKind(new DateTime(2009, 2, 20, 8, 40, 0), DateTimeKind.Utc).ToLocalTime());
            DwcParser.ParseDate("1963-03-08T14:07-0600").Should()
                .Be(DateTime.SpecifyKind(new DateTime(1963, 3, 8, 14, 7, 0), DateTimeKind.Utc)
                    .Add(TimeSpan.FromHours(6)).ToLocalTime());
        }
    }
}