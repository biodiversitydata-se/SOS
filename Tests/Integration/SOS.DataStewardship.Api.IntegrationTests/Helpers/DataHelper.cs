namespace SOS.DataStewardship.Api.IntegrationTests.Helpers
{
    internal static class DataHelper
    {
        private static Random _random = new Random();
        internal static string RandomString(int length, IEnumerable<string> blackList = null!)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ0123456789";
            var value = new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());

            if (blackList?.Contains(value, StringComparer.CurrentCultureIgnoreCase) ?? false)
            {
                return RandomString(length, blackList);
            }

            return value;
        }
    }
}
