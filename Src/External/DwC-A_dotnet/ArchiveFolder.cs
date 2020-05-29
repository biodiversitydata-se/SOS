using System;
using System.IO;
using System.IO.Compression;

namespace DwC_A
{
    internal class ArchiveFolder : IArchiveFolder
    {
        private readonly string fileName;
        private readonly string folderPath;

        /// <summary>
        ///     Extracts archive to a folder
        /// </summary>
        /// <param name="fileName">Zip archive file name</param>
        /// <param name="folderPath">Path to extract to.  Leave null for a temp folder</param>
        public ArchiveFolder(string fileName, string folderPath = null)
        {
            this.fileName = fileName;
            this.folderPath = string.IsNullOrEmpty(folderPath) ? GetTempPath() : folderPath;
        }

        public bool ShouldCleanup { get; private set; }

        public string Extract()
        {
            ZipFile.ExtractToDirectory(fileName, folderPath);
            return folderPath;
        }

        public void DeleteFolder()
        {
            Directory.Delete(folderPath, true);
        }

        private string GetTempPath()
        {
            ShouldCleanup = true;
            var newPath = Path.Combine(Path.GetTempPath(), "dwca", Guid.NewGuid().ToString());
            return newPath;
        }
    }
}