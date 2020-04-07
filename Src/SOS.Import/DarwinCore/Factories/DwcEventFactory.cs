using DwC_A;
using DwC_A.Meta;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore.Factories
{
    public static class DwcEventFactory
    {
        public static DwcEvent Create(IRow row, DwcaDatasetInfo datasetInfo, int idIndex)
        {
            var dwcEvent = new DwcEvent();
            if (datasetInfo != null)
            {
                dwcEvent.DataProviderId = datasetInfo.DataProviderId;
                dwcEvent.DataProviderIdentifier = datasetInfo.DataProviderIdentifier;
                dwcEvent.DwcArchiveFilename = datasetInfo.ArchiveFilename;
            }
            foreach (FieldType fieldType in row.FieldMetaData)
            {
                var val = row[fieldType.Index];
                DwcTermValueMapper.MapValueByTerm(dwcEvent, fieldType.Term, val);
            }

            dwcEvent.RecordId = row[idIndex];
            return dwcEvent;
        }
    }
}