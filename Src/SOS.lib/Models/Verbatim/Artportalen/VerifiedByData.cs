using SOS.Lib.Models.Shared;
using System.Collections.Generic;

namespace SOS.Lib.Models.Verbatim.Artportalen
{
    /// <summary>
    /// Contains properties used to create a verified by string.
    /// </summary>
    public class VerifiedByData
    {
        public int SightingId { get; set; }
        public string DeterminerName { get; set; }
        public string DeterminerText { get; set; }
        public int? SightingRelationDeterminationYear { get; set; } // SightingRelationTypeId.Determiner = 3
        public int? SpeciesCollectionItemDeterminerYear { get; set; }
        public int? DeterminerYear => SightingRelationDeterminationYear ?? SpeciesCollectionItemDeterminerYear;
        public string DeterminationDescription { get; set; }
        public string ConfirmatorName { get; set; }
        public string ConfirmatorText { get; set; }
        public int? SightingRelationConfirmationYear { get; set; } // SightingRelationTypeId.Confirmator = 5
        public int? SpeciesCollectionItemConfirmatorYear { get; set; }
        public int? ConfirmatorYear => SightingRelationConfirmationYear ?? SpeciesCollectionItemConfirmatorYear;
        public UserInternal DeterminerInternal { get; set; }
        public UserInternal ConfirmatorInternal { get; set; }
    }
}