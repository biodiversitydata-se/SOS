using FluentAssertions;
using SOS.Lib.Constants;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Process.UnitTests.TestHelpers;
using SOS.TestHelpers.Helpers.Builders;
using Xunit;

namespace SOS.Process.UnitTests.Processors.DarwinCoreArchive.DwcaObservationFactory
{
    [CollectionDefinition("DwcaObservationFactory collection")]
    public class OccurrenceTests : IClassFixture<DwcaObservationFactoryFixture>
    {
        public OccurrenceTests(DwcaObservationFactoryFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly DwcaObservationFactoryFixture _fixture;

        [Fact(Skip = "Behavior mapping not completed yet")]
        public void Behavior_field_with_value_foraging_is_mapped_to_Activity_vocabulary()
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
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.Activity.Id.Should().Be((int)ActivityId.Foraging);
        }

        /// <remarks>
        ///     Life Stage GBIF Vocabulary
        ///     http://rs.gbif.org/vocabulary/gbif/life_stage.xml
        /// </remarks>
        [Theory]
        [InlineData("adult", LifeStageId.Adult)]
        [InlineData("egg", LifeStageId.Egg)]
        [InlineData("eggs", LifeStageId.Egg)]
        [InlineData("juvenile", LifeStageId.Juvenile)]
        public void LifeStage_field_with_valid_value_is_mapped_to_LifeStage_vocabulary(
            string lifeStageValue,
            LifeStageId expectedLifeStageId)
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithLifeStage(lifeStageValue)
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var observation = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observation.Occurrence.LifeStage.Id.Should().Be((int)expectedLifeStageId);
        }

        [Fact]
        public void OccurrenceStatus_field_with_value_absent_is_mapped_to_OccurrenceStatus_vocabulary()
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
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.OccurrenceStatus.Id.Should().Be((int)OccurrenceStatusId.Absent);
            result.Occurrence.IsPositiveObservation.Should().BeFalse();
            result.Occurrence.IsNeverFoundObservation.Should().BeTrue();
            result.Occurrence.IsNotRediscoveredObservation.Should().BeFalse();
            result.Occurrence.IsNaturalOccurrence.Should().BeTrue();
        }

        [Fact]
        public void OccurrenceStatus_field_with_value_present_is_mapped_to_OccurrenceStatus_vocabulary()
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
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.OccurrenceStatus.Id.Should().Be((int)OccurrenceStatusId.Present);
            result.Occurrence.IsPositiveObservation.Should().BeTrue();
            result.Occurrence.IsNeverFoundObservation.Should().BeFalse();
            result.Occurrence.IsNotRediscoveredObservation.Should().BeFalse();
            result.Occurrence.IsNaturalOccurrence.Should().BeTrue();
        }

        [Fact(Skip = "ReproductiveCondition mapping not completed yet")]
        public void ReproductiveCondition_field_with_value_fruitbearing_is_mapped_to_LifeStage_vocabulary()
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
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.LifeStage.Id.Should().Be((int)LifeStageId.InFruit);
        }

        [Fact(Skip = "ReproductiveCondition mapping not completed yet")]
        public void ReproductiveCondition_field_with_value_pregnant_is_mapped_to_Activity_vocabulary()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithReproductiveCondition("pregnant")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.Activity.Id.Should().Be((int)ActivityId.PregnantFemale);
        }

        [Theory]
        [InlineData("female", SexId.Female)]
        [InlineData("Male", SexId.Male)]
        public void Sex_field_with_valid_value_is_mapped_to_gender_vocabulary(
            string sexFieldValue,
            SexId expectedGenderId)
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithSex(sexFieldValue)
                .Build();
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.Sex.Id.Should().Be((int)expectedGenderId);
        }

        [Fact]
        public void Sex_field_with_misspelled_value_is_mapped_to_custom_value()
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
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.Sex.Id.Should().Be(VocabularyConstants.NoMappingFoundCustomValueIsUsedId);
            result.Occurrence.Sex.Value.Should().Be("Malle");
        }

        [Fact]
        public void Sex_field_with_null_value_is_not_mapped_to_gender_vocabulary()
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
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.Sex.Should().BeNull();
        }
    }
}