using DwC_A;
using SOS.Harvest.DarwinCore;
using SOS.Harvest.DarwinCore.Interfaces;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.IntegrationTests.Extensions;
using System.IO.Compression;

namespace SOS.Observations.Api.IntegrationTests.Helpers;
internal static class DwcaHelper
{
    public static async Task<DwcaComposite> ReadDwcaFileAsync(string filePath, DataProvider dataProvider)
    {
        filePath = filePath.GetAbsoluteFilePath();
        IDwcArchiveReader dwcArchiveReader = new DwcArchiveReader(dataProvider, 0);
        string outputPath = Path.GetTempPath();
        using var archiveReader = new ArchiveReader(filePath, outputPath);
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

    public static List<Dictionary<string, string>> ReadOccurrenceDwcFile(byte[] zipFileBytes)
    {
        var items = new List<Dictionary<string, string>>();
        using var memoryStream = new MemoryStream(zipFileBytes);
        using var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read);
        using var csvStream = zipArchive.GetEntry("occurrence.txt")?.Open();
        using var streamRdr = new StreamReader(csvStream!);
        var dwCReader = new NReco.Csv.CsvReader(streamRdr, "\t");
        var columnIdByHeader = new Dictionary<string, int>();
        var headerByColumnId = new Dictionary<int, string>();

        // Read header
        dwCReader.Read();
        for (int i = 0; i < dwCReader.FieldsCount; i++)
        {
            string val = dwCReader[i];
            columnIdByHeader.Add(val, i);
            headerByColumnId.Add(i, val);
        }

        // Read data
        while (dwCReader.Read())
        {
            var item = new Dictionary<string, string>();
            for (int i = 0; i < dwCReader.FieldsCount; i++)
            {
                string val = dwCReader[i];
                item.Add(headerByColumnId[i], val);
            }

            items.Add(item);
        }
        return items;
    }

    public class DwcaComposite
    {
        public List<DwcVerbatimDataset>? Datasets { get; set; }
        public IEnumerable<Lib.Models.Verbatim.DarwinCore.DwcEventOccurrenceVerbatim>? Events { get; set; }
        public IEnumerable<Lib.Models.Verbatim.DarwinCore.DwcObservationVerbatim>? Occurrences { get; set; }
    }
}