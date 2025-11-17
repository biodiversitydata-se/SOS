using System.Reflection;

namespace SOS.DataStewardship.Api.IntegrationTests.Core.Extensions;

internal static class StringExtensions
{
    extension(string filePath)
    {
        public string GetAbsoluteFilePath()
        {
            if (Path.IsPathFullyQualified(filePath)) return filePath;

            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var absolutePath = Path.Combine(assemblyPath, filePath);
            return absolutePath;
        }
    }
}
