using System;
using System.Threading.Tasks;
using SOS.Lib.Enums;

namespace SOS.Lib.Jobs.Process
{
    public interface ICopyProviderDataJob
    {
        /// <summary>
        /// Copy data from active to inactive instance.
        /// </summary>
        /// <param name="dataProviderId"></param>
        /// <returns></returns>
        Task<bool> RunAsync(int dataProviderId);
    }
}