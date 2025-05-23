﻿using SOS.Lib.Models.Shared;
using System.Collections.Generic;

namespace SOS.Lib.Models.Verbatim.Artportalen
{
    public class PersonSighting
    {
        public string ConfirmedBy { get; set; }
        public int? ConfirmationYear { get; set; }
        public string DeterminedBy { get; set; }
        public int? DeterminationYear { get; set; }
        public string Observers { get; set; }
        public ICollection<UserInternal> ObserversInternal { get; set; }
        public string VerifiedBy { get; set; }
        public ICollection<UserInternal> VerifiedByInternal { get; set; }
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