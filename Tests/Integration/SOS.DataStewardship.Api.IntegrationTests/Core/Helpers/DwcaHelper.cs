using DwC_A;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.Harvest.DarwinCore.Interfaces;
using SOS.Harvest.DarwinCore;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.DataStewardship.Api.IntegrationTests.Core.Extensions;

namespace SOS.DataStewardship.Api.IntegrationTests.Core.Helpers
{
    internal static class DwcaHelper
    {
        public static async Task<DwcaComposite> ReadDwcaFileAsync(string filePath)
        {
            filePath = filePath.GetAbsoluteFilePath();
            var dataProvider = new DataProvider { Id = 105, Identifier = "TestDataStewardshipBats", Type = DataProviderType.DwcA };
            IDwcArchiveReader dwcArchiveReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
            string outputPath = Path.GetTempPath();
            using var archiveReader = new ArchiveReader(filePath, outputPath); // @"C:\Temp\DwcaImport");
            var archiveReaderContext = ArchiveReaderContext.Create(archiveReader, dataProvider);

            var datasets = await dwcArchiveReader.ReadDatasetsAsync(archiveReaderContext);
            var occurrences = await dwcArchiveReader.ReadOccurrencesAsync(archiveReaderContext);
            var events = await dwcArchiveReader.ReadEventsAsync(archiveReaderContext);

            return new DwcaComposite
            {
                Datasets = datasets,
                Events = events,
                Occurrences = occurrences
            };
        }

        public class DwcaComposite
        {
            public List<DwcVerbatimDataset> Datasets { get; set; }
            public IEnumerable<Lib.Models.Verbatim.DarwinCore.DwcEventOccurrenceVerbatim> Events { get; set; }
            public IEnumerable<Lib.Models.Verbatim.DarwinCore.DwcObservationVerbatim> Occurrences { get; set; }
        }
    }
}
