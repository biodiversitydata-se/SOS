using SOS.Lib.Extensions;
using SOS.Lib.Models.Search.Filters;
using System.Text;

namespace SOS.Lib.IO;

public class FileWriterBase
{

    /// <summary>
    /// Store filter in folder o zip
    /// </summary>
    /// <param name="temporaryZipExportFolderPath"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    protected async Task StoreFilterAsync(string temporaryZipExportFolderPath, SearchFilter filter)
    {
        try
        {
            await using var fileStream = File.Create(Path.Combine(temporaryZipExportFolderPath, "filter.json"));
            await using var streamWriter = new StreamWriter(fileStream, Encoding.UTF8);
            await streamWriter.WriteAsync(filter.GetFilterAsJson());
            streamWriter.Close();
            await streamWriter.DisposeAsync();
            fileStream.Close();
            await fileStream.DisposeAsync();
        }
        catch
        {
            return;
        }
    }

    /// <summary>
    /// Save file in folder of zip
    /// </summary>
    /// <param name="temporaryZipExportFolderPath"></param>
    /// <param name="stream"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    protected async Task StoreFileAsync(string temporaryZipExportFolderPath, Stream stream, string fileName)
    {
        if (stream == null)
        {
            return;
        }
        try
        {
            await using var fileStream = File.Create(Path.Combine(temporaryZipExportFolderPath, fileName));
            await stream.CopyToAsync(fileStream);
         
            fileStream.Close();
            await fileStream.DisposeAsync();
        }
        catch
        {
            return;
        }
    }
}