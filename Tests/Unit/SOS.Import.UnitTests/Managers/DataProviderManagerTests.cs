﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Import.Managers;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Models.Verbatim.Shared;
using Xunit;

namespace SOS.Import.UnitTests.Managers
{
    public class DataProviderManagerTests
    {
        private readonly Mock<IDataProviderRepostitory> _dataProviderRepositoryMock;
        private readonly Mock<ILogger<DataProviderManager>> _loggerMock;

        private DataProviderManager TestObject => new DataProviderManager(
            _dataProviderRepositoryMock.Object,
            _loggerMock.Object);

        /// <summary>
        /// Constructor
        /// </summary>
        public DataProviderManagerTests()
        {
            _dataProviderRepositoryMock = new Mock<IDataProviderRepostitory>();
            _loggerMock = new Mock<ILogger<DataProviderManager>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            TestObject.Should().NotBeNull();

            Action create = () => new DataProviderManager(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("dataProviderRepository");

            create = () => new DataProviderManager(
                _dataProviderRepositoryMock.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }

        /// <summary>
        /// Add a provider successfully
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddDataProviderSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _dataProviderRepositoryMock.Setup(ts => ts.AddOrUpdateAsync(It.IsAny<DataProvider>()))
                .ReturnsAsync(true);
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.AddDataProvider(It.IsAny<DataProvider>());
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }

        /// <summary>
        /// Fail to a add provider
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddDataProviderFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _dataProviderRepositoryMock.Setup(ts => ts.AddOrUpdateAsync(It.IsAny<DataProvider>()))
                .ReturnsAsync(false);
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.AddDataProvider(It.IsAny<DataProvider>());
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }

        /// <summary>
        /// Exception occur when a provider is added
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task AddDataProviderException()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _dataProviderRepositoryMock.Setup(ts => ts.AddOrUpdateAsync(It.IsAny<DataProvider>()))
                .Throws<Exception>();
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () => { await TestObject.AddDataProvider(It.IsAny<DataProvider>()); };
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            await act.Should().ThrowAsync<Exception>();
        }

        /// <summary>
        /// Delete a provider successfully
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DeleteDataProviderSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _dataProviderRepositoryMock.Setup(ts => ts.DeleteAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.DeleteDataProvider(It.IsAny<int>());
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }

        /// <summary>
        /// Fail to a delete provider
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DeleteDataProviderFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _dataProviderRepositoryMock.Setup(ts => ts.DeleteAsync(It.IsAny<int>()))
                .ReturnsAsync(false);
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.DeleteDataProvider(It.IsAny<int>());
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }

        /// <summary>
        /// Exception occur when a provider is deleted
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DeleteDataProviderException()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _dataProviderRepositoryMock.Setup(ts => ts.DeleteAsync(It.IsAny<int>()))
                .Throws<Exception>();
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () => { await TestObject.DeleteDataProvider(It.IsAny<int>()); };
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            await act.Should().ThrowAsync<Exception>();
        }

        /// <summary>
        /// Update a provider successfully
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateDataProviderSuccess()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _dataProviderRepositoryMock.Setup(ts => ts.UpdateAsync(It.IsAny<int>(), It.IsAny<DataProvider>()))
                .ReturnsAsync(true);
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.UpdateDataProvider(It.IsAny<int>(), It.IsAny<DataProvider>());
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeTrue();
        }

        /// <summary>
        /// Fail to a update provider
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateDataProviderFail()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _dataProviderRepositoryMock.Setup(ts => ts.UpdateAsync(It.IsAny<int>(), It.IsAny<DataProvider>()))
                .ReturnsAsync(false);
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await TestObject.UpdateDataProvider(It.IsAny<int>(), It.IsAny<DataProvider>());
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            result.Should().BeFalse();
        }

        /// <summary>
        /// Exception occur when a provider is updated
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateDataProviderException()
        {
            // -----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            _dataProviderRepositoryMock.Setup(ts => ts.UpdateAsync(It.IsAny<int>(), It.IsAny<DataProvider>()))
                .Throws<Exception>();
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            Func<Task> act = async () => { await TestObject.UpdateDataProvider(It.IsAny<int>(), It.IsAny<DataProvider>()); };
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------

            await act.Should().ThrowAsync<Exception>();
        }
    }
}
