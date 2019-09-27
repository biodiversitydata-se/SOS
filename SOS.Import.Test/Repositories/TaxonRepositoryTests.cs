using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Entities;
using SOS.Import.Repositories.Source.SpeciesPortal;
using SOS.Import.Services.Interfaces;
using Xunit;

namespace SOS.Import.Test.Repositories
{
    /// <summary>
    /// Tests for taxon repository
    /// </summary>
    public class TaxonRepositoryTests
    {
        private readonly Mock<ISpeciesPortalDataService> _speciesPortalDataServiceMock;
        private readonly Mock<ILogger<TaxonRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public TaxonRepositoryTests()
        {
            _speciesPortalDataServiceMock = new Mock<ISpeciesPortalDataService>();
            _loggerMock = new Mock<ILogger<TaxonRepository>>();
        }

        /// <summary>
        /// Constructor test
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new TaxonRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new TaxonRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("speciesPortalDataService");

            create = () => new TaxonRepository(
                _speciesPortalDataServiceMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Get taxa test success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAsyncSuccess()
        {
            IEnumerable<TaxonEntity> projects = new []
            {
                    new TaxonEntity { Id = 1, SwedishName = "Taxon 1" },
                    new TaxonEntity { Id = 2, SwedishName = "Taxon 2" }
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<TaxonEntity>(It.IsAny<string>(), null))
                .ReturnsAsync(projects);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var TaxonRepository = new TaxonRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await TaxonRepository.GetAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        /// Test get taxa fail
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetAsyncException()
        {
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<TaxonEntity>(It.IsAny<string>(), null))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var TaxonRepository = new TaxonRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await TaxonRepository.GetAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }
    }
}
