namespace SOS.Lib.Models.Verbatim.Artportalen
{
    public class PersonSighting
    {
        public string Observers { get; set; }
        public string VerifiedBy { get; set; }
        public string ReportedBy { get; set; }
        public int ReportedByUserId { get; set; }
        public string SpeciesCollection { get; set; }

        public override string ToString()
        {
            return $"{nameof(Observers)}: \"{Observers}\", {nameof(VerifiedBy)}: \"{VerifiedBy}\", {nameof(ReportedBy)}: \"{ReportedBy}\", {nameof(SpeciesCollection)}: \"{SpeciesCollection}\"";
        }
    }
}
