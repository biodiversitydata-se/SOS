using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace SOS.Lib.DataStructures
{
    /// <summary>
    ///     Dictionary that can contain multiple values for each key.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    /// <remarks>
    ///     An alternative (perhaps faster) data structure to use, could be C5 MultiHashDictionary:
    ///     https://github.com/sestoft/C5/blob/master/C5.UserGuideExamples/MultiDictionary.cs
    /// </remarks>
    public class HashMapDictionary<TKey, TValue> : IEnumerable
    {
        private readonly ConcurrentDictionary<TKey, List<TValue>> _keyValue =
            new ConcurrentDictionary<TKey, List<TValue>>();

        private readonly ConcurrentDictionary<TValue, List<TKey>> _valueKey =
            new ConcurrentDictionary<TValue, List<TKey>>();

        public ICollection<TKey> Keys => _keyValue.Keys;

        public ICollection<TValue> Values => _valueKey.Keys;

        public int Count => _keyValue.Count;

        public bool IsReadOnly => false;

        public List<TValue> this[TKey index]
        {
            get => _keyValue[index];
            set => _keyValue[index] = value;
        }

        public List<TKey> this[TValue index]
        {
            get => _valueKey[index];
            set => _valueKey[index] = value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _keyValue.GetEnumerator();
        }

        public void Add(TKey key, TValue value)
        {
            lock (this)
            {
                if (!_keyValue.TryGetValue(key, out var result))
                    _keyValue.TryAdd(key, new List<TValue> {value});
                else if (!result.Contains(value))
                    result.Add(value);

                if (!_valueKey.TryGetValue(value, out var result2))
                    _valueKey.TryAdd(value, new List<TKey> {key});
                else if (!result2.Contains(key))
                    result2.Add(key);
            }
        }

        public bool TryGetValues(TKey key, out List<TValue> value)
        {
            if (key == null)
            {
                value = null;
                return false;
            }

            return _keyValue.TryGetValue(key, out value);
        }

        public bool TryGetKeys(TValue value, out List<TKey> key)
        {
            if (value == null)
            {
                key = null;
                return false;
            }

            return _valueKey.TryGetValue(value, out key);
        }

        public bool ContainsKey(TKey key)
        {
            return _keyValue.ContainsKey(key);
        }

        public bool ContainsValue(TValue value)
        {
            return _valueKey.ContainsKey(value);
        }

        public void Remove(TKey key)
        {
            lock (this)
            {
                if (_keyValue.TryRemove(key, out var values))
                {
                    foreach (var item in values)
                    {
                        var remove2 = _valueKey.TryRemove(item, out var keys);
                    }
                }
            }
        }

        public void Remove(TValue value)
        {
            lock (this)
            {
                if (_valueKey.TryRemove(value, out var keys))
                {
                    foreach (var item in keys)
                    {
                        var remove2 = _keyValue.TryRemove(item, out var values);
                    }
                }
            }
        }

        public void Clear()
        {
            _keyValue.Clear();
            _valueKey.Clear();
        }
    }
}