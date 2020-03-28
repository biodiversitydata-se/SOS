using DwC_A;
using DwC_A.Meta;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore.Factories
{
    public static class DwcEventFactory
    {
        public static DwcEvent Create(IRow row, string filename, int idIndex)
        {
            var dwcEvent = new DwcEvent { DwcArchiveFilename = filename };
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