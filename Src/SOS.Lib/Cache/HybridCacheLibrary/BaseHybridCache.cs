using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SOS.Lib.Cache.HybridCacheLibrary;
public abstract class BaseHybridCache<K, V> : ICache<K, V>
{
    protected long _capacity;
    internal ConcurrentDictionary<K, Node<K, V>> _cache;
    internal ConcurrentDictionary<int, DoublyLinkedList<K, V>> _frequencyList;
    protected int _minFrequency;
    internal readonly NodePool<K, V> _nodePool = new NodePool<K, V>();
    protected readonly object _lockObject = new object();

    public abstract long Capacity { get; }

    public BaseHybridCache(long capacity) // Constructor eklendi
    {
        _capacity = capacity;
        _cache = new ConcurrentDictionary<K, Node<K, V>>();
        _frequencyList = new ConcurrentDictionary<int, DoublyLinkedList<K, V>>();
        _minFrequency = 1;
    }

    public abstract void Add(K key, V value);
    public abstract void Add(K key, V value, int frequency);
    protected abstract void Evict();
    public abstract IEnumerator<KeyValuePair<K, V>> GetEnumerator();


    public V Get(K key)
    {
        if (!_cache.TryGetValue(key, out var node))
        {
            throw new KeyNotFoundException("The given key was not present in the cache.");
        }

        lock (node)
        {
            UpdateNodeFrequency(node);
        }

        return node.Value;
    }
    public bool TryGet(K key, out V value)
    {
        if (_cache.TryGetValue(key, out var node))
        {
            value = node.Value;
            lock (node)
            {
                UpdateNodeFrequency(node);
            }
            return true;
        }
        value = default;
        return false;
    }
    public int GetFrequency(K key)
    {
        if (!_cache.TryGetValue(key, out var node))
        {
            throw new KeyNotFoundException("The given key was not present in the cache.");
        }
        return node.Frequency;
    }

    internal void AddToFrequencyList(Node<K, V> node)
    {
        if (!_frequencyList.TryGetValue(node.Frequency, out var list))
        {
            list = new DoublyLinkedList<K, V>();
            _frequencyList[node.Frequency] = list;
        }

        list.AddFirst(node);

        if (node.Frequency == 1 || node.Frequency < _minFrequency)
        {
            _minFrequency = node.Frequency;
        }
    }

    internal void UpdateNodeFrequency(Node<K, V> node)
    {
        var oldFrequency = node.Frequency;
        if (_frequencyList.TryGetValue(oldFrequency, out var oldList))
        {
            oldList.Remove(node);

            if (oldFrequency == _minFrequency && oldList.IsEmpty())
            {
                _minFrequency++;
                _frequencyList.TryRemove(oldFrequency, out _);
            }
        }

        node.Frequency++;
        AddToFrequencyList(node);
    }

    internal IEnumerator<KeyValuePair<K, Node<K, V>>> GetCacheEnumerator()
    {
        return _cache.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
