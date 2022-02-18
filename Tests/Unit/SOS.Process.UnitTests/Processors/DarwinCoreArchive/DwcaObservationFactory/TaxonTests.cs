using System.Threading.Tasks;
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
        private readonly DwcaObservationFactoryFixture _fixture;

        public TaxonTests(DwcaObservationFactoryFixture fixture)
        {
            _fixture = fixture;
        }

        /// <remarks>
        /// Only Mammalia taxon and its underlying taxa is available in this unit test to keep the execution time fast.
        /// </remarks>
        [Theory]        
        [InlineData(null, "urn:lsid:dyntaxa.se:Taxon:233622", 233622)] // find integer in guid
        [InlineData("equus asinus", null, 233622)] // find by scientific name
        //[InlineData("Felis lynx", null, 100057)] // find by synonyme (synonyms aren't yet included in test data)
        public async Task Succeeds_to_parse_taxon_from_taxonId_and_scientific_name(
            string scientificName,
            string taxonId,
            int expectedParsedTaxonId)
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
            var observation = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);
            int? parsedTaxonId = observation.Taxon?.Id;

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            parsedTaxonId.Should().Be(expectedParsedTaxonId);
        }

        /// <remarks>
        /// Only Mammalia taxon and its underlying taxa is available in this unit test to keep the execution time fast.
        /// </remarks>
        [Theory]
        [InlineData(null, null)] // all null values
        [InlineData(null, "TaxonId_text_without_taxon_id")] // text with no id
        [InlineData(null, "TaxonId_text_with_id_that_doesnt_exist_9999999")] // taxon id that doesn't exist
        [InlineData(null, "TaxonId_text_with_id_that_is_larger_than_max_integer_9999999000000000000")] // integer larger than int.max
        public void Fails_to_parse_taxon_from_taxonId_and_scientific_name(
            string scientificName,
            string taxonId)
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
            var observation = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observation.Taxon.Id.Should().Be(-1, "because -1 means that the taxon was not found");
        }
    }
}