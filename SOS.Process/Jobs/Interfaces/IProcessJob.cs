﻿using System.Threading.Tasks;
using Hangfire;

namespace SOS.Process.Jobs.Interfaces
{
    public interface IProcessJob
    {
        /// <summary>
        /// Process data
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<bool> Run(int sources, IJobCancellationToken cancellationToken);
    }
}
