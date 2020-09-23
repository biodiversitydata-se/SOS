using SOS.Lib.Helpers;

namespace SOS.Lib.Extensions
{
    public static class DwcFormatterExtensions
    {
        /// <summary>
        /// Replace new line and tabs with the specified string.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="replacement"></param>
        /// <returns></returns>
        public static string RemoveNewLineTabs(this string str, string replacement = " ")
        {
            return DwcFormatter.RemoveNewLineTabs(str, replacement);
        }
    }
}