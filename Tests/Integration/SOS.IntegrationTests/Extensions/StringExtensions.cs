using System.Reflection;

namespace SOS.ContainerIntegrationTests.Extensions;
internal static class StringExtensions
{
    public static string GetAbsoluteFilePath(this string filePath)
    {
        if (Path.IsPathFullyQualified(filePath)) return filePath;

        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var absolutePath = Path.Combine(assemblyPath!, filePath);
        return absolutePath;
    }
}