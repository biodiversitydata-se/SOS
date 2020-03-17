namespace SOS.Lib.Models.Processed.Observation
{
    public class TaxonVernacularName
    {
        public string Name { get; set; }
        public string Language { get; set; }
        public string CountryCode { get; set; }
        public bool IsPreferredName { get; set; }

        protected bool Equals(TaxonVernacularName other)
        {
            return Name == other.Name && Language == other.Language && CountryCode == other.CountryCode && IsPreferredName == other.IsPreferredName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TaxonVernacularName) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Language != null ? Language.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CountryCode != null ? CountryCode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsPreferredName.GetHashCode();
                return hashCode;
            }
        }
    }
}