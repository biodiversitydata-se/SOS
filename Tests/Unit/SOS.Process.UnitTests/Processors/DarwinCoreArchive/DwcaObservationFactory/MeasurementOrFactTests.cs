using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Process.UnitTests.TestHelpers;
using SOS.TestHelpers.Helpers.Builders;
using Xunit;

namespace SOS.Process.UnitTests.Processors.DarwinCoreArchive.DwcaObservationFactory
{
    [CollectionDefinition("DwcaObservationFactory collection")]
    public class MeasurementOrFactTests : IClassFixture<DwcaObservationFactoryFixture>
    {
        public MeasurementOrFactTests(DwcaObservationFactoryFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly DwcaObservationFactoryFixture _fixture;

        [Fact]
        public async Task EventMeasurementOrFacts_with_value_is_mapped_successfully()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .Build();
            dwcaObservation.EventMeasurementOrFacts = new List<DwcMeasurementOrFact>
            {
                new DwcMeasurementOrFact
                {
                    MeasurementType = "Vegetation area", MeasurementValue = "11.07", MeasurementUnit = "m^2"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            var emofItem = result.Event.MeasurementOrFacts.First();
            emofItem.MeasurementType.Should().Be("Vegetation area");
            emofItem.MeasurementValue.Should().Be("11.07");
            emofItem.MeasurementUnit.Should().Be("m^2");
        }

        [Fact]
        public async Task EventExtendedMeasurementOrFacts_with_value_is_mapped_successfully()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .Build();
            dwcaObservation.EventExtendedMeasurementOrFacts = new List<DwcExtendedMeasurementOrFact>
            {
                new DwcExtendedMeasurementOrFact()
                {
                    MeasurementType = "Vegetation area", MeasurementValue = "11.07", MeasurementUnit = "m^2"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            var emofItem = result.Event.MeasurementOrFacts.First();
            emofItem.MeasurementType.Should().Be("Vegetation area");
            emofItem.MeasurementValue.Should().Be("11.07");
            emofItem.MeasurementUnit.Should().Be("m^2");
        }


        [Fact]
        public async Task EventMeasurementOrFacts_with_null_value_is_set_to_null()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .Build();
            dwcaObservation.EventMeasurementOrFacts = null;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Event.MeasurementOrFacts.Should().BeNull();
        }

        [Fact]
        public async Task EventMeasurementOrFacts_with_zero_item_collection_is_set_to_null()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .Build();
            dwcaObservation.EventMeasurementOrFacts = new List<DwcMeasurementOrFact>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Event.MeasurementOrFacts.Should().BeNull();
        }

        [Fact]
        public async Task ObservationMeasurementOrFacts_with_value_is_mapped_successfully()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .Build();
            dwcaObservation.ObservationMeasurementOrFacts = new List<DwcMeasurementOrFact>
            {
                new DwcMeasurementOrFact
                {
                    MeasurementType = "Vegetation area", MeasurementValue = "11.07", MeasurementUnit = "m^2"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            var emofItem = result.MeasurementOrFacts.First();
            emofItem.MeasurementType.Should().Be("Vegetation area");
            emofItem.MeasurementValue.Should().Be("11.07");
            emofItem.MeasurementUnit.Should().Be("m^2");
        }

        [Fact]
        public async Task ObservationExtendedMeasurementOrFacts_with_value_is_mapped_successfully()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .Build();
            dwcaObservation.ObservationExtendedMeasurementOrFacts = new List<DwcExtendedMeasurementOrFact>
            {
                new DwcExtendedMeasurementOrFact()
                {
                    MeasurementType = "Vegetation area", MeasurementValue = "11.07", MeasurementUnit = "m^2"
                }
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            var emofItem = result.MeasurementOrFacts.First();
            emofItem.MeasurementType.Should().Be("Vegetation area");
            emofItem.MeasurementValue.Should().Be("11.07");
            emofItem.MeasurementUnit.Should().Be("m^2");
        }

        [Fact]
        public async Task ObservationMeasurementOrFacts_with_null_value_is_set_to_null()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .Build();
            dwcaObservation.ObservationMeasurementOrFacts = null;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.MeasurementOrFacts.Should().BeNull();
        }

        [Fact]
        public async Task ObservationMeasurementOrFacts_with_zero_item_collection_is_set_to_null()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .Build();
            dwcaObservation.ObservationMeasurementOrFacts = new List<DwcMeasurementOrFact>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.MeasurementOrFacts.Should().BeNull();
        }

    }
}