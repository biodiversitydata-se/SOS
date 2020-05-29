using System.Collections.Generic;
using DwC_A.Meta;

namespace DwC_A.Factories
{
    internal class RowFactory : IRowFactory
    {
        public IRow CreateRow(IEnumerable<string> fields, IFieldMetaData fieldMetaData)
        {
            return new Row(fields, fieldMetaData);
        }
    }
}