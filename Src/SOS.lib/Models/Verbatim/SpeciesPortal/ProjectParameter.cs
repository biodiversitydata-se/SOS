using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Lib.Models.Verbatim.SpeciesPortal
{
    /// <summary>
    /// Project parameter.
    /// </summary>
    public class ProjectParameter
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