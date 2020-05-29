using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DwC_A.Exceptions;

namespace DwC_A.Meta
{
    internal class FieldMetaData : IFieldMetaData
    {
        private const string idFieldName = "id";
        private readonly IDictionary<string, int> fieldIndexDictionary;
        private readonly IEnumerable<FieldType> fieldTypes;
        private readonly IdFieldType idFieldType;

        public FieldMetaData(IdFieldType idFieldType, ICollection<FieldType> fieldTypes)
        {
            this.idFieldType = idFieldType;
            if (idFieldType != null && idFieldType.IndexSpecified
                                    && fieldTypes.All(n => n.Index != idFieldType.Index))
            {
                this.fieldTypes = fieldTypes.Append(new FieldType {Index = idFieldType.Index, Term = idFieldName})
                    .OrderBy(n => n.Index);
            }
            else
            {
                this.fieldTypes = fieldTypes
                    .OrderBy(n => n.Index);
            }

            fieldIndexDictionary = this.fieldTypes.ToDictionary(k => k.Term, v => v.Index);
        }

        public int IndexOf(string term)
        {
            if (!fieldIndexDictionary.ContainsKey(term))
            {
                throw new TermNotFoundException(term);
            }

            return fieldIndexDictionary[term];
        }

        public bool TryGetTermIndex(string term, out int index)
        {
            return fieldIndexDictionary.TryGetValue(term, out index);
        }

        public IEnumerator<FieldType> GetEnumerator()
        {
            return fieldTypes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return fieldTypes.GetEnumerator();
        }

        public FieldType this[int index] => fieldTypes.ElementAt(index);

        public FieldType this[string term] => fieldTypes.ElementAt(IndexOf(term));
    }
}