using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Factories.Interfaces
{
    /// <summary>
    /// Interface for field mappings.
    /// </summary>
    public interface IFieldMappingFactory
    {
        /// <summary>
        /// Import field mappings to MongoDb.
        /// </summary>
        /// <returns></returns>
        Task<bool> ImportAsync();

        /// <summary>
        /// Creates a field mapping json file for specified field.
        /// </summary>
        /// <param name="fieldMappingFieldId"></param>
        /// <returns></returns>
        Task<(string Filename, byte[] Bytes)> CreateFieldMappingFileAsync(FieldMappingFieldId fieldMappingFieldId);

        /// <summary>
        /// Creates field mappings for all specified fields and creates a zip file.
        /// </summary>
        /// <param name="fieldMappingFieldIds"></param>
        /// <returns></returns>
        Task<byte[]> CreateFieldMappingsZipFileAsync(IEnumerable<FieldMappingFieldId> fieldMappingFieldIds);
    }
}
