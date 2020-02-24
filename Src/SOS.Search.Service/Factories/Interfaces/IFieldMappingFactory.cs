using System.Collections.Generic;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.TaxonTree;

namespace SOS.Search.Service.Factories.Interfaces
{
    /// <summary>
    /// Field mapping factory.
    /// </summary>
    public interface IFieldMappingFactory
    {
        /// <summary>
        /// Get field mappings
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<FieldMapping>> GetFieldMappingsAsync();

        /// <summary>
        /// Try get translated value.
        /// </summary>
        /// <param name="fieldId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="sosId"></param>
        /// <param name="translatedValue"></param>
        /// <returns></returns>
        bool TryGetTranslatedValue(FieldMappingFieldId fieldId, string cultureCode, int sosId, out string translatedValue);

        /// <summary>
        /// Tries to get a non localized value.
        /// </summary>
        /// <param name="fieldId"></param>
        /// <param name="sosId"></param>
        /// <param name="translatedValue"></param>
        /// <returns></returns>
        bool TryGetValue(FieldMappingFieldId fieldId, int sosId, out string translatedValue);
    }
}