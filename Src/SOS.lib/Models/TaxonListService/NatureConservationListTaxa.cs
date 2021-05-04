using System.Collections.Generic;

namespace SOS.Lib.Models.TaxonListService
{
    public class NatureConservationListTaxa
    {
        public int ListId { get; set; }
        public List<TaxonInformation> TaxonInformation { get; set; }
    }

    public class NatureConservationListTaxaResult
    {
        public List<NatureConservationListTaxa> NatureConservationListTaxa { get; set; }
    }
}