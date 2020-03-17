using SOS.Lib.Constants;

namespace SOS.Lib.Models.Processed.Observation
{
    public class ProcessedFieldMapValue
    {
        public int Id { get; set; }
        public string Value { get; set; }

        public static ProcessedFieldMapValue Create(int? val)
        {
            return !val.HasValue ? null : new ProcessedFieldMapValue { Id = val.Value };
        }

        public static ProcessedFieldMapValue Create(string val)
        {
            return new ProcessedFieldMapValue { Id = FieldMappingConstants.NoMappingFoundCustomValueIsUsedId, Value = val};
        }
    }
}