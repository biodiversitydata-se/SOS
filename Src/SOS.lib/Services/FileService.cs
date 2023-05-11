using System.IO;
using System.IO.Compression;
using SOS.Lib.Services.Interfaces;

namespace SOS.Lib.Services
{
    /// <summary>
    ///     File services
    /// </summary>
    public class FileService : IFileService
    {
        /// <inheritdoc />
        public void CompressDirectory(string folderPath, string targetPath)
        {
            ZipFile.CreateFromDirectory(folderPath, targetPath);
        }

        public string CompressFolder(string path, string folderName)
        {
            var zipFilePath = Path.Join(path, $"{folderName}.zip");
            CompressDirectory($"{path}/{folderName}", zipFilePath);
            return zipFilePath;
        }

        /// <inheritdoc />
        public void CreateFolder(string path, string folder)
        {
            Directory.CreateDirectory($"{path}/{folder}");
        }

        /// <inheritdoc />
        public void CreateFolder(string folderPath)
        {
            Directory.CreateDirectory(folderPath);
        }

        /// <inheritdoc />
        public void DeleteFile(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                File.Delete(path);
            }
        }

        /// <inheritdoc />
        public void DeleteFolder(string path)
        {
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }
}