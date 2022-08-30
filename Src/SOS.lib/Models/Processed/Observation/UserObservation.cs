using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Processed.Observation
{
    public class UserObservation : IEntity<long>
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public int? UserServiceUserId { get; set; }
        public int SightingId { get; set; }
        public int TaxonId { get; set; }
        public int TaxonSpeciesGroupId { get; set; }
        public string ProvinceFeatureId { get; set; } // todo - should the data type be int
        public string MunicipalityFeatureId { get; set; } // todo - should the data type be int
        public string CountryRegionFeatureId { get; set; } // todo - should the data type be int
        public int? SiteId { get; set; }
        public DateTime StartDate { get; set; }
        public int ObservationYear { get; set; }
        public int ObservationMonth { get; set; }
        public List<int> ProjectIds { get; set; }
        public bool ProtectedBySystem { get; set; }
        public bool ProtectedByUser { get; set; }

        public static long CreateId()
        {
            long unique = Interlocked.Increment(ref _counter);
            return unique;
        }
        private static long _counter;
    }
}
