using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Lib.Models.Export
{
    public enum ExportJobStatus
    {
        Unknown = 0,
        Queued = 1,
        Processing = 2,
        Succeeded = 3,
        Failed = 4
    }
}
