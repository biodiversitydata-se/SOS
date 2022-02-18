namespace SOS.Lib.Services.Interfaces
{
    /// <summary>
    /// File service
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Compress directory
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="targetPath"></param>
        void CompressDirectory(string folderPath, string targetPath);

        /// <summary>
        ///     Compress folder content
        /// </summary>
        /// <param name="path"></param>
        /// <param name="folderName"></param>
        string CompressFolder(string path, string folderName);

        /// <summary>
        ///     Create a new folder
        /// </summary>
        /// <param name="path"></param>
        /// <param name="folder"></param>
        /// <returns></returns>
        void CreateFolder(string path, string folder);

        /// <summary>
        ///     Creates a new folder.
        /// </summary>
        /// <param name="folderPath">The folder path.</param>
        void CreateFolder(string folderPath);

        /// <summary>
        ///     Delete a file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        void DeleteFile(string path);

        /// <summary>
        ///     Delete a folder
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        void DeleteFolder(string path);
    }
}