using System.Data.Common;
using DwC_A;
using DwC_A.Meta;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore.Factories
{
    public static class DwcObservationVerbatimFactory
    {
        public static DwcObservationVerbatim Create(IRow row, DwcaDatasetInfo datasetInfo, int idIndex)
        {
            var verbatimRecord = new DwcObservationVerbatim();
            if (datasetInfo != null)
            {
                verbatimRecord.DataProviderId = datasetInfo.DataProviderId;
                verbatimRecord.DataProviderIdentifier = datasetInfo.DataProviderIdentifier;
                verbatimRecord.DwcArchiveFilename = datasetInfo.ArchiveFilename;
            };
            foreach (FieldType fieldType in row.FieldMetaData)
            {
                var val = row[fieldType.Index];
                DwcTermValueMapper.MapValueByTerm(verbatimRecord, fieldType.Term, val);
            }
            
            verbatimRecord.RecordId = row[idIndex];
            return verbatimRecord;
        }
    }
}