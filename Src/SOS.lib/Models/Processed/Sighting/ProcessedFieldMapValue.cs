using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Lib.Models.Processed.Sighting
{
    public class ProcessedFieldMapValue
    {
        public int Id { get; set; }
        public string Value { get; set; }

        public static ProcessedFieldMapValue Create(int? val)
        {
            return !val.HasValue ? null : new ProcessedFieldMapValue { Id = val.Value };
        }
    }
}