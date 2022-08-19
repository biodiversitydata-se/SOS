using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Blazor.Shared.Models
{
    public enum ApiEnvironment
    {
        Unknown = 0,
        Local = 1,
        Dev = 2,
        St = 3,
        At = 4,
        Prod = 5
    }
}
