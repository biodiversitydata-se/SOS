namespace SOS.Lib.Models.Verbatim.Artportalen
{
    public class SpeciesCollectionItem
    {
        public int SightingId { get; set; }
        public int? CollectorId { get; set; } // Linked to [User].Id
        public int? OrganizationId { get; set; }
        public string DeterminerText { get; set; }
        public int? DeterminerYear { get; set; }
        public string Description { get; set; }
        public string ConfirmatorText { get; set; }
        public int? ConfirmatorYear { get; set; }
    }
}
