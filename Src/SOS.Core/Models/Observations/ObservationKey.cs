using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Core.Models.Observations
{
    public class ObservationKey : IObservationKey
    {
        public int DataProviderId { get; set; }
        public string CatalogNumber { get; set; }

        public ObservationKey(int dataProviderId, string catalogNumber)
        {
            DataProviderId = dataProviderId;
            CatalogNumber = catalogNumber;
        }

        public bool Equals(ObservationKey other)
        {
            return DataProviderId == other.DataProviderId && CatalogNumber == other.CatalogNumber;
        }

        public override bool Equals(object obj)
        {
            return obj is ObservationKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (DataProviderId * 397) ^ (CatalogNumber != null ? CatalogNumber.GetHashCode() : 0);
            }
        }
    }
}
