namespace SOS.Harvest.Processors.Taxon.Interfaces
{
   public interface ITaxonProcessor
   {
       Task<int> ProcessTaxaAsync();
   }
}
