using System;
using SOS.Lib.Constants;

namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// Describes a value associated with a vocabulary.
    /// </summary>
    public class VocabularyValue
    {
        /// <summary>
        /// If the entry exist in the vocabulary, then Id is greater than or equal to 0.
        /// If the entry doesn't exist in the vocabulary, then Id is equal to -1.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Indicates whether this value exists in the vocabulary.
        /// </summary>
        /// <returns></returns>
        public bool IsCustomValue()
        {
            return Id == -1;
        }

        public static VocabularyValue Create(int? val)
        {
            return !val.HasValue ? null : new VocabularyValue {Id = val.Value};
        }

        public static VocabularyValue Create(string val)
        {
            return new VocabularyValue
                {Id = VocabularyConstants.NoMappingFoundCustomValueIsUsedId, Value = val};
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