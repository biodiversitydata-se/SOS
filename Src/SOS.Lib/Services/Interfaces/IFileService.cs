namespace SOS.Lib.Services.Interfaces;

/// <summary>
/// File service
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Compress directory
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="targetPath"></param>
    void CompressDirectory(string sourcePath, string targetPath);

    /// <summary>
    ///     Creates a new folder.
    /// </summary>
    /// <param name="folderPath">The folder path.</param>
    void CreateDirectory(string folderPath);

    /// <summary>
    ///     Delete a folder
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    void DeleteDirectory(string path);

    /// <summary>
    ///     Delete a file
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    void DeleteFile(string path);

    /// <summary>
    /// Check if directory is empty
    /// </summary>
    /// <param name="directoryPath"></param>
    /// <returns></returns>
    bool IsDirectoryEmpty(string directoryPath);

    /// <summary>
    /// Move a file
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="targetPath"></param>
    void MoveFile(string sourcePath, string targetPath);
}