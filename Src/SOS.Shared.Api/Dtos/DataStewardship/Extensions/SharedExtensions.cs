using System.Collections.Generic;
using System.Linq;

namespace SOS.Shared.Api.Dtos.DataStewardship.Extensions
{
    public static class SharedExtensions
    {
        public static string Concat(this IEnumerable<string> values, int maxCount = 0)
        {
            if (!values?.Any() ?? true)
            {
                return null!;
            }

            if (values.Count() > maxCount)
            {
                return $"{string.Join(',', values.Take(maxCount))}...";
            }

            return string.Join(',', values);
        }
    }
}
