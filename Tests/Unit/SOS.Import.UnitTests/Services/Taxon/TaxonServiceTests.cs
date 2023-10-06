using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SOS.Harvest.Services.Taxon;
using SOS.Harvest.Services.Taxon.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Factories;
using System.Linq;
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

        [Fact(Skip = "Intended to run on demand")]
        public void CompareDyntaxaDwcaFiles()
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
            var dwcTaxa1 = taxonService.GetTaxaFromDwcaFile(@"C:\Temp\2023-10-04\dyntaxa-custom-dwca.zip2023-09-30T05_44_22.zip");
            var taxa1 = dwcTaxa1.ToProcessedTaxa();
            var taxonTree1 = TaxonTreeFactory.CreateTaxonTree(taxa1);

            var dwcTaxa2 = taxonService.GetTaxaFromDwcaFile(@"C:\Temp\2023-10-04\dyntaxa-custom-dwca.zip2023-10-04T05_45_54.zip");
            System.Collections.Generic.IDictionary<int, Lib.Models.Processed.Observation.Taxon> taxa2 = dwcTaxa2.ToProcessedTaxa();
            var taxonTree2 = TaxonTreeFactory.CreateTaxonTree(taxa2);

            var additionalTaxaInWorkingFile = taxa2.Values.ExceptBy(taxa1.Values.Select(m => m.Id), m => m.Id);
            var summary = additionalTaxaInWorkingFile.Select(m => new TaxonSummary(m));

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            taxonTree1.Should().NotBeNull();
        }

        public class TaxonSummary
        {
            public Lib.Models.Processed.Observation.Taxon Taxon { get; set; }

            public TaxonSummary(Lib.Models.Processed.Observation.Taxon taxon)
            {
                Taxon = taxon;
            }

            public override string ToString()
            {
                return $"Category={Taxon.TaxonRank}, Name={Taxon.VernacularName} ";
            }
        }
    }
}