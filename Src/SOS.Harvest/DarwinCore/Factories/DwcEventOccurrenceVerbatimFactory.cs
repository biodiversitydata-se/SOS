using DwC_A;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Harvest.DarwinCore.Factories
{
    public static class DwcEventOccurrenceVerbatimFactory
    {
        public static DwcEventOccurrenceVerbatim Create(int id, IRow row, IIdIdentifierTuple idIdentifierTuple, int idIndex)
        {
            var verbatimRecord = new DwcEventOccurrenceVerbatim { Id = id };
            if (idIdentifierTuple != null)
            {
                verbatimRecord.DataProviderId = idIdentifierTuple.Id;
                verbatimRecord.DataProviderIdentifier = idIdentifierTuple.Identifier;
            }

            foreach (var fieldType in row.FieldMetaData)
            {
                var val = row[fieldType.Index];
                if (!string.IsNullOrEmpty(val))
                {
                    DwcTermValueMapper.MapValueByTerm(verbatimRecord, fieldType.Term, val);
                }
            }

            verbatimRecord.RecordId = row[idIndex];
            return verbatimRecord;
        }
    }

}