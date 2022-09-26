namespace SOS.UserStatistics.Api.Cache.Models
{
    public class Cache<TKey, TValue>
    {
        private ConcurrentDictionary<TKey, TValue> _cache = new();

        public int NrOfItems()
        {
            return _cache.Count;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return _cache.AddOrUpdate(key, addValue, updateValueFactory);
        }

        public bool ContainsKey(TKey key)
        {
            return _cache.ContainsKey(key);
        }
    }
}
