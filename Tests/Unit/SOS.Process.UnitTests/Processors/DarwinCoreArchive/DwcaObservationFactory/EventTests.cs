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
        public async Task EventDate_is_parsed_correct()
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

    }
}