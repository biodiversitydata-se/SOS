namespace SOS.Lib.Models.Gis
{
    public class GeoGridTileTaxonObservationCount
    {
        public int TaxonId { get; set; }
        public int ObservationCount { get; set; }

        public override string ToString()
        {
            return $"{nameof(TaxonId)}: {TaxonId}, {nameof(ObservationCount)}: {ObservationCount}";
        }
    }
}