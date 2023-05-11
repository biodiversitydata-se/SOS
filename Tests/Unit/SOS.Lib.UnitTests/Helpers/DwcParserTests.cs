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
        [MemberData(nameof(DwcEventDateWithTimeTestData))]
        public void Parse_DwC_EventDateWithTime_fields(
            string eventDate,
            string year,
            string month,
            string day,
            string eventTime,
            bool expected,
            DateTime? expectedStartDate,
            DateTime? expectedEndDate,
            TimeSpan? expextedStartTime,
            TimeSpan? expectedEndtime)
        {
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = DwcParser.TryParseEventDate(eventDate, year, month, day, eventTime, out var startDate, out var endDate, out var startTime, out var endTime);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Should().Be(expected);
            startDate.Should().Be(expectedStartDate);
            endDate.Should().Be(expectedEndDate);
            startTime.Should().Be(expextedStartTime);
            endTime.Should().Be(expectedEndtime);
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
                new object[] { null, null, null, null, false, null, null },
                new object[] { "a", "b", "c", "d", false, null, null },
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

        public static IEnumerable<object[]> DwcEventDateWithTimeTestData =>
            new List<object[]>
            {
                new object[]
                {
                    "2009-02-20", null, null, null, null, true,
                    new DateTime(2009, 2, 20),
                    new DateTime(2009, 2, 20),
                    null,
                    null
                },
                new object[]
                {
                    "2009-02-20T08:40Z", "2009", "2", "20", null, true,
                    new DateTime(2009, 2, 20, 9, 40, 0),
                    new DateTime(2009, 2, 20, 9, 40, 0),
                    new TimeSpan( 9, 40, 0),
                    new TimeSpan( 9, 40, 0)
                }, 
                new object[]
                {
                    "2021-10-23T09:04:12.000Z", null, null, null, "09:04:12.000Z", true,
                    new DateTime(2021, 10, 23, 11, 4, 12),
                    new DateTime(2021, 10, 23, 11, 4, 12),
                    new TimeSpan(11, 4, 12),
                    new TimeSpan(11, 4, 12)
                },
                new object[]
                {
                    "2021-10-01T13:12:31", null, null, null, "11:12:31Z", true,
                    new DateTime(2021, 10, 1, 13, 12, 31),
                    new DateTime(2021, 10, 1, 13, 12, 31),
                    new TimeSpan(13, 12, 31),
                    new TimeSpan(13, 12, 31)
                },
                new object[]
                {
                    "2020-06-18", null, null, null, "08:00/10:40", true,
                    new DateTime(2020, 6, 18, 8, 0, 0),
                    new DateTime(2020, 6, 18, 10, 40, 0),
                    new TimeSpan( 8, 0, 0),
                    new TimeSpan(10, 40, 0)
                },
                new object[]
                {
                    "2020-06-18", null, null, null, "00:00:00", true,
                    new DateTime(2020, 6, 18, 0, 0, 0),
                    new DateTime(2020, 6, 18, 0, 0, 0),
                    new TimeSpan(0, 0, 0),
                    new TimeSpan(0, 0, 0)
                }
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
                new object[] { "2007-11-13", "31", false, null, null }, // 2007-11-13/31 There is only 30 days in november
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
            DwcParser.ParseDate("1995-09-13 00:00:00.0").Should().Be(new DateTime(1995, 9, 13));
            DwcParser.ParseDate("1900-01-01 00:00:00.0").Should().Be(new DateTime(1900, 1, 1));
            DwcParser.ParseDate("2009-08-06 18:25:01.0000000").Should().Be(new DateTime(2009, 8, 6, 18, 25, 1));
            DwcParser.ParseDate("2019-03-18T08:13:26.000Z").Should().Be(DateTime.SpecifyKind(new DateTime(2019, 3, 18, 8, 13, 26), DateTimeKind.Utc).ToLocalTime());
        }

        [Fact]
        public void Test_parse_virtual_herbarium_dates()
        {            
            DwcParser.ParseDate("1907-8-14").Should().Be(new DateTime(1907, 8, 14));
            DwcParser.ParseDate("2020-7-3").Should().Be(new DateTime(2020, 7, 3));
            DwcParser.ParseDate("2020-10-3").Should().Be(new DateTime(2020, 10, 3));
            DwcParser.ParseDate("1889-8").Should().Be(new DateTime(1889,08,1));
            DwcParser.ParseDate("18-9").Should().BeNull();            
        }
    }
}