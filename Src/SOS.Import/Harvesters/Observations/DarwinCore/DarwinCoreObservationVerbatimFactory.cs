using System.Diagnostics;
using DwC_A;
using DwC_A.Meta;
using DwC_A.Terms;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.Harvesters.Observations.DarwinCore
{
    public class DarwinCoreObservationVerbatimFactory
    {
        public static DarwinCoreObservationVerbatim Create(IRow row, string filename)
        {
            var verbatimRecord = new DarwinCoreObservationVerbatim {DwcArchiveFilename = filename};
            foreach (FieldType fieldType in row.FieldMetaData)
            {
                var val = row[fieldType.Index];
                DwcMapper.MapValueByTerm(verbatimRecord, fieldType.Term, val);
            }

            return verbatimRecord;
        }
    }
}