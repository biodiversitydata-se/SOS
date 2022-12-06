using DwC_A;
using SOS.Lib.Configuration.Import;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;

namespace SOS.Harvest.DarwinCore
{
    public class ArchiveReaderContext
    {
        public ArchiveReader ArchiveReader { get; set; }
        public Dictionary<string, DwcVerbatimObservationDataset> ObservationDatasetByEventId { get; set; }
        public Dictionary<string, List<string>> OccurrenceIdsByEventId { get; set; }
        public IIdIdentifierTuple DataProvider { get; set; }
        public int MaxNrObservationsToReturn { get; set; } = int.MaxValue;
        public int BatchSize { get; set; } = 100000;

        public static ArchiveReaderContext Create(ArchiveReader archiveReader)
        {
            return new ArchiveReaderContext
            {
                ArchiveReader = archiveReader
            };
        }

        public static ArchiveReaderContext Create(ArchiveReader archiveReader, IIdIdentifierTuple dataProvider)
        {
            return new ArchiveReaderContext
            {
                ArchiveReader = archiveReader,
                DataProvider = dataProvider
            };
        }

        public static ArchiveReaderContext Create(ArchiveReader archiveReader, IIdIdentifierTuple dataProvider, DwcaConfiguration dwcaConfiguration)
        {
            return new ArchiveReaderContext
            {
                ArchiveReader = archiveReader,
                DataProvider = dataProvider,
                MaxNrObservationsToReturn = dwcaConfiguration.MaxNumberOfSightingsHarvested.GetValueOrDefault(int.MaxValue),
                BatchSize= dwcaConfiguration.BatchSize
            };
        }
    }
}