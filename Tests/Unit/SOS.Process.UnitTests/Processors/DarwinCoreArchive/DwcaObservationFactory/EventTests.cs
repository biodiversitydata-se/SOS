using System;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Process.UnitTests.TestHelpers;
using SOS.TestHelpers.Helpers.Builders;
using Xunit;

namespace SOS.Process.UnitTests.Processors.DarwinCoreArchive.DwcaObservationFactory
{
    [CollectionDefinition("DwcaObservationFactory collection")]
    public class EventTests : IClassFixture<DwcaObservationFactoryFixture>
    {
        private readonly DwcaObservationFactoryFixture _fixture;

        public EventTests(DwcaObservationFactoryFixture fixture)
        {
            _fixture = fixture;
        }


        [Fact]
        public void EventDate_is_parsed_correct()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithEventDate("2019-03-18T08:13:26.000Z")
                .WithEventTime("08:13:26.000Z")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Event.StartDate.Should().Be(DateTime.SpecifyKind(new DateTime(2019, 3, 18,8,13,26), DateTimeKind.Utc));
        }

        [Fact]
        public void EventDate_with_time_interval_is_parsed_correct()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithEventDate("2010-06-13")
                .WithEventTime("12:50:00+02/13:35:00+02")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Event.StartDate.Should().Be(DateTime.SpecifyKind(new DateTime(2010, 6, 13, 12, 50, 0), DateTimeKind.Utc)
                .Subtract(TimeSpan.FromHours(2)));
            result.Event.EndDate.Should().Be(DateTime.SpecifyKind(new DateTime(2010, 6, 13, 13, 35, 0), DateTimeKind.Utc)
                .Subtract(TimeSpan.FromHours(2)));
        }

        [Fact]
        public void EventDate_with_time_is_parsed_correct()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithEventDate("2014-04-24")
                .WithEventTime("14:30:00+02")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Event.StartDate.Should().Be(DateTime.SpecifyKind(new DateTime(2014, 4, 24, 14, 30, 0), DateTimeKind.Utc)
                .Subtract(TimeSpan.FromHours(2)));
            result.Event.StartTime.Should().Be("14:30");
            result.Event.EndTime.Should().Be("14:30");
        }
    }
}