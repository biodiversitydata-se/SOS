using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Lib.Models.Analysis;
public class AooEooItem
{    
    public int? TaxonId { get; set; }
    public string? ScientificName { get; set; }
    public string? VernacularName { get; set; }
    public string TaxonIdCaption { get; set; }
    public string TaxonNameCaption { get; set; }

    public double? AlphaValue { get; set; }
    public string Id { get; set; }
    public int Aoo { get; set; }
    public int Eoo { get; set; }
    public int GridCellArea { get; set; }
    public string GridCellAreaUnit { get; set; } = "km2";
    public int ObservationsCount { get; set; } 
}
