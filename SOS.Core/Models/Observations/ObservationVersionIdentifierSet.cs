using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Core.Models.Observations
{
    public class ObservationVersionIdentifierSet
    {
        public string Hash { get; set; }
        public List<ObservationVersionIdentifier> ObservationVersionIdentifiers { get; set; }
    }
}
