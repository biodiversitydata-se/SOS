using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Lib.Models.Processed.Sighting
{
    public class ProcessedFieldMapValue
    {
        public int Id { get; set; } // todo - should this be nullable or should we treat custom and null values as integers? I.e. -2 = null, -1 = custom value.
        public string Value { get; set; }
    }
}