using SOS.Lib.Extensions;
using SOS.Lib.Models.Search.Filters;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Lib.IO
{
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
                fileStream.Close();
            }
            catch
            {
                return;
            }
        }
    }
}
