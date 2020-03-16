using DwC_A.Meta;
using System.Collections.Generic;

namespace DwC_A.Factories
{
    public interface IRowFactory
    {
        IRow CreateRow(IEnumerable<string> fields, IFieldMetaData fieldMetaData);
    }
}