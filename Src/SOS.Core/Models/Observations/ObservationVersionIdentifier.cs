using System;
using System.Collections.Generic;
using System.Text;
using MessagePack;

namespace SOS.Core.Models.Observations
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class ObservationVersionIdentifier
    {
        public string Id { get; set; }
        public int DataProviderId { get; set; }
        public string CatalogNumber { get; set; }
        public int Version { get; set; }
    }
}
