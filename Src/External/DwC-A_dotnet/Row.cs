using DwC_A.Meta;
using System.Collections.Generic;
using System.Linq;

namespace DwC_A
{
    internal class Row : IRow
    {
        public Row(IEnumerable<string> fields, IFieldMetaData fieldMetaData)
        {
            this.FieldMetaData = fieldMetaData;
            this.FieldValueById = new Dictionary<int, string>();
            int index = 0;
            foreach (var val in fields)
            {
                this.FieldValueById.Add(index, val);
                index++;
            }
        }

        public IEnumerable<string> Fields => FieldValueById.Values;

        public IDictionary<int, string> FieldValueById { get; }

        public IFieldMetaData FieldMetaData { get; }

        public string this[string term]
        {
            get
            {
                var index = FieldMetaData.IndexOf(term);
                return this[index];
            }
        }

        public string this[int index]
        {
            get
            {
                // todo - improve performance by introducing Dictionary
                return FieldValueById[index];
                //return Fields.ElementAt(index);
            }
        }

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