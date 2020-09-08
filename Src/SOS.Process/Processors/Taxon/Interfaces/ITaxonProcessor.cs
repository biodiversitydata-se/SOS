using System.Threading.Tasks;

namespace SOS.Process.Processors.Taxon.Interfaces
{
   public interface ITaxonProcessor
   {
       Task<int> ProcessTaxaAsync();
   }
}
