using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Core.Models.Observations
{
    public class VerbatimTestDataProviderObservation
    {
        public int Id { get; set; }
        public string ScientificName { get; set; }
        public double XCoord { get; set; }
        public double YCoord { get; set; }
        public DateTime ObservedDate { get; set; }
        public string ObserverName { get; set; }
    }
}
