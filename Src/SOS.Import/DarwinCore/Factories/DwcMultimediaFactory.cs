using DwC_A;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore.Factories
{
    public static class DwcMultimediaFactory
    {
        public static DwcMultimedia Create(IRow row)
        {
            var item = new DwcMultimedia();
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