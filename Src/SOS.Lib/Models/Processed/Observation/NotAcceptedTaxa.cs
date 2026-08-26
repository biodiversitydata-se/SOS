using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Lib.Models.Processed.Observation;

public class NotAcceptedTaxa<T>
{
    public T TaxonId { get; set; }
    public string ScientificName { get; set; }
    public string VernacularName { get; set; }
    public string TaxonRemarks { get; set; }
    public string ConceptDefinition { get; set; }
    public int? TaxonCategoryId { get; set; }
    public string TaxonCategorySwedishName { get; set; }
    public string TaxonCategoryEnglishName { get; set; }
    public string TaxonCategoryDarwinCoreName { get; set; }

}
