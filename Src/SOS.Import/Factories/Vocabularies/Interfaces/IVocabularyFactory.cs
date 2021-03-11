using System.Threading.Tasks;

namespace SOS.Import.Factories.Vocabularies.Interfaces
{
    /// <summary>
    ///     Interface for creating vocabulary.
    /// </summary>
    public interface IVocabularyFactory
    {
        /// <summary>
        ///     Create vocabulary.
        /// </summary>
        /// <returns></returns>
        Task<Lib.Models.Shared.Vocabulary> CreateVocabularyAsync();
    }
}