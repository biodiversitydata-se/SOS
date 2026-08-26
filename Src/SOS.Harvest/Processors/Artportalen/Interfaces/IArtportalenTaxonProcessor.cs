using System.Threading.Tasks;

namespace SOS.Harvest.Processors.Artportalen.Interfaces;

public interface IArtportalenTaxonProcessor
{
    Task<int> ProcessArtportalenTaxaAsync();
}
