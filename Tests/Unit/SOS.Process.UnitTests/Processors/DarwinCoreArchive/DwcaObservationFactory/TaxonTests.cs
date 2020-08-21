using FluentAssertions;
using SOS.Process.UnitTests.TestHelpers;
using SOS.TestHelpers.Helpers.Builders;
using SOS.TestHelpers.Taxonomy;
using Xunit;

namespace SOS.Process.UnitTests.Processors.DarwinCoreArchive.DwcaObservationFactory
{
    [CollectionDefinition("DwcaObservationFactory collection")]
    public class TaxonTests : IClassFixture<DwcaObservationFactoryFixture>
    {
        public TaxonTests(DwcaObservationFactoryFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly DwcaObservationFactoryFixture _fixture;

        [Fact]
        public void Taxon_with_lowercase_Scientific_Name_equus_asinus_is_parsed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithScientificName("equus asinus")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Taxon.DyntaxaTaxonId.Should().Be(Taxon.EquusAsinus.DyntaxaTaxonId);
        }

        [Fact]
        public void Taxon_with_null_as_Scientific_Name_cant_find_DyntaxaTaxonId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithScientificName(null)
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Taxon.DyntaxaTaxonId.Should().Be(default);
        }

        [Fact]
        public void Taxon_with_Scientific_Name_Equus_asinus_is_parsed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithScientificName("Equus asinus")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Taxon.DyntaxaTaxonId.Should().Be(Taxon.EquusAsinus.DyntaxaTaxonId);
        }

        [Fact]
        public void Taxon_with_synonyme_name_Trientalis_europaea_is_parsed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithScientificName("Didelphis vulpecula")
                //.WithScientificName("Trientalis europaea")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);
            
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Taxon.DyntaxaTaxonId.Should().Be(Taxon.TrientalisEuropaea.DyntaxaTaxonId);
        }

        [Fact]
        public void Taxon_with_taxon_id_233622_is_parsed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithTaxonId("233622")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Taxon.DyntaxaTaxonId.Should().Be(Taxon.EquusAsinus.DyntaxaTaxonId);
        }

        [Fact]
        public void Taxon_with_taxon_id_lsid_233622_is_parsed()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithTaxonId("urn:lsid:dyntaxa.se:Taxon:233622")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Taxon.DyntaxaTaxonId.Should().Be(Taxon.EquusAsinus.DyntaxaTaxonId);
        }

        [Fact]
        public void Taxon_with_invalid_taxon_id_is_parsed_to_null()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithTaxonId("TextTextTextText")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Taxon.Should().BeNull();
        }

        [Fact]
        public void Taxon_with_taxon_id_set_to_null_is_parsed_to_null()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithTaxonId(null)
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Taxon.Should().BeNull();
        }


        /// <remarks>
        /// Only Mammalia taxon and its underlying taxa is available in this unit test to keep the execution time fast.
        /// </remarks>
        [Theory]
        [InlineData(null, null, null)]
        [InlineData(null, "233622", 233622)]
        [InlineData(null, "urn:lsid:dyntaxa.se:Taxon:233622", 233622)]
        [InlineData(null, "TaxonId_text_without_taxon_id", null)]
        [InlineData(null, "TaxonId_text_with_id_that_doesnt_exist_9999999", null)]
        [InlineData(null, "TaxonId_text_with_id_that_is_larger_than_max_integer_9999999000000000000", null)]
        public void Parses_taxon_from_taxonId_and_scientific_name(
            string scientificName,
            string taxonId,
            int? expectedParsedTaxonId)
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithScientificName(scientificName)
                .WithTaxonId(taxonId)
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var obs = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);
            int? parsedTaxonId = obs.Taxon?.Id;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            parsedTaxonId.Should().Be(expectedParsedTaxonId);
        }
    }
}