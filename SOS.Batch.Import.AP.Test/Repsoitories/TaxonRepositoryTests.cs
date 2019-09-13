using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Batch.Import.AP.Entities;
using SOS.Batch.Import.AP.Repositories.Source;
using SOS.Batch.Import.AP.Services.Interfaces;
using Xunit;

namespace SOS.Batch.Import.AP.Test
{
    public class TaxonRepositoryTests
    {
        private Mock<IDbConnection> _connection;
        private Mock<ISpeciesPortalDataService> _speciesPortalDataServiceMock;
        private Mock<ILogger<TaxonRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public TaxonRepositoryTests()
        {
            _connection = new Mock<IDbConnection>();
            _speciesPortalDataServiceMock = new Mock<ISpeciesPortalDataService>();
            _loggerMock = new Mock<ILogger<TaxonRepository>>();
        }

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

        [Fact]
        public async Task GetAsyncSuccess()
        {
            IEnumerable<TaxonEntity> projects = new []
            {
                    new TaxonEntity { Id = 1, SwedishName = "Taxon 1" },
                    new TaxonEntity { Id = 2, SwedishName = "Taxon 2" }
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<TaxonEntity>(It.IsAny<string>()))
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

        [Fact]
        public async Task GetAsyncException()
        {
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<TaxonEntity>(It.IsAny<string>()))
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
