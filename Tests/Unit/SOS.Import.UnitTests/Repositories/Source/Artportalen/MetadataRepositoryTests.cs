using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Harvest.Entities.Artportalen;
using SOS.Harvest.Repositories.Source.Artportalen;
using SOS.Harvest.Services.Interfaces;
using SOS.Lib.Constants;
using SOS.Lib.Models.Shared;
using Xunit;

namespace SOS.Import.UnitTests.Repositories.Source.Artportalen
{
    /// <summary>
    ///     Meta data repository tests
    /// </summary>
    public class MetadataRepositoryTests
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public MetadataRepositoryTests()
        {
            _artportalenDataServiceMock = new Mock<IArtportalenDataService>();
            _loggerMock = new Mock<ILogger<MetadataRepository>>();
        }

        private readonly Mock<IArtportalenDataService> _artportalenDataServiceMock;
        private readonly Mock<ILogger<MetadataRepository>> _loggerMock;

        private MetadataRepository TestObject => new MetadataRepository(
            _artportalenDataServiceMock.Object,
            _loggerMock.Object);

        /// <summary>
        ///     Test get activities exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetActivitiesAsyncException()
        {
            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity<int>>(It.IsAny<string>(), null, false))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () =>
            {
                var result = await TestObject.GetActivitiesAsync();
            };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            await act.Should().ThrowAsync<Exception>();
        }

        /// <summary>
        ///     Test get activities success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetActivitiesAsyncSuccess()
        {
            IEnumerable<MetadataWithCategoryEntity<int>> activities = new[]
            {
                new MetadataWithCategoryEntity<int>
                {
                    Id = 1, CategoryId = 1, CategoryName = "Category", Translation = "Activity 1", CultureCode = "sv-GB"
                },
                new MetadataWithCategoryEntity<int>
                {
                    Id = 2, CategoryId = 1, CategoryName = "Category", Translation = "Activity 2", CultureCode = "sv-GB"
                }
            };

            _artportalenDataServiceMock
                .Setup(spds => spds.QueryAsync<MetadataWithCategoryEntity<int>>(It.IsAny<string>(), null, false))
                .ReturnsAsync(activities);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetActivitiesAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        ///     Test get activities exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetBiotopesAsyncException()
        {
            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity<int>>(It.IsAny<string>(), null, false))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () =>
            {
                var result = await TestObject.GetBiotopesAsync();
            };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            await act.Should().ThrowAsync<Exception>();
        }

        /// <summary>
        ///     Test get activities success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetBiotopessAsyncSuccess()
        {
            IEnumerable<MetadataEntity<int>> biotopes = new[]
            {
                new MetadataEntity<int> {Id = 1, Translation = "Biotope 1", CultureCode = Cultures.en_GB},
                new MetadataEntity<int> {Id = 2, Translation = "Biotope 2", CultureCode = Cultures.en_GB}
            };

            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity<int>>(It.IsAny<string>(), null, false))
                .ReturnsAsync(biotopes);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetBiotopesAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        ///     Test get genders exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetGendersAsyncException()
        {
            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity<int>>(It.IsAny<string>(), null, false))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () =>
            {
                var result = await TestObject.GetGendersAsync();
            };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            await act.Should().ThrowAsync<Exception>();
        }

        /// <summary>
        ///     Test get genders success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetGendersAsyncSuccess()
        {
            IEnumerable<MetadataEntity<int>> activities = new[]
            {
                new MetadataEntity<int> {Id = 1, Translation = "Gender 1", CultureCode = Cultures.en_GB},
                new MetadataEntity<int> {Id = 2, Translation = "Gender 2", CultureCode = Cultures.en_GB}
            };

            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity<int>>(It.IsAny<string>(), null, false))
                .ReturnsAsync(activities);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetGendersAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        ///     Test get stages exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetStagesAsyncException()
        {
            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity<int>>(It.IsAny<string>(), null, false))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () =>
            {
                var result = await TestObject.GetStagesAsync();
            };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            await act.Should().ThrowAsync<Exception>();
        }

        /// <summary>
        ///     Test get stages success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetStagesAsyncSuccess()
        {
            IEnumerable<MetadataEntity<int>> activities = new[]
            {
                new MetadataEntity<int> {Id = 1, Translation = "Stage 1", CultureCode = Cultures.en_GB},
                new MetadataEntity<int> {Id = 2, Translation = "Stage 2", CultureCode = Cultures.en_GB}
            };

            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity<int>>(It.IsAny<string>(), null, false))
                .ReturnsAsync(activities);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetStagesAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        ///     Test get activities exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetSubstratesAsyncException()
        {
            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity<int>>(It.IsAny<string>(), null, false))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () =>
            {
                var result = await TestObject.GetSubstratesAsync();
            };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            await act.Should().ThrowAsync<Exception>();
        }

        /// <summary>
        ///     Test get activities success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetSubstratesAsyncSuccess()
        {
            IEnumerable<MetadataEntity<int>> substrates = new[]
            {
                new MetadataEntity<int> {Id = 1, Translation = "Substrate 1", CultureCode = Cultures.en_GB},
                new MetadataEntity<int> {Id = 2, Translation = "Substrate 2", CultureCode = Cultures.en_GB}
            };

            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity<int>>(It.IsAny<string>(), null, false))
                .ReturnsAsync(substrates);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetSubstratesAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        ///     Test get units exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetUnitsAsyncException()
        {
            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity<int>>(It.IsAny<string>(), null, false))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () =>
            {
                var result = await TestObject.GetUnitsAsync();
            };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            await act.Should().ThrowAsync<Exception>();
        }

        /// <summary>
        ///     Test get units success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetUnitsAsyncSuccess()
        {
            IEnumerable<MetadataEntity<int>> activities = new[]
            {
                new MetadataEntity<int> {Id = 1, Translation = "Unit 1", CultureCode = Cultures.en_GB},
                new MetadataEntity<int> {Id = 2, Translation = "Unit 2", CultureCode = Cultures.en_GB}
            };

            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity<int>>(It.IsAny<string>(), null, false))
                .ReturnsAsync(activities);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetUnitsAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }

        /// <summary>
        ///     Test get units exception
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetValidationStatusAsyncException()
        {
            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity<int>>(It.IsAny<string>(), null, false))
                .Throws<Exception>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () =>
            {
                var result = await TestObject.GetValidationStatusAsync();
            };

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            await act.Should().ThrowAsync<Exception>();
        }

        /// <summary>
        ///     Test get units success
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetValidationStatusAsyncSuccess()
        {
            IEnumerable<MetadataEntity<int>> validationStatus = new[]
            {
                new MetadataEntity<int> {Id = 1, Translation = "ValidationStatus 1", CultureCode = Cultures.en_GB},
                new MetadataEntity<int> {Id = 2, Translation = "ValidationStatus 2", CultureCode = Cultures.en_GB}
            };

            _artportalenDataServiceMock.Setup(spds => spds.QueryAsync<MetadataEntity<int>>(It.IsAny<string>(), null, false))
                .ReturnsAsync(validationStatus);

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.GetValidationStatusAsync();
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().HaveCount(2);
        }
    }
}