using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Administration.Gui
{
    public class AuthenticationConfiguration
    {
        public string SecretKey { get; set; }
        public string Issuer { get; set; }
        public string SecretPassword { get; set; }
    }
}
