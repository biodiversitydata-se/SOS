namespace SOS.Lib.Models.Processed.Observation
{
    /// <summary>
    /// Taxon scientific name.
    /// </summary>
    public class TaxonScientificName
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public TaxonScientificName()
        {

        }

        /// <summary>
        /// Scientific Name Author
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// A scientific name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// This term is true if the source citing the use of this scientific name indicates the usage has 
        /// some preference or specific standing over other possible scientific names used for the species.
        /// </summary>
        public bool IsPreferredName { get; set; }

        /// <summary>
        /// Valid for sighting
        /// </summary>
        public bool ValidForSighting { get; set; }

       
        protected bool Equals(TaxonScientificName other)
        {
            return Name == other.Name && Author == other.Author && IsPreferredName == other.IsPreferredName && ValidForSighting == other.ValidForSighting;
        }
    }
}