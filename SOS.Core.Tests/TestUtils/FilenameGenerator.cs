using System;

namespace SOS.Core.Tests.TestUtils
{
    /// <summary>
    /// Contains functions for generating file names that contain today's date and time.
    /// </summary>
    public static class FilenameGenerator
    {
        /// <summary>
        /// Creates a filename.
        /// </summary>
        /// <param name="name">The name part of the filename.</param>
        /// <param name="fileExtension">The file extension.</param>
        /// <returns>A valid filename.</returns>
        public static string CreateFilename(string name, string fileExtension)
        {
            return CreateFilename(name, fileExtension, DateTime.Now);
        }

        /// <summary>
        /// Creates a filename.
        /// </summary>
        /// <param name="name">The name part of the filename.</param>
        /// <param name="fileExtension">The file extension.</param>
        /// <param name="date">The date that will be used to generate the date part of the filename.</param>
        /// <returns>A valid filename.</returns>
        public static string CreateFilename(string name, string fileExtension, DateTime date)
        {
            string dateFilenamePart = GenerateDateFilenamePart(date);
            return string.Format("{0}-{1}.{2}", name, dateFilenamePart, fileExtension);
        }

        /// <summary>
        /// Generates the date filename part.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>Date part in a filename.</returns>
        private static string GenerateDateFilenamePart(DateTime date)
        {
            return string.Format("{0:yyyy-MM-dd}-{1:00}-{2:00}-{3:00}", date, date.Hour, date.Minute, date.Second);
        }
    }
}