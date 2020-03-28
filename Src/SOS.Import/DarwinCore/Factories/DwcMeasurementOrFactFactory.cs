using System;
using System.Collections.Generic;
using System.Text;
using DwC_A;
using SOS.Lib.Models.Verbatim.DarwinCore;

namespace SOS.Import.DarwinCore.Factories
{
    public static class DwcMeasurementOrFactFactory
    {
        public static DwcMeasurementOrFact Create(IRow row)
        {
            var item = new DwcMeasurementOrFact();
            foreach (var fieldType in row.FieldMetaData)
            {
                var val = row[fieldType.Index];
                DwcTermValueMapper.MapValueByTerm(item, fieldType.Term, val);
            }

            return item;
        }
    }
}