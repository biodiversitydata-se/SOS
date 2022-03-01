namespace SOS.Harvest.Entities.Artportalen
{
    public class SpeciesCollectionItemEntity
    {
        public int SightingId { get; set; }
        public int? CollectorId { get; set; } // Linked to [User].Id
        public int? OrganizationId { get; set; }
        public string DeterminerText { get; set; }
        public int? DeterminerYear { get; set; }
        public string Description { get; set; }
        public string ConfirmatorText { get; set; }
        public int? ConfirmatorYear { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(SightingId)}: {SightingId}, {nameof(CollectorId)}: {CollectorId}, {nameof(OrganizationId)}: {OrganizationId}, {nameof(DeterminerText)}: {DeterminerText}, {nameof(DeterminerYear)}: {DeterminerYear}, {nameof(Description)}: {Description}, {nameof(ConfirmatorText)}: {ConfirmatorText}, {nameof(ConfirmatorYear)}: {ConfirmatorYear}";
        }
    }
}