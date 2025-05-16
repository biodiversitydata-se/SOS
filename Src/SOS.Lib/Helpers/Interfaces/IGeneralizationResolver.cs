using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search.Filters;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace SOS.Lib.Helpers.Interfaces
{
    public interface IGeneralizationResolver
    {
        Task ResolveGeneralizedObservationsAsync(SearchFilter filter, IEnumerable<JsonNode> observations);
        Task ResolveGeneralizedObservationsAsync(SearchFilter filter, IEnumerable<Observation> observations);
    }
}