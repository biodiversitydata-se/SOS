using SOS.Lib.Services.Interfaces;
using System.IO;
using System.IO.Compression;

namespace SOS.Lib.Services;

/// <summary>
///     File services
/// </summary>
public class FileService : IFileService
{
    /// <inheritdoc />
    public void CompressDirectory(string sourcePath, string targetPath)
    {
        ZipFile.CreateFromDirectory(sourcePath, targetPath);
    }

    /// <inheritdoc />
    public void CreateDirectory(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }

    /// <inheritdoc />
    public void DeleteDirectory(string path)
    {
        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
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
    public bool IsDirectoryEmpty(string directoryPath)
    {
        return (Directory.GetFiles(directoryPath)?.Length ?? 0) == 0;
    }

    /// <inheritdoc />
    public void MoveFile(string sourcePath, string targetPath)
    {
        if (!string.IsNullOrEmpty(sourcePath) && File.Exists(sourcePath))
        {
            File.Move(sourcePath, targetPath, true);
        }
    }
    
}