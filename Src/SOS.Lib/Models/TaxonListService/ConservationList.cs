using System.Collections.Generic;

namespace SOS.Lib.Models.TaxonListService;

public class ConservationList
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; }
}

public class ConservationListsResult
{
    public List<ConservationList> ConservationLists { get; set; }
}
