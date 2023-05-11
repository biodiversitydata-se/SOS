using System.Collections.Generic;

namespace SOS.Observations.Api.Models.DevOps
{
    public class MultiResponse<T>
    {
        public int Count { get; set; }

        public IEnumerable<T> Value { get; set; }
    }
}
