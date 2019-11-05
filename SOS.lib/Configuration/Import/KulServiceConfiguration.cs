using System;
using System.Collections.Generic;
using System.Text;

namespace SOS.Lib.Configuration.Import
{
    public class KulServiceConfiguration
    {
        /// <summary>
        /// A secret token is needed for authorization when making calls to KUL web service.
        /// </summary>
        public string Token { get; set; }
        
        /// <summary>
        /// Max number of sightings that will be returned.
        /// </summary>
        public int MaxReturnedChanges { get; set; } = 100000;
    }
}