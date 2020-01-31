using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SOS.Lib.Enums;

namespace SOS.Process.Jobs.Interfaces
{
    public interface ICopyFieldMappingsJob
    {
        /// <summary>
        /// Copy field mappings from verbatim db to process db.
        /// </summary>
        /// <returns></returns>
        Task<bool> RunAsync();
    }
}
