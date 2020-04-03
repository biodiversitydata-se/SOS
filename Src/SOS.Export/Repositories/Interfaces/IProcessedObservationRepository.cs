using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Search;

namespace SOS.Export.Repositories.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProcessedObservationRepository : IBaseRepository<ProcessedObservation, string>
    {
        public class ScrollObservationResults
        {
            public IEnumerable<ProcessedObservation> Documents { get; set; }
            public string ScrollId { get; set; }
        }
        public class ScrollProjectResults
        {
            public IEnumerable<ProcessedProject> Documents { get; set; }
            public string ScrollId { get; set; }
        }
        /// <summary>
        /// Get filtered chunk
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<ScrollObservationResults> StartGetChunkAsync(FilterBase filter, int skip, int take);

        /// <summary>
        /// Get filtered chunk
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<ScrollObservationResults> GetChunkAsync(string scrollId);

        /// <summary>
        /// Get project parameters.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<ScrollProjectResults> StartGetProjectParameters(FilterBase filter, int skip, int take);

        /// <summary>
        /// Get project parameters.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        Task<ScrollProjectResults> GetProjectParameters(string scrollId);
    }
}
