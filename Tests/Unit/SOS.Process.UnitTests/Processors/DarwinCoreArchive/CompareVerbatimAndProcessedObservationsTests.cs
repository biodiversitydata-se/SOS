using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Process.Helpers;
using SOS.Process.UnitTests.TestHelpers;
using SOS.Process.UnitTests.TestHelpers.Factories;
using SOS.TestHelpers.Helpers.Builders;
using Xunit;

namespace SOS.Process.UnitTests.Processors.DarwinCoreArchive
{
    [CollectionDefinition("DwcaObservationFactory collection")]
    public class CompareVerbatimAndProcessedObservationsTests : IClassFixture<DwcaObservationFactoryFixture>
    {
        public CompareVerbatimAndProcessedObservationsTests(DwcaObservationFactoryFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly DwcaObservationFactoryFixture _fixture;

        public class CompareObservation
        {
            public object VerbatimObservation { get; set; }
            public object ProcessedObservation { get; set; }
        }

        [Fact]
        public void Sex_with_Male_value_is_field_mapped()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var processedFieldMappingRepositoryStub = ProcessedFieldMappingRepositoryStubFactory.Create();
            var fieldMappingResolverHelper = new FieldMappingResolverHelper(processedFieldMappingRepositoryStub.Object,
                new FieldMappingConfiguration {LocalizationCultureCode = "sv-SE", ResolveValues = true});
            var builder = new DwcObservationVerbatimBuilder();
            var dwcaObservation = builder
                .WithDefaultValues()
                .WithSex("Male")
                .Build();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var processedObservation = _fixture.DwcaObservationFactory.CreateProcessedObservation(dwcaObservation);
            fieldMappingResolverHelper.ResolveFieldMappedValues(new List<ProcessedObservation> {processedObservation});
            var compareResult = new CompareObservation
            {
                VerbatimObservation = dwcaObservation,
                ProcessedObservation = processedObservation
            };
            var strJsonObservationCompare = JsonConvert.SerializeObject(
                compareResult,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            strJsonObservationCompare.Should().NotBeEmpty();
        }
    }
}