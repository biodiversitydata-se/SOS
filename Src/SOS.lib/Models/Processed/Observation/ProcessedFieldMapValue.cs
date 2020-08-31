using System;
using SOS.Lib.Constants;

namespace SOS.Lib.Models.Processed.Observation
{
    public class ProcessedFieldMapValue
    {
        public int Id { get; set; }
        public string Value { get; set; }

        public static ProcessedFieldMapValue Create(int? val)
        {
            return !val.HasValue ? null : new ProcessedFieldMapValue {Id = val.Value};
        }

        public static ProcessedFieldMapValue Create(string val)
        {
            return new ProcessedFieldMapValue
                {Id = FieldMappingConstants.NoMappingFoundCustomValueIsUsedId, Value = val};
        }

        protected bool Equals(ProcessedFieldMapValue other)
        {
            return Id == other.Id && Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProcessedFieldMapValue) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Value);
        }
    }
}