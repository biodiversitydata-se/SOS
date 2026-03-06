using System.Collections.Generic;

namespace SOS.Shared.Api.Dtos.Status;

public class TaxonListSensitivityStatisticsDto
{
    public class TaxonInfo
    {
        public int Id { get; set; }
        public string ScientificName { get; set; }
        public string SwedishName { get; set; }
    }

    public int Category3Count { get; set; }
    public int Category4Count { get; set; }
    public int Category5Count { get; set; }
    public List<TaxonInfo> Category3Taxa { get; set; }
    public List<TaxonInfo> Category4Taxa { get; set; }
    public List<TaxonInfo> Category5Taxa { get; set; }
}
