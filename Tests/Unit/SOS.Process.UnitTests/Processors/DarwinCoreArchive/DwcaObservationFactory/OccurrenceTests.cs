using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Moq;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.FieldMappingValues;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Processors.DarwinCoreArchive;
using SOS.Process.UnitTests.TestHelpers;
using SOS.Process.UnitTests.TestHelpers.Factories;
using SOS.TestHelpers.Gis;
using SOS.TestHelpers.Helpers.Builders;
using Xunit;

namespace SOS.Process.UnitTests.Processors.DarwinCoreArchive.DwcaObservationFactory
{
    [CollectionDefinition("DwcaObservationFactory collection")]
    public class OccurrenceTests : IClassFixture<DwcaObservationFactoryFixture>
    {
        private readonly DwcaObservationFactoryFixture _fixture;

        public OccurrenceTests(DwcaObservationFactoryFixture fixture)
        {
            _fixture = fixture;
        }
    
        [Fact]
        public void Sex_with_Male_value_is_field_mapped()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithSex("Male")
                .Build();
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.Gender.Id.Should().Be((int)GenderId.Male);
        }

        [Fact]
        public void Sex_with_female_value_is_field_mapped()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithSex("female")
                .Build();
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.Gender.Id.Should().Be((int)GenderId.Female);
        }

        [Fact]
        public void Sex_with_Misspelled_value_is_mapped_to_custom_value()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithSex("Malle") // misspelled
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.Gender.Id.Should().Be((int)FieldMappingConstants.NoMappingFoundCustomValueIsUsedId);
            result.Occurrence.Gender.Value.Should().Be("Malle");
        }

        [Fact]
        public void Sex_with_null_value_is_not_field_mapped()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithSex(null)
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.Gender.Should().BeNull();
        }

        /// <summary>
        /// Life Stage GBIF Vocabulary
        /// http://rs.gbif.org/vocabulary/gbif/life_stage.xml
        /// zygote
        /// embryo
        /// larva
        /// juvenile
        /// adult
        /// sporophyte
        /// spore
        /// gametophyte
        /// gamete
        /// pupa
        /// </summary>
        /// <remarks>
        /// There exist multiple synonyms for several values. For example synonyms to 'larva' are:
        /// larvae, tadpole, polliwog, polliwig, polewig, polwig, planula, nauplius, zoea, nymph, caterpillar, grub
        /// maggot, wriggler, trochophore, veliger, glochidium, ammocoete, leptocephalus, bipinnaria
        /// </remarks>
        [Fact]
        public void GBIF_LifeStage_vocabulary()
        {
        }

  
        [Fact]
        public void LifeStage_value_egg_is_field_mapped()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithLifeStage("egg")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.LifeStage.Id.Should().Be((int) LifeStageId.Egg);
        }

        [Fact]
        public void LifeStage_value_juvenile_is_field_mapped()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithLifeStage("juvenile")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.LifeStage.Id.Should().Be((int)LifeStageId.Juvenile);
        }

        [Fact]
        public void LifeStage_value_adult_is_field_mapped()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithLifeStage("adult")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.LifeStage.Id.Should().Be((int)LifeStageId.Adult);
        }

        [Fact]
        public void ReproductiveCondition_value_pregnant_is_field_mapped_to_Activity()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithReproductiveCondition("non-reproductive")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.Activity.Id.Should().Be((int)ActivityId.PregnantFemale);
        }

        [Fact]
        public void ReproductiveCondition_value_in_bloom_is_field_mapped_to_LifeStage()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithReproductiveCondition("fruit-bearing")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.LifeStage.Id.Should().Be((int)LifeStageId.InFruit);
        }

        [Fact]
        public void Behavior_value_foraging_is_field_mapped_to_Activity()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithBehavior("foraging")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.Activity.Id.Should().Be((int)ActivityId.Foraging);
        }

        [Fact]
        public void OccurrenceStatus_value_present_is_field_mapped_to_OccurrenceStatus()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithOccurrenceStatus("present")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.OccurrenceStatus.Id.Should().Be((int)OccurrenceStatusId.Present);
            result.Occurrence.IsPositiveObservation.Should().BeTrue();
            result.Occurrence.IsNeverFoundObservation.Should().BeFalse();
            result.Occurrence.IsNotRediscoveredObservation.Should().BeFalse();
            result.Occurrence.IsNaturalOccurrence.Should().BeTrue();
        }

        [Fact]
        public void OccurrenceStatus_value_absent_is_field_mapped_to_OccurrenceStatus()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithOccurrenceStatus("absent")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.OccurrenceStatus.Id.Should().Be((int)OccurrenceStatusId.Absent);
            result.Occurrence.IsPositiveObservation.Should().BeFalse();
            result.Occurrence.IsNeverFoundObservation.Should().BeTrue();
            result.Occurrence.IsNotRediscoveredObservation.Should().BeFalse();
            result.Occurrence.IsNaturalOccurrence.Should().BeTrue();
        }

    }
}