﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using SOS.Lib.Models.Processed.DarwinCore;

namespace SOS.Process.Factories.Interfaces
{
    /// <summary>
    /// Process base factory
    /// </summary>
    public interface IProcessFactory
    {
        /// <summary>
        /// Process sightings
        /// </summary>
        /// <param name="taxa"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> ProcessAsync(IDictionary<int, DarwinCoreTaxon> taxa, IJobCancellationToken cancellationToken);

    }
}
