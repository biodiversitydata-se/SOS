﻿using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SOS.Process.Database.Interfaces;
using SOS.Process.Repositories.Destination;
using Xunit;

namespace SOS.Process.UnitTests.Repositories.Destination
{
    public class ProcessedTaxonRepositoryTests
    {
        private readonly Mock<IProcessClient> _processClient;
        private readonly Mock<ILogger<ProcessedTaxonRepository>> _loggerMock;

        /// <summary>
        /// Constructor
        /// </summary>
        public ProcessedTaxonRepositoryTests()
        {
            _processClient = new Mock<IProcessClient>();
            _loggerMock = new Mock<ILogger<ProcessedTaxonRepository>>();
        }

        /// <summary>
        /// Test constructor
        /// </summary>
        [Fact]
        public void ConstructorTest()
        {
            Action create = () => new ProcessedTaxonRepository(
                _processClient.Object,
                _loggerMock.Object);
            create.Should().Throw<NullReferenceException>();

           create = () => new ProcessedTaxonRepository(
                null,
                _loggerMock.Object);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("client");

            create = () => new ProcessedTaxonRepository(
                _processClient.Object,
                null);
            create.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logger");
        }
    }
}
