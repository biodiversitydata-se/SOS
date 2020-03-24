using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DwC_A.Terms;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Import.Harvesters.Observations;
using SOS.Import.MongoDb;
using SOS.Import.Repositories.Destination.DarwinCoreArchive;
using SOS.Lib.Enums;
using Xunit;

namespace SOS.Import.IntegrationTests.Harvesters.Observations
{
    public class DwcObservationHarvesterIntegrationTests : TestBase
    {
        private const string PsophusStridulusArchivePath = "./resources/dwca/dwca-occurrence-lifewatch-psophus-stridulus.zip";
        private const string DwcArchiveWithEmofExtension = "./resources/dwca/dwca-occurrence-emof-lifewatch.zip";
        private const string SamplingEventDwcArchiveWithMofExtension = "./resources/dwca/dwca-event-mof-swedish-butterfly-monitoring.zip";

        [Fact]
        public async Task Harvest_psophus_stridulus_occurrence_dwc_archive_observations()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcObservationHarvester = CreateDwcObservationHarvester();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var harvestInfo = await dwcObservationHarvester.HarvestObservationsAsync(PsophusStridulusArchivePath, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            harvestInfo.Status.Should().Be(RunStatus.Success);
        }

        [Fact]
        public async Task Harvest_occurrence_dwc_archive_with_emof_extension()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcObservationHarvester = CreateDwcObservationHarvester();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            // todo - handle emof extension
            var harvestInfo = await dwcObservationHarvester.HarvestObservationsAsync(DwcArchiveWithEmofExtension, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            harvestInfo.Status.Should().Be(RunStatus.Success);
        }

        [Fact]
        public async Task Harvest_sampling_event_dwc_archive_with_mof_extension()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var dwcObservationHarvester = CreateDwcObservationHarvester();

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            // todo - handle sampling event based dwc and measurementOrFact extension.
            // https://github.com/gbif/ipt/wiki/BestPracticesSamplingEventData
            // https://www.gbif.org/data-quality-requirements-sampling-events
            var harvestInfo = await dwcObservationHarvester.HarvestObservationsAsync(SamplingEventDwcArchiveWithMofExtension, JobCancellationToken.Null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            harvestInfo.Status.Should().Be(RunStatus.Success);
        }

        [Fact]
        public void CreateDwcMapperMapValueByTermSwitchStatements()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var fieldValues = GetFieldValues(typeof(Terms));
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("switch (term)");
            sb.AppendLine("{");
            foreach (var pair in fieldValues)
            {
                sb.AppendLine($"   case \"{pair.Value}\":");
                sb.AppendLine($"        observation.{CapitalizeFirstChar(pair.Key)} = val;");
                sb.AppendLine("        break;");
            }

            sb.AppendLine("}");

            var result = sb.ToString();
        }

        private string CapitalizeFirstChar(string input)
        {
            try
            {
                return input.First().ToString().ToUpper() + input.Substring(1);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        public static Dictionary<string, string> GetFieldValues(Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(string))
                .ToDictionary(f => f.Name,
                    f => (string)f.GetValue(null));
        }

        public static Dictionary<string, string> GetFieldValues(object obj)
        {
            return obj.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.FieldType == typeof(string))
                .ToDictionary(f => f.Name,
                    f => (string)f.GetValue(null));
        }
        private DwcObservationHarvester CreateDwcObservationHarvester()
        {
            var importConfiguration = GetImportConfiguration();
            var importClient = new ImportClient(
                importConfiguration.VerbatimDbConfiguration.GetMongoDbSettings(),
                importConfiguration.VerbatimDbConfiguration.DatabaseName,
                importConfiguration.VerbatimDbConfiguration.BatchSize);
            var dwcObservationHarvester = new DwcObservationHarvester(
                new DarwinCoreArchiveVerbatimRepository(
                    importClient, 
                    new NullLogger<DarwinCoreArchiveVerbatimRepository>()),
                new NullLogger<DwcObservationHarvester>());
            return dwcObservationHarvester;
        }
    }
}