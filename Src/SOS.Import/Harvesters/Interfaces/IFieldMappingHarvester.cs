using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Shared;

namespace SOS.Import.Harvesters.Interfaces
{
    /// <summary>
    ///     Interface for field mappings.
    /// </summary>
    public interface IFieldMappingHarvester
    {
        /// <summary>
        ///     Import field mappings to MongoDb.
        /// </summary>
        /// <returns></returns>
        Task<HarvestInfo> HarvestAsync();

        /// <summary>
        ///     Creates a field mapping json file for specified field.
        /// </summary>
        /// <param name="fieldMappingFieldId"></param>
        /// <returns></returns>
        Task<(string Filename, byte[] Bytes)> CreateFieldMappingFileAsync(FieldMappingFieldId fieldMappingFieldId);

        /// <summary>
        ///     Creates field mappings for all specified fields and creates a zip file.
        /// </summary>
        /// <param name="fieldMappingFieldIds"></param>
        /// <returns></returns>
        Task<byte[]> CreateFieldMappingsZipFileAsync(IEnumerable<FieldMappingFieldId> fieldMappingFieldIds);

        Task<IEnumerable<FieldMapping>> CreateAllFieldMappingsAsync(
            IEnumerable<FieldMappingFieldId> fieldMappingFieldIds);
    }
}