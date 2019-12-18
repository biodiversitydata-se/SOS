using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Lib.Models.Processed.DarwinCore
{
    /// <summary>
    /// Darwin Core Project Parameter.
    /// </summary>
    public class DarwinCoreProjectParameter
    {
        public int SightingId { get; set; }
        public int ProjectId { get; set; }
        public int ProjectParameterId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string Value { get; set; }
        public string DataType { get; set; }
    }
}
