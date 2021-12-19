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

        /// <summary>
        /// Remove control characters and other non-printable characters.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="newLineTabReplacement"></param>
        /// <param name="otherCharReplacement"></param>
        /// <returns></returns>
        public static string RemoveIllegalCharacters(this string str, string newLineTabReplacement = " ", string otherCharReplacement = "")
        {
            return DwcFormatter.RemoveIllegalCharacters(str, newLineTabReplacement, otherCharReplacement);
        }
    }
}