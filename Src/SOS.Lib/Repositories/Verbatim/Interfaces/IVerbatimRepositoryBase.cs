using MongoDB.Driver;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Repositories.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace SOS.Lib.Repositories.Verbatim.Interfaces;

/// <summary>
/// </summary>
public interface IVerbatimRepositoryBase<TEntity, TKey> : IRepositoryBase<TEntity, TKey> where TEntity : IEntity<TKey>
{
    /// <summary>
    /// Make collection permanent
    /// </summary>
    /// <returns></returns>
    Task<bool> PermanentizeCollectionAsync();

    /// <summary>
    /// Make collection permanent
    /// </summary>
    /// <returns></returns>
    Task<bool> PermanentizeCollectionAsync(bool? tempMode = null);

    /// <summary>
    /// Get source file for provider
    /// </summary>
    /// <param name="providerId"></param>
    /// <returns></returns>
    Task<Stream> GetSourceFileAsync(int providerId);

    /// <summary>
    /// Store verbatim file
    /// </summary>
    /// <param name="providerId"></param>
    /// <param name="fileStream"></param>
    /// <returns></returns>
    Task<bool> StoreSourceFileAsync(int providerId, Stream fileStream);

    /// <summary>
    /// Set repository in temp mode
    /// </summary>
    bool TempMode { get; set; }

    Task<bool> RenameCollectionAsync(string currentCollectionName, string newCollectionName);
    Task<bool> CopyCollectionAsync(string sourceCollectionName, string targetCollectionName, bool overwriteExistingTargetCollection = true);
    Task<bool> PermanentizeCollectionAsync(string tempCollectionName, string targetCollectionName);
    Task<bool> PermanentizeCollectionAsync(IMongoCollection<TEntity> tempCollection, IMongoCollection<TEntity> targetCollection);
    Task<bool> CheckDuplicatesAsync(string field, IMongoCollection<TEntity> mongoCollection);
}