using System.Collections;
using System.Collections.Generic;

namespace SOS.Lib.DataStructures
{
    /// <summary>
    /// Dictionary that can contain multiple values for each key.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public class HashMapDictionary<TKey, TValue> : IEnumerable
    {
        private readonly System.Collections.Concurrent.ConcurrentDictionary<TKey, List<TValue>> _keyValue = new System.Collections.Concurrent.ConcurrentDictionary<TKey, List<TValue>>();
        private readonly System.Collections.Concurrent.ConcurrentDictionary<TValue, List<TKey>> _valueKey = new System.Collections.Concurrent.ConcurrentDictionary<TValue, List<TKey>>();

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

        public void Add(TKey key, TValue value)
        {
            lock (this)
            {
                if (!_keyValue.TryGetValue(key, out List<TValue> result))
                    _keyValue.TryAdd(key, new List<TValue>() { value });
                else if (!result.Contains(value))
                    result.Add(value);

                if (!_valueKey.TryGetValue(value, out List<TKey> result2))
                    _valueKey.TryAdd(value, new List<TKey>() { key });
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
                if (_keyValue.TryRemove(key, out List<TValue> values))
                {
                    foreach (var item in values)
                    {
                        var remove2 = _valueKey.TryRemove(item, out List<TKey> keys);
                    }
                }
            }
        }

        public void Remove(TValue value)
        {
            lock (this)
            {
                if (_valueKey.TryRemove(value, out List<TKey> keys))
                {
                    foreach (var item in keys)
                    {
                        var remove2 = _keyValue.TryRemove(item, out List<TValue> values);
                    }
                }
            }
        }

        public void Clear()
        {
            _keyValue.Clear();
            _valueKey.Clear();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _keyValue.GetEnumerator();
        }
    }
}