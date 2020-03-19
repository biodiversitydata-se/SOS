using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Observation;

namespace SOS.Process.Repositories.Destination.Interfaces
{
    /// <summary>
    /// Repository for retrieving processd taxa.
    /// </summary>
    public interface IProcessedTaxonRepository: IProcessBaseRepository<ProcessedTaxon, int>
    {
        /// <summary>
        /// Gets all processed taxa.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ProcessedTaxon>> GetTaxaAsync();
    }
}
