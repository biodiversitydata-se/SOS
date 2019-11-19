using System.Text.RegularExpressions;

namespace SOS.Lib.Extensions
{
    public static class StringExtensions
    {
        public static string ToAlfanumeric(this string value)
        {
            var regex = new Regex(@"\w+");
            return regex.Match(value).Value;
        }
    }
}
