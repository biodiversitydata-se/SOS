using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Moq;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.FieldMappingValues;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Processors.DarwinCoreArchive;
using SOS.Process.UnitTests.TestHelpers;
using SOS.Process.UnitTests.TestHelpers.Factories;
using SOS.TestHelpers.Gis;
using SOS.TestHelpers.Helpers.Builders;
using SOS.TestHelpers.Taxonomy;
using Xunit;

namespace SOS.Process.UnitTests.Processors.DarwinCoreArchive.DwcaObservationFactory
{
    [CollectionDefinition("DwcaObservationFactory collection")]
    public class IdentificationTests : IClassFixture<DwcaObservationFactoryFixture>
    {
        private readonly DwcaObservationFactoryFixture _fixture;

        public IdentificationTests(DwcaObservationFactoryFixture fixture)
        {
            _fixture = fixture;
        }

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
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Identification.DateIdentified.Should().Be(new DateTime(2019,5,29));
        }

        [Fact]
        public void DateIdentified_specified_as_DateTime_type_is_parsed_to_same_DateTime()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var date = new DateTime(2019,5,29);
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithDateIdentified(date)
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Identification.DateIdentified.Should().Be(date);
        }

        [Fact]
        public void IdentificationVerificationStatus_with_unverified_value_is_fieldmapped_to_Unvalidated_statusId()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithIdentificationVerificationStatus("unverified")
                .Build();
            
            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);
            
            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            result.Identification.ValidationStatus.Id.Should().Be((int) ValidationStatusId.Unvalidated);
        }
    }
}