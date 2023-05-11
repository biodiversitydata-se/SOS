using DwC_A;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Harvest.DarwinCore.Factories
{
    public static class DwcAudubonMediaFactory
    {
        public static DwcAudubonMedia Create(IRow row)
        {
            var item = new DwcAudubonMedia();
            foreach (var fieldType in row.FieldMetaData)
            {
                var val = row[fieldType.Index];
                if (!string.IsNullOrEmpty(val))
                {
                    DwcTermValueMapper.MapValueByTerm(item, fieldType.Term, val);
                }
            }

            return item;
        }
    }
}