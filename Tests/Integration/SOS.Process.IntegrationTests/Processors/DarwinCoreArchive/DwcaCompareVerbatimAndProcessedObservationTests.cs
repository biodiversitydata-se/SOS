using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DwC_A;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SOS.Import.DarwinCore;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Database;
using SOS.Lib.Enums;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Lib.Repositories.Processed;
using SOS.Process.Helpers;
using SOS.Process.IntegrationTests.TestHelpers;
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

            var strJsonObservationCompare = SerializeToMinimalJson(observationComparisions);
            await File.WriteAllTextAsync(savePath, strJsonObservationCompare);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            strJsonObservationCompare.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Compare_Riksskogstaxeringen_verbatim_and_processed_data()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const int nrObservationsToSave = 1000;
            const string archivePath = @"C:\DwC-A\Riksskogstaxeringen\Riksskogstaxeringen-RTFulldataset20200626.zip";
            const string savePath = @"C:\Temp\RiksskogstaxeringenObservationsCompare.json";
            if (File.Exists(savePath)) File.Delete(savePath);
            var dataProvider = new DataProvider
            {
                Id = 104,
                Identifier = "Riksskogstaxeringen",
                Type = DataProviderType.DwcA
            };
            var fieldMappingResolverHelper = CreateFieldMappingResolverHelper();
            var dwcaReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
            using var archiveReader = new ArchiveReader(archivePath, @"c:\temp");
            var observations = await dwcaReader.ReadArchiveAsync(archiveReader, dataProvider, nrObservationsToSave);
            var observationComparisions = new List<CompareObservation>();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            foreach (var verbatimObservation in observations)
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

            var strJsonObservationCompare = SerializeToMinimalJson(observationComparisions);
            await File.WriteAllTextAsync(savePath, strJsonObservationCompare);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            strJsonObservationCompare.Should().NotBeEmpty();
        }

        private string SerializeToMinimalJson(object obj)
        {
            List<string> deleteProperties = new List<string>
            {
                "VerbatimObservation.RecordId",
                "VerbatimObservation.Id",
                "VerbatimObservation.DataProviderId",
                "VerbatimObservation.DataProviderIdentifier",
                "ProcessedObservation.IsInEconomicZoneOfSweden",
                "ProcessedObservation.Location.Point",
                "ProcessedObservation.Location.PointWithBuffer"
            };

            List<string> keepPropertyNames = new List<string> { "Id", "Value" };
            return JsonHelper.SerializeToMinimalJson(obj, deleteProperties, keepPropertyNames);
        }
    }
}