﻿using FluentAssertions;
using SOS.Lib.Enums.VocabularyValues;
using SOS.Process.UnitTests.TestHelpers;
using SOS.TestHelpers.Helpers.Builders;
using System;
using Xunit;

namespace SOS.Process.UnitTests.Processors.DarwinCoreArchive.DwcaObservationFactory
{
    [CollectionDefinition("DwcaObservationFactory collection")]
    public class IdentificationTests : IClassFixture<DwcaObservationFactoryFixture>
    {
        public IdentificationTests(DwcaObservationFactoryFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly DwcaObservationFactoryFixture _fixture;

        [Fact]
        public void DateIdentified_is_parsed_to_DateTime_type()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithDateIdentified("2019-05-29")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Identification.DateIdentified.Should().Be(new DateTime(2019, 5, 29).ToUniversalTime().ToString());
        }

        [Fact]
        public void DateIdentified_specified_as_DateTime_type_is_parsed_to_same_DateTime()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var date = new DateTime(2019, 5, 29, 0, 0, 0, DateTimeKind.Utc);
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithDateIdentified(date)
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Identification.DateIdentified.Should().Be(date.ToString());
        }

        [Fact]
        public void DataProvider_with_ReportedAsExpert_as_default()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcaObservationFactory = _fixture.CreateDwcaObservationFactory(ValidationStatusId.ReportedByExpert);
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = dwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Identification.VerificationStatus.Id.Should().Be((int)ValidationStatusId.ReportedByExpert);
            result.Identification.Verified.Should().Be(true);
        }

        [Theory]
        [InlineData("verified", ValidationStatusId.Verified, true)]
        [InlineData("unverified", ValidationStatusId.Unvalidated, false)]
        public void IdentificationVerificationStatus_field_with_valid_value_is_mapped_to_ValidationStatus_vocabulary(
            string identificationVerificationStatusValue,
            ValidationStatusId expectedValidationStatusId,
            bool expectedValidatedValue)
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithIdentificationVerificationStatus(identificationVerificationStatusValue)
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation, true);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            /* result.Identification.ValidationStatus.Id.Should().Be((int)expectedValidationStatusId);
             result.Identification.Validated.Should().Be(expectedValidatedValue);*/
            result.Identification.VerificationStatus.Id.Should().Be((int)expectedValidationStatusId);
            result.Identification.Verified.Should().Be(expectedValidatedValue);
        }
    }
}