namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// Taxon vernacular name.
    /// </summary>
    public class TaxonVernacularName
    {
        /// <summary>
        /// A common vernacular name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ISO 639-1 language code used for the vernacular name value..
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// The standard code for the country in which the vernacular name is used.
        /// Recommended best practice is to use the ISO 3166-1-alpha-2 country codes available as a vocabulary
        /// at http://rs.gbif.org/vocabulary/iso/3166-1_alpha2.xml. For multiple countries separate values with a comma ","
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// This term is true if the source citing the use of this vernacular name indicates the usage has 
        /// some preference or specific standing over other possible vernacular names used for the species.
        /// </summary>
        public bool IsPreferredName { get; set; }

        public override string ToString()
        {
            return $"{Name} [{Language}]";
        }

        protected bool Equals(TaxonVernacularName other)
        {
            return Name == other.Name && Language == other.Language && CountryCode == other.CountryCode &&
                   IsPreferredName == other.IsPreferredName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TaxonVernacularName) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name != null ? Name.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Language != null ? Language.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CountryCode != null ? CountryCode.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsPreferredName.GetHashCode();
                return hashCode;
            }
        }
    }
}