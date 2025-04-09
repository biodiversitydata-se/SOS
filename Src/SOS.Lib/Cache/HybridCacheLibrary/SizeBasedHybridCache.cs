using System;
using System.Collections.Generic;

namespace SOS.Lib.Cache.HybridCacheLibrary;

public class SizeBasedHybridCache<K, V> : BaseHybridCache<K, V>
{
    private long _currentCacheSizeInBytes;
    private long _maxCacheSizeInBytes;

    public SizeBasedHybridCache(long maxCacheSize, CacheSizeType sizeType = CacheSizeType.Bytes) : base(maxCacheSize) // Base class capacity is irrelevant here
    {
        _maxCacheSizeInBytes = maxCacheSize * (sizeType switch
        {
            CacheSizeType.Bytes => 1,
            CacheSizeType.Kilobytes => 1024,
            CacheSizeType.Megabytes => 1024 * 1024,
            _ => throw new ArgumentOutOfRangeException(nameof(sizeType), sizeType, null)
        });
    }
    public long CurrentCacheSizeInBytes => _currentCacheSizeInBytes;
    public long RemainingCacheSizeInBytes => _maxCacheSizeInBytes - _currentCacheSizeInBytes;
    public override long Capacity => _maxCacheSizeInBytes;

    public override void Add(K key, V value)
    {
        Add(key, value, 1);
    }

    public override void Add(K key, V value, int frequency)
    {
        lock (_lockObject)
        {
            long newSize = ObjectSizeCalculator.CalculateObjectSize(value);

            while (_currentCacheSizeInBytes + newSize > _maxCacheSizeInBytes)
            {
                Evict();
            }

            if (_cache.TryGetValue(key, out var existingNode))
            {
                long existingSize = ObjectSizeCalculator.CalculateObjectSize(existingNode.Value);
                _currentCacheSizeInBytes -= existingSize;

                existingNode.Value = value;
                UpdateNodeFrequency(existingNode);
            }
            else
            {
                var newNode = _nodePool.Get(key, value);
                newNode.Frequency = frequency;
                _cache[key] = newNode;
                AddToFrequencyList(newNode);
            }

            _currentCacheSizeInBytes += newSize;
        }
    }

    protected override void Evict()
    {
        if (_frequencyList.TryGetValue(_minFrequency, out var list))
        {
            var nodeToEvict = list.RemoveLast();
            if (nodeToEvict != null)
            {
                long size = ObjectSizeCalculator.CalculateObjectSize(nodeToEvict.Value);
                _currentCacheSizeInBytes -= size;

                _cache.TryRemove(nodeToEvict.Key, out _);
                _nodePool.Return(nodeToEvict);

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

    public void SetCapacity(int newCapacity, bool shrink = false, CacheSizeType sizeType = CacheSizeType.Bytes)
    {
        lock (_lockObject)
        {
            if (newCapacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(newCapacity), "Capacity must be greater than zero.");
            }

            long newMaxCacheSizeInBytes = newCapacity * (sizeType switch
            {
                CacheSizeType.Bytes => 1,
                CacheSizeType.Kilobytes => 1024,
                CacheSizeType.Megabytes => 1024 * 1024,
                _ => throw new ArgumentOutOfRangeException(nameof(sizeType), sizeType, null)
            });

            if (newMaxCacheSizeInBytes == _maxCacheSizeInBytes)
            {
                return;
            }

            if (shrink)
            {
                _maxCacheSizeInBytes = newMaxCacheSizeInBytes;
            }

            while (_currentCacheSizeInBytes > newMaxCacheSizeInBytes)
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

