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
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Processed.Validation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.Process.Helpers;
using SOS.Process.IntegrationTests.TestHelpers;
using SOS.Process.Models;
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

        public class ProcessObservationsResult
        {
            public int NrObservationsProcessed { get; set; }
            public int NrValidObservations { get; set; }
            public int NrInvalidObservations { get; set; }
            public List<ValidCompareObservation> ValidObservations { get; set; }
            public List<InvalidCompareObservation> InvalidObservations { get; set; }
        }

        public class ValidCompareObservation
        {
            public object VerbatimObservation { get; set; }
            public object ProcessedObservation { get; set; }
        }

        public class InvalidCompareObservation
        {
            public object VerbatimObservation { get; set; }
            public ICollection<string> ProcessedObservationDefects { get; set; }
        }


        [Fact]
        public async Task Process_SHARK_observations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/SHARK_Zooplankton_NAT_DwC-A.zip";
            const string savePath = @"c:\temp\SharkObservationsCompare.json";
            if (File.Exists(savePath)) File.Delete(savePath);
            var dataProvider = new DataProvider
            {
                Id = 101,
                Identifier = "SHARK",
                Type = DataProviderType.DwcA
            };
            using var archiveReader = new ArchiveReader(archivePath, @"c:\temp");

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await CreateProcessDwcaObservationsResult(archiveReader, dataProvider);
            var strJsonObservationCompare = SerializeToMinimalJson(result);
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
            const string archivePath = @"C:\DwC-A\Riksskogstaxeringen\Riksskogstaxeringen-RTFulldataset20200626.zip";
            const string savePath = @"C:\Temp\RiksskogstaxeringenObservationsCompare.json";
            if (File.Exists(savePath)) File.Delete(savePath);
            var dataProvider = new DataProvider
            {
                Id = 104,
                Identifier = "Riksskogstaxeringen",
                Type = DataProviderType.DwcA
            };
            using var archiveReader = new ArchiveReader(archivePath, @"c:\temp");

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await CreateProcessDwcaObservationsResult(archiveReader, dataProvider);
            var strJsonObservationCompare = SerializeToMinimalJson(result);
            await File.WriteAllTextAsync(savePath, strJsonObservationCompare);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            strJsonObservationCompare.Should().NotBeEmpty();
        }

        private async Task<ProcessObservationsResult> CreateProcessDwcaObservationsResult(
            ArchiveReader archiveReader,
            IIdIdentifierTuple idIdentifierTuple,
            int nrValidObservationsLimit = 100,
            int nrInvalidObservationsLimit = 100,
            int maxNrObservationsToRead = 100000)
        {
            var fieldMappingResolverHelper = CreateFieldMappingResolverHelper();
            var dwcaReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
            int batchSize = 100000;

            var observationsBatches = dwcaReader.ReadArchiveInBatchesAsync(
                archiveReader,
                idIdentifierTuple,
                batchSize);

            var validObservations = new List<ValidCompareObservation>();
            var invalidObservations = new List<InvalidCompareObservation>();
            int nrProcessedObservations = 0;
            int nrValidObservations = 0;
            int nrInvalidObservations = 0;
            await foreach (var observationsBatch in observationsBatches)
            {
                foreach (var verbatimObservation in observationsBatch)
                {
                    var processedObservation =
                        _fixture.DwcaObservationFactory.CreateProcessedObservation(verbatimObservation);
                    nrProcessedObservations++;
                    fieldMappingResolverHelper.ResolveFieldMappedValues(new List<ProcessedObservation>
                        {processedObservation});

                    var observationValidation = _fixture.ValidationManager.ValidateObservation(processedObservation);
                    if (observationValidation.IsValid)
                    {
                        nrValidObservations++;
                        if (validObservations.Count < nrValidObservationsLimit)
                        {
                            validObservations.Add(new ValidCompareObservation
                            {
                                VerbatimObservation = verbatimObservation,
                                ProcessedObservation = processedObservation
                            });
                        }
                    }
                    else
                    {
                        nrInvalidObservations++;
                        if (invalidObservations.Count < nrInvalidObservationsLimit)
                        {
                            invalidObservations.Add(new InvalidCompareObservation
                            {
                                VerbatimObservation = verbatimObservation,
                                ProcessedObservationDefects = observationValidation.Defects
                            });
                        }
                    }

                    if (nrProcessedObservations >= maxNrObservationsToRead) break;
                }

                if (nrProcessedObservations >= maxNrObservationsToRead) break;
            }

            return new ProcessObservationsResult()
            {
                NrObservationsProcessed = nrProcessedObservations,
                NrValidObservations = nrValidObservations,
                NrInvalidObservations = nrInvalidObservations,
                InvalidObservations = invalidObservations,
                ValidObservations = validObservations
            };
        }


        private string SerializeToMinimalJson(object obj)
        {
            List<string> deleteProperties = new List<string>
            {
                "ValidObservations.[*].VerbatimObservation.RecordId",
                "ValidObservations.[*].VerbatimObservation.Id",
                "ValidObservations.[*].VerbatimObservation.DataProviderId",
                "ValidObservations.[*].VerbatimObservation.DataProviderIdentifier",
                "ValidObservations.[*].ProcessedObservation.IsInEconomicZoneOfSweden",
                "ValidObservations.[*].ProcessedObservation.Location.Point",
                "ValidObservations.[*].ProcessedObservation.Location.PointWithBuffer",
                "InvalidObservations.[*].VerbatimObservation.RecordId",
                "InvalidObservations.[*].VerbatimObservation.Id",
                "InvalidObservations.[*].VerbatimObservation.DataProviderId",
                "InvalidObservations.[*].VerbatimObservation.DataProviderIdentifier",
            };

            List<string> keepPropertyNames = new List<string> { "Id", "Value" };
            return JsonHelper.SerializeToMinimalJson(obj, deleteProperties, keepPropertyNames);
        }
    }
}