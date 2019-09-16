using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Batch.Import.AP.Entities;
using SOS.Batch.Import.AP.Repositories.Source;
using SOS.Batch.Import.AP.Services.Interfaces;
using Xunit;

namespace SOS.Batch.Import.AP.Test.Repositories
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
            IEnumerable<MetadataEntity> activities = new []
            {
                    new MetadataEntity { Id = 1, Name = "Activity 1" },
                    new MetadataEntity { Id = 2, Name = "Activity 2" }
            };

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>()))
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
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>()))
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

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>()))
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
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>()))
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

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>()))
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
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>()))
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

            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>()))
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
            _speciesPortalDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity>(It.IsAny<string>()))
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
        #endregion Activities
    }
}
