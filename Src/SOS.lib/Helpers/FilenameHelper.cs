using System;

namespace SOS.Lib.Helpers
{
    /// <summary>
    ///     Contains functions for generating file names that contain today's date and time.
    /// </summary>
    public static class FilenameHelper
    {
        /// <summary>
        ///     Creates a filename by joining: name, file extension and current date & time.
        /// </summary>
        /// <param name="name">The name part of the filename.</param>
        /// <param name="fileExtension">The file extension.</param>
        /// <returns>A valid filename.</returns>
        public static string CreateFilenameWithDate(string name, string fileExtension)
        {
            return CreateFilenameWithDate(name, fileExtension, DateTime.Now);
        }

        /// <summary>
        ///     Creates a filename by joining: name and current date & time. File extension is excluded.
        /// </summary>
        /// <param name="name">The name part of the filename.</param>
        /// <param name="preserveFileExtension">If true, the file extension will be preserved.</param>
        /// <returns>A valid filename.</returns>
        public static string CreateFilenameWithDate(string name, bool preserveFileExtension = false)
        {
            string fileExtension = System.IO.Path.GetExtension(name);
            if (preserveFileExtension && !string.IsNullOrEmpty(fileExtension))
            {
                var dateFilenamePart = GenerateDateFilenamePart(DateTime.Now);
                var nameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(name);
                return $"{nameWithoutExtension}-{dateFilenamePart}{fileExtension}";
                //return System.IO.Path.Combine($"{nameWithoutExtension}-{dateFilenamePart}", fileExtension.Replace(".",""));
            }
            else
            {
                var dateFilenamePart = GenerateDateFilenamePart(DateTime.Now);
                return $"{name}-{dateFilenamePart}";
            }
        }

        /// <summary>
        ///     Creates a filename.
        /// </summary>
        /// <param name="name">The name part of the filename.</param>
        /// <param name="fileExtension">The file extension.</param>
        /// <param name="date">The date that will be used to generate the date part of the filename.</param>
        /// <returns>A valid filename.</returns>
        public static string CreateFilenameWithDate(string name, string fileExtension, DateTime date)
        {
            var dateFilenamePart = GenerateDateFilenamePart(date);
            return $"{name}-{dateFilenamePart}.{fileExtension}";
        }

        /// <summary>
        ///     Generates the date filename part.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>Date part in a filename.</returns>
        private static string GenerateDateFilenamePart(DateTime date)
        {
            return $"{date:yyyy-MM-dd}-{date.Hour:00}-{date.Minute:00}-{date.Second:00}";
        }
    }
}