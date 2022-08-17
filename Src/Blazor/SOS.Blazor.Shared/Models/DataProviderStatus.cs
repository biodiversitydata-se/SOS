using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Blazor.Shared.Models
{
    public class DataProviderStatus
    {
        public int Id { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string Organization { get; set; }
        public int PublicObservationsActiveInstanceCount { get; set; }
        public int PublicObservationsInactiveInstanceCount { get; set; }
        public int PublicObservationsDifference => PublicObservationsActiveInstanceCount - PublicObservationsInactiveInstanceCount;
        public int ProtectedObservationsActiveInstanceCount { get; set; }
        public int ProtectedObservationsInactiveInstanceCount { get; set; }
        public int ProtectedObservationsDifference => ProtectedObservationsActiveInstanceCount - ProtectedObservationsInactiveInstanceCount;
    }
}
