using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Factories;
using SOS.Import.Models.TaxonAttributeService;
using SOS.Import.Repositories.Destination.Taxon.Interfaces;
using SOS.Import.Services.Interfaces;
using SOS.Lib.Enums;
using SOS.Lib.Models.Processed.DarwinCore;
using Xunit;

namespace SOS.Import.UnitTests.Factories
{
    public class TaxonFactoryTests
    {
        private readonly Mock<ITaxonVerbatimRepository> _taxonVerbatimRepositoryMock;
        private readonly Mock<ITaxonService> _taxonServiceMock;
        private readonly Mock<ITaxonAttributeService> _taxonAttributeServiceMock;
        private readonly Mock<ILogger<TaxonFactory>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public TaxonFactoryTests()
        {
            _taxonVerbatimRepositoryMock = new Mock<ITaxonVerbatimRepository>();
            _taxonServiceMock = new Mock<ITaxonService>();
            _taxonAttributeServiceMock = new Mock<ITaxonAttributeService>();
            _loggerMock = new Mock<ILogger<TaxonFactory>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new TaxonFactory(
                _taxonVerbatimRepositoryMock.Object,
                _taxonServiceMock.Object,
                _taxonAttributeServiceMock.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new TaxonFactory(
                null,
                _taxonServiceMock.Object,
                _taxonAttributeServiceMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("taxonVerbatimRepository");

            create = () => new TaxonFactory(
                _taxonVerbatimRepositoryMock.Object,
                null,
                _taxonAttributeServiceMock.Object,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("taxonService");

            create = () => new TaxonFactory(
                _taxonVerbatimRepositoryMock.Object,
                _taxonServiceMock.Object,
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("taxonAttributeService");

           
            create = () => new TaxonFactory(
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
            var taxonFactory = new TaxonFactory(
                _taxonVerbatimRepositoryMock.Object,
                _taxonServiceMock.Object,
                _taxonAttributeServiceMock.Object,
                _loggerMock.Object);

            var result = await taxonFactory.HarvestAsync();
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
            var taxonFactory = new TaxonFactory(
                _taxonVerbatimRepositoryMock.Object,
                _taxonServiceMock.Object,
                _taxonAttributeServiceMock.Object,
                _loggerMock.Object);

            var result = await taxonFactory.HarvestAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Status.Should().Be(RunStatus.Failed);
        }
    }
}
