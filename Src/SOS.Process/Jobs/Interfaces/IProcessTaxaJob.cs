using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using SOS.Lib.Enums;

namespace SOS.Process.Jobs.Interfaces
{
    public interface IProcessTaxaJob
    {
        /// <summary>
        /// Read taxonomy from verbatim database, do some conversions and adds it to processed database.
        /// </summary>
        /// <returns></returns>
        Task<bool> RunAsync();
    }
}
