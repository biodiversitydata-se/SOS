using DwC_A;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore.Factories
{
    public static class DwcAudubonMediaFactory
    {
        public static DwcAudubonMedia Create(IRow row)
        {
            var item = new DwcAudubonMedia();
            foreach (var fieldType in row.FieldMetaData)
            {
                var val = row[fieldType.Index];
                DwcTermValueMapper.MapValueByTerm(item, fieldType.Term, val);
            }

            return item;
        }
    }
}