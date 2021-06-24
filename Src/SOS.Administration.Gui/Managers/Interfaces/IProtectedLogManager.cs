using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Administration.Gui.Models;

namespace SOS.Administration.Gui.Managers.Interfaces
{
    public interface IProtectedLogManager
    {
        /// <summary>
        /// Search log
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        Task<IEnumerable<ProtectedLogDto>> SearchAsync(DateTime from, DateTime to);
    }
}
