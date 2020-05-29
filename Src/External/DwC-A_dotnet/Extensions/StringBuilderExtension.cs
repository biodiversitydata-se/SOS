using System.Text;

namespace DwC_A.Extensions
{
    internal static class StringBuilderExtension
    {
        public static string Flush(this StringBuilder stringBuilder)
        {
            var toString = stringBuilder.ToString();
            stringBuilder.Clear();
            return toString;
        }
    }
}