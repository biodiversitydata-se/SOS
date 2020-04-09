using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Harvesters;
using SOS.Import.Models.TaxonAttributeService;
using SOS.Import.Repositories.Destination.Taxon.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.DarwinCore;
using Xunit;

namespace SOS.Import.UnitTests.Harvesters
{
    public class TaxonHarvesterTests
    {
        private readonly Mock<ITaxonVerbatimRepository> _taxonVerbatimRepositoryMock;
        private readonly Mock<ITaxonService> _taxonServiceMock;
        private readonly Mock<ITaxonAttributeService> _taxonAttributeServiceMock;
        private readonly Mock<ILogger<TaxonHarvester>> _loggerMock;

        private TaxonHarvester TestObject => new TaxonHarvester(
            _taxonVerbatimRepositoryMock.Object,
            _taxonServiceMock.Object,
            _taxonAttributeServiceMock.Object,
            _loggerMock.Object);

        /// <summary>
        /// Constructor
        /// </summary>
        public TaxonHarvesterTests()
        {
            _taxonVerbatimRepositoryMock = new Mock<ITaxonVerbatimRepository>();
            _taxonServiceMock = new Mock<ITaxonService>();
            _taxonAttributeServiceMock = new Mock<ITaxonAttributeService>();
            _loggerMock = new Mock<ILogger<TaxonHarvester>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new TaxonHarvester(
                null,
                _taxonServiceMock.Object,
                _taxonAttributeServiceMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("taxonVerbatimRepository");

            create = () => new TaxonHarvester(
                _taxonVerbatimRepositoryMock.Object,
                null,
                _taxonAttributeServiceMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("taxonService");

            create = () => new TaxonHarvester(
                _taxonVerbatimRepositoryMock.Object,
                _taxonServiceMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("taxonAttributeService");

           
            create = () => new TaxonHarvester(
                _taxonVerbatimRepositoryMock.Object,
                _taxonServiceMock.Object,
                _taxonAttributeServiceMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Make a successful clams harvest
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HarvestTaxaAsyncSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _taxonServiceMock.Setup(ts => ts.GetTaxaAsync())
                .ReturnsAsync(new[] { new DarwinCoreTaxon
                {
                    TaxonID = "urn:taxon:100024"
                }  });

            _taxonAttributeServiceMock.Setup(tas => tas.GetCurrentRedlistPeriodIdAsync())
                .ReturnsAsync(1);

            _taxonAttributeServiceMock.Setup(tas => tas.GetTaxonAttributesAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<int>>(), It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<TaxonAttributeModel>());

            _taxonVerbatimRepositoryMock.Setup(tr => tr.DeleteCollectionAsync())
                .ReturnsAsync(true);
            _taxonVerbatimRepositoryMock.Setup(tr => tr.AddCollectionAsync())
                .ReturnsAsync(true);
            _taxonVerbatimRepositoryMock.Setup(tr => tr.AddManyAsync(It.IsAny<IEnumerable<DarwinCoreTaxon>>()))
                .ReturnsAsync(true);


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.HarvestAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Status.Should().Be(RunStatus.Success);
        }

        /// <summary>
        /// Test aggregation fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HarvestClamsAsyncFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _taxonServiceMock.Setup(ts => ts.GetTaxaAsync())
                .ThrowsAsync(new Exception("Fail"));


            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.HarvestAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Status.Should().Be(RunStatus.Failed);
        }
    }
}
