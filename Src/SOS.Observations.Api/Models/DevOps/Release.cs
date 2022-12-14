using System.Collections.Generic;

namespace SOS.Observations.Api.Models.DevOps
{
    public class Release
    {
        public IEnumerable<Environment> Environments { get; set; }
    }
}
