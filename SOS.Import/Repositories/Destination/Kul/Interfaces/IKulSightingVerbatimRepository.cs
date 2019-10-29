using System;
using System.Collections.Generic;
using System.Text;
using SOS.Import.Repositories.Destination.Interfaces;
using SOS.Lib.Models.Verbatim.Kul;

namespace SOS.Import.Repositories.Destination.Kul.Interfaces
{
    public interface IKulSightingVerbatimRepository : IVerbatimRepository<KulSightingVerbatim, string>
    {
    }
}
