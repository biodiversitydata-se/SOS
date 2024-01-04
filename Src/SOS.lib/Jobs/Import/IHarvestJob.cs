﻿using Hangfire;
using SOS.Lib.Enums;
using System.Threading.Tasks;

namespace SOS.Lib.Jobs.Import
{
    public interface IHarvestJob
    {
        /// <summary>
        /// Run harvest job
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Queue("high")]
        Task<bool> RunAsync(IJobCancellationToken cancellationToken);

        /// <summary>
        /// Run job
        /// </summary>
        /// <param name="mode"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Queue("high")]
        Task<bool> RunAsync(JobRunModes mode, IJobCancellationToken cancellationToken);
    }
}