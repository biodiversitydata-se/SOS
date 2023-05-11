using DwC_A;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Harvest.DarwinCore.Factories
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

            foreach (var fieldType in row.FieldMetaData)
            {
                var val = row[fieldType.Index];
                if (!string.IsNullOrEmpty(val))
                {
                    DwcTermValueMapper.MapValueByTerm(dwcEvent, fieldType.Term, val);
                }
            }

            dwcEvent.RecordId = row[idIndex];
            return dwcEvent;
        }
    }
}