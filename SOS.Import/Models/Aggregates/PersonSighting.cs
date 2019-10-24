namespace SOS.Import.Models.Aggregates
{
    public class PersonSighting
    {
        public string Observers { get; set; }
        public string VerifiedBy { get; set; }
        public string ReportedBy { get; set; }
        public string SpeciesCollection { get; set; }

        public override string ToString()
        {
            return $"{nameof(Observers)}: \"{Observers}\", {nameof(VerifiedBy)}: \"{VerifiedBy}\", {nameof(ReportedBy)}: \"{ReportedBy}\", {nameof(SpeciesCollection)}: \"{SpeciesCollection}\"";
        }
    }
}
