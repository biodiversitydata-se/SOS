using System.Threading.Tasks;
using FluentAssertions;
using SOS.Lib.Constants;
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
        public async Task Null_value_is_not_mapped()
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
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Asserts
            //-----------------------------------------------------------------------------------------------------------
            result.Occurrence.Sex.Id.Should().Be(VocabularyConstants.NoMappingFoundCustomValueIsUsedId);
            result.Occurrence.Sex.Value.Should().Be("Malle");
        }
    }
}