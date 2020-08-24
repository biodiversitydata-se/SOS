namespace SOS.TestHelpers.Taxonomy
{
    public static class Taxon
    {
        public static (
            int DyntaxaTaxonId,
            string ScientificName,
            string VernacularName,
            string Author,
            string TaxonRank,
            string Kingdom) EquusAsinus = (
                DyntaxaTaxonId: 233622,
                ScientificName: "Equus asinus",
                VernacularName: "åsna",
                Author: "Linnaeus, 1758",
                TaxonRank: "species",
                Kingdom: "Animalia");

        public static (
            int DyntaxaTaxonId,
            bool IsRecommendedName,
            string ScientificName,
            string VernacularName,
            string Author,
            string TaxonRank,
            string Kingdom) TrientalisEuropaea = (
                DyntaxaTaxonId: 221154,
                IsRecommendedName: false,
                ScientificName: "Trientalis europaea",
                VernacularName: "skogsstjärna",
                Author: "(L.) U. Manns & Anderb.",
                TaxonRank: "species",
                Kingdom: "Plantae");

    }
}