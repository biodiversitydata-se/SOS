using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SOS.DataStewardship.Api.IntegrationTests.Extensions
{
    internal static class StringExtensions
    {
        public static string GetAbsoluteFilePath(this string filePath)
        {
            if (Path.IsPathFullyQualified(filePath)) return filePath;

            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var absolutePath = Path.Combine(assemblyPath, filePath);
            return absolutePath;
        }
    }
}
