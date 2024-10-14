using System.Collections.Generic;

namespace SOS.Lib.Models.TaxonListService
{
    /// <summary>
    /// Only created to make it cacheable
    /// </summary>
    public class TaxonListSetsById : Dictionary<int, (HashSet<int> Taxa, HashSet<int> WithUnderlyingTaxa)>
    {

    }
}
