using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using CsvHelper.Configuration;

namespace SOS.Export.Services.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// Compress folder content
        /// </summary>
        /// <param name="path"></param>
        /// <param name="folder"></param>
        void CompressFolder(string path, string folder);

        /// <summary>
        /// Copy files to destination
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="files"></param>
        /// <param name="destinationPath"></param>
        void CopyFiles(string sourcePath, IEnumerable<string> files, string destinationPath);

        /// <summary>
        /// Create a new folder
        /// </summary>
        /// <param name="path"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        void CreateFolder(string path, string folder);

        /// <summary>
        /// Delete a folder
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        void DeleteFolder(string path);

        /// <summary>
        /// Get name of all files in a folder
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        IEnumerable<string> GetFolderFiles(string path);

        /// <summary>
        /// Get a xml document from disk
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        XmlDocument GetXmlDocument(string path);

        /// <summary>
        /// Save XML document
        /// </summary>
        /// <param name="xmlDocument"></param>
        /// <param name="path"></param>
        void SaveXmlDocument(XmlDocument xmlDocument, string path);

        /// <summary>
        /// Create or append data to csv file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="create"></param>
        /// <param name="records"></param>
        /// <param name="map"></param>
        /// <returns></returns>
        Task WriteToCsvFileAsync<T>(string filePath, bool create, IEnumerable<T> records, ClassMap<T> map);

        
    }
}
