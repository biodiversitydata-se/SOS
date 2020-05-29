using DwC_A;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore.Factories
{
    public static class DwcObservationVerbatimFactory
    {
        public static DwcObservationVerbatim Create(IRow row, IIdIdentifierTuple idIdentifierTuple, int idIndex)
        {
            var verbatimRecord = new DwcObservationVerbatim();
            if (idIdentifierTuple != null)
            {
                verbatimRecord.DataProviderId = idIdentifierTuple.Id;
                verbatimRecord.DataProviderIdentifier = idIdentifierTuple.Identifier;
            }

            foreach (var fieldType in row.FieldMetaData)
            {
                var val = row[fieldType.Index];
                DwcTermValueMapper.MapValueByTerm(verbatimRecord, fieldType.Term, val);
            }

            verbatimRecord.RecordId = row[idIndex];
            return verbatimRecord;
        }
    }
}