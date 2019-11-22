using System.Text.RegularExpressions;

namespace SOS.Lib.Extensions
{
    public static class StringExtensions
    {
        public static string FirstAlfanumeric(this string value)
        {
            var regex = new Regex(@"\w+");
            return regex.Match(value).Value;
        }

        public static string LastAlfanumeric(this string value)
        {
            var regex = new Regex(@"\w+$");
            return regex.Match(value).Value;
        }
    }
}
