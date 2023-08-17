using DwC_A;
using Microsoft.Extensions.Logging.Abstractions;
using SOS.ContainerIntegrationTests.Extensions;
using SOS.Harvest.DarwinCore.Interfaces;
using SOS.Harvest.DarwinCore;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Shared;

namespace SOS.ContainerIntegrationTests.Helpers;
internal static class DwcaHelper
{
    public static async Task<DwcaComposite> ReadDwcaFileAsync(string filePath, DataProvider dataProvider)
    {
        filePath = filePath.GetAbsoluteFilePath();
        IDwcArchiveReader dwcArchiveReader = new DwcArchiveReader(new NullLogger<DwcArchiveReader>());
        string outputPath = Path.GetTempPath();
        using var archiveReader = new ArchiveReader(filePath, outputPath); // @"C:\Temp\DwcaImport");
        var archiveReaderContext = ArchiveReaderContext.Create(archiveReader, dataProvider);

        var datasets = await dwcArchiveReader.ReadDatasetsAsync(archiveReaderContext);
        var occurrences = (await dwcArchiveReader.ReadOccurrencesAsync(archiveReaderContext))?.ToList();
        var events = await dwcArchiveReader.ReadEventsAsync(archiveReaderContext);
        var absentOccurrences = events?.SelectMany(m => m.CreateAbsentObservations()).ToList();
        occurrences?.AddRange(absentOccurrences!);

        return new DwcaComposite
        {
            Datasets = datasets == null ? new List<DwcVerbatimDataset>() : datasets,
            Events = events!,
            Occurrences = occurrences!
        };
    }

    public class DwcaComposite
    {
        public List<DwcVerbatimDataset> Datasets { get; set; }
        public IEnumerable<Lib.Models.Verbatim.DarwinCore.DwcEventOccurrenceVerbatim> Events { get; set; }
        public IEnumerable<Lib.Models.Verbatim.DarwinCore.DwcObservationVerbatim> Occurrences { get; set; }
    }
}