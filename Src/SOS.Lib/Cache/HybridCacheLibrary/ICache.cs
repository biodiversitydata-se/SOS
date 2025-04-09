using System.Collections.Generic;

namespace SOS.Lib.Cache.HybridCacheLibrary;
public interface ICache<K, V> : IEnumerable<KeyValuePair<K, V>>
{
    V Get(K key);
    bool TryGet(K key, out V value);
    void Add(K key, V value);
    void Add(K key, V value, int frequency);
    int GetFrequency(K key);
    long Capacity { get; }
}
