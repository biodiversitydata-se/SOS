using System.Linq;
using System.Threading.Tasks;
using DwC_A;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Import.DarwinCore;
using Xunit;

namespace SOS.Import.IntegrationTests.DarwinCore
{
    public class DwcSamplingEventArchiveReaderIntegrationTests
    {
        [Fact]
        public async Task Read_event_dwca_with_emof_extension_as_events()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-event-emof-vims_neamap.zip";
            var dwcArchiveReader = new DwcSamplingEventArchiveReader(new NullLogger<DwcArchiveReader>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            using var archiveReader = new ArchiveReader(archivePath);
            var dwcEvents = await dwcArchiveReader.ReadArchiveAsync(archiveReader, null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            dwcEvents.Count.Should().Be(1970);
            var dwcEvent = dwcEvents.Single(e => e.EventID == "Station:NM20130901150");
            dwcEvent.ExtendedMeasurementOrFacts
                .Count().Should().Be(6,
                    "because there are 6 different event measurements for the 'Station:NM20130901150' event");
            dwcEvent.ExtendedMeasurementOrFacts
                .Single(m => m.MeasurementType == "air temperature")
                .MeasurementValue.Should().Be("20");
            dwcEvent.ExtendedMeasurementOrFacts
                .Single(m => m.MeasurementType == "wind speed")
                .MeasurementValue.Should().Be("9");
        }


        [Fact]
        public async Task Read_event_dwca_with_mof_extension_swedish_butterfly_monitoring_as_events()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-event-mof-swedish-butterfly-monitoring.zip";
            var dwcArchiveReader = new DwcSamplingEventArchiveReader(new NullLogger<DwcArchiveReader>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            using var archiveReader = new ArchiveReader(archivePath);
            var dwcEvents = await dwcArchiveReader.ReadArchiveAsync(archiveReader, null);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            dwcEvents.Count.Should().Be(20449);
            var dwcEvent = dwcEvents.Single(observation => observation.EventID == "SEBMS:10015");
            dwcEvent.MeasurementOrFacts.Count.Should().Be(6,
                "because there are 6 different event measurements for the 'SEBMS:10015:201062' event");
            dwcEvent.MeasurementOrFacts
                .Single(measurement => measurement.MeasurementType == "Temperature")
                .MeasurementValue.Should().Be("19");
            dwcEvent.MeasurementOrFacts
                .Single(measurement => measurement.MeasurementType == "Sunshine")
                .MeasurementValue.Should().Be("80");
            dwcEvent.MeasurementOrFacts
                .Single(measurement => measurement.MeasurementType == "Wind direction")
                .MeasurementValue.Should().Be("0.0");
            dwcEvent.MeasurementOrFacts
                .Single(measurement => measurement.MeasurementType == "Wind Speed")
                .MeasurementValue.Should().Be("3");
            dwcEvent.MeasurementOrFacts
                .Single(measurement => measurement.MeasurementType == "ZeroObservation")
                .MeasurementValue.Should().Be("false");
            dwcEvent.MeasurementOrFacts
                .Single(measurement => measurement.MeasurementType == "Site type")
                .MeasurementValue.Should().Be("Point site");
        }
    }
}