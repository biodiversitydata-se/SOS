using MongoDB.Driver;

namespace SOS.Lib.Models;

public sealed class CollectionSession<TEntity>
{
    public string CollectionName { get; }
    public string TempCollectionName { get; }

    public IMongoCollection<TEntity> Collection { get; }
    public IMongoCollection<TEntity> TempCollection { get; }

    public CollectionSession(
        string collectionName,
        string tempCollectionName,
        IMongoCollection<TEntity> collection,
        IMongoCollection<TEntity> tempCollection)
    {
        CollectionName = collectionName;
        TempCollectionName = tempCollectionName;
        Collection = collection;
        TempCollection = tempCollection;
    }
}

