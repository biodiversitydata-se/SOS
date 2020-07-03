using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DwC_A;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Import.DarwinCore;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Database;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Process.Helpers;
using SOS.Process.IntegrationTests.TestHelpers;
using SOS.Process.Repositories.Destination;
using Xunit;

namespace SOS.Process.IntegrationTests.Processors.DarwinCoreArchive
{
    public class DwcaCompareVerbatimAndProcessedObservationTests : TestBase,
        IClassFixture<DwcaObservationFactoryIntegrationFixture>
    {
        public DwcaCompareVerbatimAndProcessedObservationTests(DwcaObservationFactoryIntegrationFixture fixture)
        {
            _fixture = fixture;
        }

        private readonly DwcaObservationFactoryIntegrationFixture _fixture;

        private FieldMappingResolverHelper CreateFieldMappingResolverHelper()
        {
            var processDbConfiguration = GetProcessDbConfiguration();
            var processClient = new ProcessClient(
                processDbConfiguration.GetMongoDbSettings(),
                processDbConfiguration.DatabaseName,
                processDbConfiguration.ReadBatchSize,
                processDbConfiguration.WriteBatchSize);

            var processedFieldMappingRepository =
                new ProcessedFieldMappingRepository(processClient, new NullLogger<ProcessedFieldMappingRepository>());
            return new FieldMappingResolverHelper(processedFieldMappingRepository,
                new FieldMappingConfiguration {LocalizationCultureCode = "sv-SE", ResolveValues = true});
        }

        public class CompareObservation
        {
            public object VerbatimObservation { get; set; }
            public object ProcessedObservation { get; set; }
        }

        [Fact]
        public async Task Process_SHARK_observations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const int nrObservationsToSave = 10;
            const string archivePath = "./resources/dwca/SHARK_Zooplankton_NAT_DwC-A.zip";
            const string savePath = @"c:\temp\SharkObservationsCompare.json";
            if (File.Exists(savePath)) File.Delete(savePath);
            var dataProviderIdIdentifierTuple = new IdIdentifierTuple
            {
                Id = 101,
                Identifier = "TestSHARK"
            };
            var fieldMappingResolverHelper = CreateFieldMappingResolverHelper();
            var dwcaReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
            using var archiveReader = new ArchiveReader(archivePath);
            var observations = await dwcaReader.ReadArchiveAsync(archiveReader, dataProviderIdIdentifierTuple);
            var observationComparisions = new List<CompareObservation>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            foreach (var verbatimObservation in observations.Take(nrObservationsToSave))
            {
                var processedObservation =
                    _fixture.DwcaObservationFactory.CreateProcessedObservation(verbatimObservation);
                fieldMappingResolverHelper.ResolveFieldMappedValues(new List<ProcessedObservation>
                    {processedObservation});
                observationComparisions.Add(new CompareObservation
                {
                    VerbatimObservation = verbatimObservation,
                    ProcessedObservation = processedObservation
                });
            }

            var strJsonObservationCompare = JsonHelper.SerializeToMinimalJson(observationComparisions);
            await File.WriteAllTextAsync(savePath, strJsonObservationCompare);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            strJsonObservationCompare.Should().NotBeEmpty();
        }
    }
}