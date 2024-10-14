using SOS.Lib.Enums;
using SOS.Lib.Models.Shared;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Lib.Helpers;
public static class VocabularyHelper
{
    /// <summary>
    ///     Get vocabulary mappings.
    /// </summary>
    /// <param name="externalSystemId"></param>
    /// <param name="allVocabularies"></param>
    /// <param name="convertValuesToLowercase"></param>
    /// <returns></returns>
    public static IDictionary<VocabularyId, IDictionary<object, int>> GetVocabulariesDictionary(
        ExternalSystemId externalSystemId,
        ICollection<Vocabulary>? allVocabularies,
        bool convertValuesToLowercase)
    {
        var dic = new Dictionary<VocabularyId, IDictionary<object, int>>();

        if (allVocabularies?.Any() ?? false)
        {
            foreach (var vocabulary in allVocabularies)
            {
                var vocabularies = vocabulary.ExternalSystemsMapping.FirstOrDefault(m => m.Id == externalSystemId);
                if (vocabularies != null)
                {
                    var mapping = vocabularies.Mappings.Single();
                    var sosIdByValue = mapping.GetIdByValueDictionary(convertValuesToLowercase);
                    dic.Add(vocabulary.Id, sosIdByValue);
                }
            }
        }

        // Add missing entries. Initialize with empty dictionary.
        foreach (VocabularyId vocabularyId in (VocabularyId[])Enum.GetValues(typeof(VocabularyId)))
        {
            if (!dic.ContainsKey(vocabularyId))
            {
                dic.Add(vocabularyId, new Dictionary<object, int>());
            }
        }

        return dic;
    }
}
