using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using KulService;
using SOS.Import.Repositories.Source.Kul.Interfaces;

namespace SOS.Import.Repositories.Source.Kul
{
    public class KulSightingRepository : IKulSightingRepository
    {
        public async Task<IEnumerable<KulService.WebSpeciesObservation>> GetAsync(DateTime changedFrom, DateTime changedTo)
        {
            KulService.SpeciesObservationChangeServiceClient client = new SpeciesObservationChangeServiceClient();
            string token = ""; // todo - initialize from settings.
            const int maxReturnedChanges = 100000;

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            var result = await client.GetSpeciesObservationChangeAsSpeciesAsync(
                token,
                changedFrom,
                true,
                changedTo,
                true,
                0,
                false,
                maxReturnedChanges);

            Debug.WriteLine($"ChangedFrom: {changedFrom.ToShortDateString()}, ChangedTo: {changedTo.ToShortDateString()}, Created: {result.CreatedSpeciesObservations.Length}, Updated: {result.UpdatedSpeciesObservations.Length}, Deleted: {result.DeletedSpeciesObservationGuids.Length}");

            return result.CreatedSpeciesObservations;
        }
    }
}
