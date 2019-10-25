using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using CsvHelper;
using CsvHelper.Configuration;
using SOS.Export.Services.Interfaces;

namespace SOS.Export.Services
{
    /// <summary>
    /// File services
    /// </summary>
    public class FileService : IFileService
    {
        /// <inheritdoc />
        public void CompressFolder(string path, string folder)
        {
            ZipFile.CreateFromDirectory($"{path}/{folder}", $"{path}/{folder}.zip");
        }

        /// <inheritdoc />
        public void CopyFiles(string sourcePath, IEnumerable<string> files, string destinationPath)
        {
            if (!files?.Any() == true)
            {
                return;
            }

            foreach (var file in files)
            {
                File.Copy($"{sourcePath}/{file}", $"{destinationPath}/{file}");
            }
        }

        /// <inheritdoc />
        public void CreateFolder(string path, string folder)
        {
            Directory.CreateDirectory($"{path}/{folder}");
        }

        /// <inheritdoc />
        public void DeleteFolder(string path)
        {
            Directory.Delete($"{path}", true);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetFolderFiles(string path)
        {
            return Directory.EnumerateFiles(path);
        }

        /// <inheritdoc />
        public XmlDocument GetXmlDocument(string path)
        {
            var document = new XmlDocument();
            document.Load(path);

            return document;
        }

        /// <inheritdoc />
        public async Task WriteToCsvFileAsync<T>(string filePath, bool create, IEnumerable<T> records, ClassMap<T> map)
        {
            if (!records?.Any() ?? true)
            {
                return;
            }

            await using var fileStream = new FileStream($"{filePath}", create ? FileMode.Create : FileMode.Append);
            await using var streamWriter = new StreamWriter(fileStream);
            using var csv = new CsvWriter(streamWriter, new Configuration { HasHeaderRecord = create });

            csv.Configuration.RegisterClassMap(map);
            csv.WriteRecords(records);
        }
    }
}
