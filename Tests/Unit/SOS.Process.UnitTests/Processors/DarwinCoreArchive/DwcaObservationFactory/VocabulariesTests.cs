using FluentAssertions;
using SOS.Lib.Constants;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Process.UnitTests.TestHelpers;
using SOS.TestHelpers.Helpers.Builders;
using Xunit;

namespace SOS.Process.UnitTests.Processors.DarwinCoreArchive.DwcaObservationFactory
{
    [CollectionDefinition("DwcaObservationFactory collection")]
    public class VocabulariesTests : IClassFixture<DwcaObservationFactoryFixture>
    {
        public VocabulariesTests(DwcaObservationFactoryFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly DwcaObservationFactoryFixture _fixture;

        [Fact]
        public void Null_value_is_not_mapped()
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

        [Fact]
        public void Value_that_doesnt_exist_in_vocabulary_is_mapped_to_NoMappingFoundCustomValueIsUsedId()
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
            result.Occurrence.Gender.Id.Should().Be(VocabularyConstants.NoMappingFoundCustomValueIsUsedId);
            result.Occurrence.Gender.Value.Should().Be("Malle");
        }

        [Fact]
        public void Value_that_exist_in_vocabulary_is_mapped()
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
            result.Occurrence.Gender.Id.Should().Be((int) GenderId.Male);
        }
    }
}