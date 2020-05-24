using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DwC_A;
using Elasticsearch.Net;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging.Abstractions;
using MongoDB.Driver;
using Moq;
using Nest;
using Newtonsoft.Json;
using SOS.Import.DarwinCore;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Process.Database;
using SOS.Process.Helpers;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.IntegrationTests.TestHelpers;
using SOS.Process.Processors.DarwinCoreArchive;
using SOS.Process.Repositories.Destination;
using SOS.Process.Repositories.Destination.Interfaces;
using SOS.Process.Repositories.Source.Interfaces;
using Xunit;

namespace SOS.Process.IntegrationTests.Processors.DarwinCoreArchive
{
    public class DwcaCompareVerbatimAndProcessedObservationTests : TestBase, IClassFixture<DwcaObservationFactoryIntegrationFixture>
    {
        private readonly DwcaObservationFactoryIntegrationFixture _fixture;

        public DwcaCompareVerbatimAndProcessedObservationTests(DwcaObservationFactoryIntegrationFixture fixture)
        {
            _fixture = fixture;
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
            List<DwcObservationVerbatim> observations = await dwcaReader.ReadArchiveAsync(archiveReader, dataProviderIdIdentifierTuple);
            var observationComparisions = new List<CompareObservation>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            foreach (var verbatimObservation in observations.Take(nrObservationsToSave))
            {
                var processedObservation = _fixture.DwcaObservationFactory.CreateProcessedObservation(verbatimObservation);
                fieldMappingResolverHelper.ResolveFieldMappedValues(new List<ProcessedObservation> {processedObservation});
                observationComparisions.Add(new CompareObservation
                {
                    VerbatimObservation = verbatimObservation,
                    ProcessedObservation = processedObservation
                });
            }

            string strJsonObservationCompare = JsonHelper.SerializeToMinimalJson(observationComparisions);
            await File.WriteAllTextAsync(savePath, strJsonObservationCompare);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            strJsonObservationCompare.Should().NotBeEmpty();
        }

        private FieldMappingResolverHelper CreateFieldMappingResolverHelper()
        {
            var processConfiguration = GetProcessConfiguration();
            var processClient = new ProcessClient(
                processConfiguration.ProcessedDbConfiguration.GetMongoDbSettings(),
                processConfiguration.ProcessedDbConfiguration.DatabaseName,
                processConfiguration.ProcessedDbConfiguration.BatchSize);
            var processedFieldMappingRepository = new ProcessedFieldMappingRepository(processClient, new NullLogger<ProcessedFieldMappingRepository>());
            return new FieldMappingResolverHelper(processedFieldMappingRepository, new FieldMappingConfiguration() { LocalizationCultureCode = "sv-SE", ResolveValues = true });
        }

        public class CompareObservation
        {
            public object VerbatimObservation { get; set; }
            public object ProcessedObservation { get; set; }
        }
    }
}