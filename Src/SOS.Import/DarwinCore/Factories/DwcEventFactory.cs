using DwC_A;
using DwC_A.Meta;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore.Factories
{
    public static class DwcEventFactory
    {
        public static DwcEvent Create(IRow row, IIdIdentifierTuple idIdentifierTuple, int idIndex)
        {
            var dwcEvent = new DwcEvent();
            if (idIdentifierTuple != null)
            {
                dwcEvent.DataProviderId = idIdentifierTuple.Id;
                dwcEvent.DataProviderIdentifier = idIdentifierTuple.Identifier;
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