using System.Collections.Generic;
using SOS.Lib.Models.Shared;

namespace SOS.Lib.Models.Verbatim.Artportalen
{
    public class PersonSighting
    {
        public string Observers { get; set; }
        public IEnumerable<UserInternal> ObserversInternal { get; set; }
        public string VerifiedBy { get; set; }
        public IEnumerable<UserInternal> VerifiedByInternal { get; set; }
        public string ReportedBy { get; set; }
        public int ReportedByUserId { get; set; }
        public int? ReportedByUserServiceUserId { get; set; }
        public string ReportedByUserAlias { get; set; }
        public string SpeciesCollection { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(Observers)}: \"{Observers}\", {nameof(VerifiedBy)}: \"{VerifiedBy}\", {nameof(ReportedBy)}: \"{ReportedBy}\", {nameof(SpeciesCollection)}: \"{SpeciesCollection}\"";
        }
    }
}