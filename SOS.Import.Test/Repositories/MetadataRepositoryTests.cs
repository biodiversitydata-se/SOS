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
    /// Meta data repository tests
    /// </summary>
    public class MetadataRepositoryTests
    {
        private readonly Mock<ISpeciesPortalDataService> _speciesPortalDataServiceMock;
        private readonly Mock<ILogger<MetadataRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public MetadataRepositoryTests()
        {
            _speciesPortalDataServiceMock = new Mock<ISpeciesPortalDataService>();
            _loggerMock = new Mock<ILogger<MetadataRepository>>();
        }

        /// <summary>
        /// Test the constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            new MetadataRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object).Should().NotBeNull();

            Action create = () => new MetadataRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("speciesPortalDataService");

            create = () => new MetadataRepository(
                _speciesPortalDataServiceMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        #region Activities
        /// <summary>
        /// Test get activities success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetActivitiesAsyncSuccess()
        {
            IEnumerable<MetadataWithCategoryEntity> activities = new []
            {
                    new MetadataWithCategoryEntity { Id = 1, Name = "Activity 1", CategoryId = 1, CategoryName = "Category" },
                    new MetadataWithCategoryEntity { Id = 2, Name = "Activity 2", CategoryId = 1, CategoryName = "Category" }
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataWithCategoryEntity>(It.IsAny<string>(), null))
                .ReturnsAsync(activities);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var metadataRepository = new MetadataRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await metadataRepository.GetActivitiesAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        /// Test get activities exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetActivitiesAsyncException()
        {
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>(), null))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var metadataRepository = new MetadataRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await metadataRepository.GetActivitiesAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }
        #endregion Activities

        #region Biotopes
        /// <summary>
        /// Test get activities success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetBiotopessAsyncSuccess()
        {
            IEnumerable<MetadataEntity> biotopes = new[]
            {
                    new MetadataEntity { Id = 1, Name = "Biotope 1" },
                    new MetadataEntity { Id = 2, Name = "Biotope 2" }
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>(), null))
                .ReturnsAsync(biotopes);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var metadataRepository = new MetadataRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await metadataRepository.GetBiotopesAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        /// Test get activities exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetBiotopesAsyncException()
        {
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>(), null))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var metadataRepository = new MetadataRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await metadataRepository.GetBiotopesAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }
        #endregion Biotopes

        #region Genders
        /// <summary>
        /// Test get genders success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetGendersAsyncSuccess()
        {
            IEnumerable<MetadataEntity> activities = new[]
            {
                    new MetadataEntity { Id = 1, Name = "Gender 1" },
                    new MetadataEntity { Id = 2, Name = "Gender 2" }
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>(), null))
                .ReturnsAsync(activities);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var metadataRepository = new MetadataRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await metadataRepository.GetGendersAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        /// Test get genders exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetGendersAsyncException()
        {
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>(), null))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var metadataRepository = new MetadataRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await metadataRepository.GetGendersAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }
        #endregion Activities

        #region Stages
        /// <summary>
        /// Test get stages success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetStagesAsyncSuccess()
        {
            IEnumerable<MetadataEntity> activities = new[]
            {
                    new MetadataEntity { Id = 1, Name = "Stage 1" },
                    new MetadataEntity { Id = 2, Name = "Stage 2" }
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>(), null))
                .ReturnsAsync(activities);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var metadataRepository = new MetadataRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await metadataRepository.GetStagesAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        /// Test get stages exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetStagesAsyncException()
        {
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>(), null))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var metadataRepository = new MetadataRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await metadataRepository.GetStagesAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }
        #endregion Activities

        #region Substrates
        /// <summary>
        /// Test get activities success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetSubstratesAsyncSuccess()
        {
            IEnumerable<MetadataEntity> substrates = new[]
            {
                    new MetadataEntity { Id = 1, Name = "Substrate 1" },
                    new MetadataEntity { Id = 2, Name = "Substrate 2" }
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>(), null))
                .ReturnsAsync(substrates);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var metadataRepository = new MetadataRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await metadataRepository.GetSubstratesAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        /// Test get activities exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetSubstratesAsyncException()
        {
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>(), null))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var metadataRepository = new MetadataRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await metadataRepository.GetSubstratesAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }
        #endregion Substrates

        #region Units
        /// <summary>
        /// Test get units success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetUnitsAsyncSuccess()
        {
            IEnumerable<MetadataEntity> activities = new[]
            {
                    new MetadataEntity { Id = 1, Name = "Unit 1" },
                    new MetadataEntity { Id = 2, Name = "Unit 2" }
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>(), null))
                .ReturnsAsync(activities);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var metadataRepository = new MetadataRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await metadataRepository.GetUnitsAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        /// Test get units exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetUnitsAsyncException()
        {
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>(), null))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var metadataRepository = new MetadataRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await metadataRepository.GetUnitsAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }
        #endregion Units

        #region ValidationStatus
        /// <summary>
        /// Test get units success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetValidationStatusAsyncSuccess()
        {
            IEnumerable<MetadataEntity> validationStatus = new[]
            {
                    new MetadataEntity { Id = 1, Name = "ValidationStatus 1" },
                    new MetadataEntity { Id = 2, Name = "ValidationStatus 2" }
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>(), null))
                .ReturnsAsync(validationStatus);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var metadataRepository = new MetadataRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await metadataRepository.GetValidationStatusAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        /// Test get units exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetValidationStatusAsyncException()
        {
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>(), null))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var metadataRepository = new MetadataRepository(
                _speciesPortalDataServiceMock.Object,
                _loggerMock.Object);

            var result = await metadataRepository.GetValidationStatusAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeNull();
        }
        #endregion ValidationStatus
    }
}
