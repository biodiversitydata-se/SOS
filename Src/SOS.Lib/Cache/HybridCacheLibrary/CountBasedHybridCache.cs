using System;
using System.Collections.Generic;

namespace SOS.Lib.Cache.HybridCacheLibrary;

public class CountBasedHybridCache<K, V> : BaseHybridCache<K, V>
{
    private readonly int _initialCapacity;

    public CountBasedHybridCache(int capacity) : base(capacity)
    {
        _initialCapacity = capacity;
    }

    public override long Capacity => _capacity;

    public override void Add(K key, V value)
    {
        Add(key, value, 1);
    }
    
    public override void Add(K key, V value, int frequency)
    {
        lock (_lockObject)
        {
            if (_cache.TryGetValue(key, out var existingNode))
            {
                lock (existingNode)
                {
                    if (!EqualityComparer<V>.Default.Equals(existingNode.Value, value))
                    {
                        existingNode.Value = value;
                        UpdateNodeFrequency(existingNode);
                    }
                }
                return;
            }

            if (_cache.Count >= _capacity)
            {
                Evict();
            }

            var newNode = _nodePool.Get(key, value);
            newNode.Frequency = frequency;
            _cache[key] = newNode;
            AddToFrequencyList(newNode);
        }
    }

    protected override void Evict()
    {
        if (_frequencyList.TryGetValue(_minFrequency, out var list))
        {
            var nodeToEvict = list.RemoveLast();
            if (nodeToEvict != null)
            {
                if (_cache.TryRemove(nodeToEvict.Key, out _))
                {
                    _nodePool.Return(nodeToEvict);
                }

                if (list.IsEmpty())
                {
                    _frequencyList.TryRemove(_minFrequency, out _);
                    UpdateMinFrequency();
                }
            }
        }
    }

    private void UpdateMinFrequency()
    {
        foreach (var key in _frequencyList.Keys)
        {
            if (!_frequencyList[key].IsEmpty())
            {
                _minFrequency = key;
                return;
            }
        }
        _minFrequency = 1;
    }

    public void SetCapacity(int newCapacity, bool shrink = false)
    {
        lock (_lockObject)
        {
            if (newCapacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(newCapacity), "Capacity must be greater than zero.");
            }

            if (newCapacity == _capacity)
            {
                return;
            }

            _capacity = Math.Max(_initialCapacity, newCapacity);

            if (shrink)
            {
                _capacity = newCapacity;
            }

            long itemsToRemove = Math.Max(0, _cache.Count - _capacity);
            for (int i = 0; i < itemsToRemove; i++)
            {
                Evict();
            }
        }
    }

    public override IEnumerator<KeyValuePair<K, V>> GetEnumerator()
    {
        return new HybridCacheEnumerator<K, V>(this);
    }
}
