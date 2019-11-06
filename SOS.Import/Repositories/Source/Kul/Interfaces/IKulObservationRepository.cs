using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KulService;

namespace SOS.Import.Repositories.Source.Kul.Interfaces
{
    public interface IKulObservationRepository
    {
        Task<IEnumerable<KulService.WebSpeciesObservation>> GetAsync(DateTime changedFrom, DateTime changedTo);
    }
}
