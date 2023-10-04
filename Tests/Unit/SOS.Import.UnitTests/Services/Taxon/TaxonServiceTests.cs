using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Services.Taxon;
using SOS.Harvest.Services.Taxon.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Factories;
using Xunit;

namespace SOS.Import.UnitTests.Services.Taxon
{
    public class TaxonServiceTests
    {                
        [Fact(Skip = "Intended to run on demand")]
        public void GetTaxonTreeFromDwcaFile()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var taxonService = new TaxonService(new Mock<ITaxonServiceProxy>().Object,
                new Lib.Configuration.Process.TaxonServiceConfiguration() { BaseAddress = "" },
                new NullLogger<TaxonService>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var dwcTaxa = taxonService.GetTaxaFromDwcaFile(@"C:\Temp\2023-10-04\dyntaxa-custom-dwca.zip2023-10-04T05_45_54.zip");
            var taxa = dwcTaxa.ToProcessedTaxa();
            var taxonTree = TaxonTreeFactory.CreateTaxonTree(taxa);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            taxonTree.Should().NotBeNull();
        }
    }
}