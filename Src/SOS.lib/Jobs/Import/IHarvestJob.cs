using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Hangfire;

namespace SOS.Lib.Jobs.Import
{
    public interface IHarvestJob
    {
        Task<bool> RunAsync(IJobCancellationToken cancellationToken);
    }
}
