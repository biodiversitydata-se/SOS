using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
