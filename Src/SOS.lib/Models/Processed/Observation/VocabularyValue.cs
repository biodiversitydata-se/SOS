using System;
using SOS.Lib.Constants;

namespace SOS.Lib.Models.Processed.Observation
{
    public class VocabularyValue
    {
        public int Id { get; set; }
        public string Value { get; set; }

        public static VocabularyValue Create(int? val)
        {
            return !val.HasValue ? null : new VocabularyValue {Id = val.Value};
        }

        public static VocabularyValue Create(string val)
        {
            return new VocabularyValue
                {Id = FieldMappingConstants.NoMappingFoundCustomValueIsUsedId, Value = val};
        }

        protected bool Equals(VocabularyValue other)
        {
            return Id == other.Id && Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VocabularyValue) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Value);
        }
    }
}