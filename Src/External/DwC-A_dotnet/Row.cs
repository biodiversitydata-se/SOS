using System.Collections.Generic;
using System.Linq;
using DwC_A.Meta;

namespace DwC_A
{
    internal class Row : IRow
    {
        public Row(IEnumerable<string> fields, IFieldMetaData fieldMetaData)
        {
            FieldMetaData = fieldMetaData;
            FieldValueById = new Dictionary<int, string>();
            var index = 0;
            foreach (var val in fields)
            {
                FieldValueById.Add(index, val);
                index++;
            }
        }

        public IDictionary<int, string> FieldValueById { get; }

        public IEnumerable<string> Fields => FieldValueById.Values;

        public IFieldMetaData FieldMetaData { get; }

        public string this[string term]
        {
            get
            {
                var index = FieldMetaData.IndexOf(term);
                return this[index];
            }
        }

        public bool IsValid => FieldMetaData.Count() == FieldValueById.Count;

        public string this[int index] =>
            // todo - improve performance by introducing Dictionary
            FieldValueById[index];

        //return Fields.ElementAt(index);
        public bool TryGetValue(string term, out string val)
        {
            if (FieldMetaData.TryGetTermIndex(term, out var termIndex))
            {
                val = this[termIndex];
                return true;
            }

            val = null;
            return false;
        }

        public string GetValue(string term)
        {
            if (FieldMetaData.TryGetTermIndex(term, out var termIndex))
            {
                return this[termIndex];
            }

            return null;
        }
    }
}