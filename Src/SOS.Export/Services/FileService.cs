using System.IO;
using System.IO.Compression;
using SOS.Export.Services.Interfaces;

namespace SOS.Export.Services
{
    /// <summary>
    /// File services
    /// </summary>
    public class FileService : IFileService
    {
        /// <inheritdoc />
        public string CompressFolder(string path, string folderName)
        {
            var zipFilePath = Path.Join(path, $"{folderName}.zip");
            ZipFile.CreateFromDirectory($"{path}/{folderName}", zipFilePath);
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
            if (string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
    }
}
