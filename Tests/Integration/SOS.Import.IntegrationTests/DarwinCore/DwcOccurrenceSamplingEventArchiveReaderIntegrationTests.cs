using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DwC_A;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Import.DarwinCore;
using SOS.Lib.Models.Verbatim.DarwinCore;
using Xunit;

namespace SOS.Import.IntegrationTests.DarwinCore
{
    public class DwcOccurrenceSamplingEventArchiveReaderIntegrationTests
    {
        [Fact]
        public async Task Read_event_dwca_with_emof_extension_as_occurrences()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-event-emof-vims_neamap.zip";
            var dwcArchiveReader = new DwcOccurrenceSamplingEventArchiveReader(new NullLogger<DwcArchiveReader>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            using var archiveReader = new ArchiveReader(archivePath);
            var observations = await dwcArchiveReader.ReadArchiveAsync(archiveReader);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count.Should().Be(37201);
            var obs = observations.Single(o => o.OccurrenceID == "e3cc741e-23e6-11e8-a64f-4f26032957e3");
            obs.EventExtendedMeasurementOrFacts
                .Count().Should().Be(6, "because there are 6 different event measurements for the 'Station:NM20130901150' event");
            obs.EventExtendedMeasurementOrFacts
                .Single(m => m.MeasurementType == "air temperature")
                .MeasurementValue.Should().Be("20");
            obs.EventExtendedMeasurementOrFacts
                .Single(m => m.MeasurementType == "wind speed")
                .MeasurementValue.Should().Be("9");

            obs.ObservationExtendedMeasurementOrFacts
                .Count().Should().Be(3, "because there are 3 different occurrence measurements for the 'e3cc741e-23e6-11e8-a64f-4f26032957e3' occurrence.");
            obs.ObservationExtendedMeasurementOrFacts
                .Single(o => o.MeasurementType == "catch per unit effort count")
                .MeasurementValue.Should().Be("1.304408918");
            obs.ObservationExtendedMeasurementOrFacts
                .Single(o => o.MeasurementType == "catch per unit effort biomass")
                .MeasurementValue.Should().Be("0.632638325");
            obs.ObservationExtendedMeasurementOrFacts
                .Single(o => o.MeasurementType == "total number of individuals caught")
                .MeasurementValue.Should().Be("1");

        }

        /// <summary>
        /// Some links to Sampling Event Data Dwc-A:
        /// - https://github.com/gbif/ipt/wiki/BestPracticesSamplingEventData
        /// - https://www.gbif.org/data-quality-requirements-sampling-events
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Read_event_dwca_with_mof_extension_swedish_butterfly_monitoring_as_occurrences()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            const string archivePath = "./resources/dwca/dwca-event-mof-swedish-butterfly-monitoring.zip";
            var dwcArchiveReader = new DwcOccurrenceSamplingEventArchiveReader(new NullLogger<DwcArchiveReader>());

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            using var archiveReader = new ArchiveReader(archivePath);
            var observations = await dwcArchiveReader.ReadArchiveAsync(archiveReader);

            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            observations.Count.Should().Be(105872);
            var obs = observations.Single(observation => observation.OccurrenceID == "SEBMS:10015:201062");
            obs.EventMeasurementOrFacts.Count.Should().Be(6, "because there are 6 different event measurements for the 'SEBMS:10015:201062' event");
            obs.EventMeasurementOrFacts
                .Single(measurement => measurement.MeasurementType == "Temperature")
                .MeasurementValue.Should().Be("19");
            obs.EventMeasurementOrFacts
                .Single(measurement => measurement.MeasurementType == "Sunshine")
                .MeasurementValue.Should().Be("80");
            obs.EventMeasurementOrFacts
                .Single(measurement => measurement.MeasurementType == "Wind direction")
                .MeasurementValue.Should().Be("0.0");
            obs.EventMeasurementOrFacts
                .Single(measurement => measurement.MeasurementType == "Wind Speed")
                .MeasurementValue.Should().Be("3");
            obs.EventMeasurementOrFacts
                .Single(measurement => measurement.MeasurementType == "ZeroObservation")
                .MeasurementValue.Should().Be("false");
            obs.EventMeasurementOrFacts
                .Single(measurement => measurement.MeasurementType == "Site type")
                .MeasurementValue.Should().Be("Point site");
        }
    }
}