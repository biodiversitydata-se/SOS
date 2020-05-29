using System.Collections.Generic;
using DwC_A.Meta;

namespace DwC_A.Factories
{
    public interface IRowFactory
    {
        IRow CreateRow(IEnumerable<string> fields, IFieldMetaData fieldMetaData);
    }
}