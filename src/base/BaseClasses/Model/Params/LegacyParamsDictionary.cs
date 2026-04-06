using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BaseClasses.Model.Params
{
    /// <summary>
    /// Legacy-словарь параметров, который синхронизирует изменения в ParamBag.
    /// </summary>
    [Obsolete("Используйте ParamBag вместо LegacyParamsDictionary для новых разработок.")]
    public sealed class LegacyParamsDictionary : IDictionary<string, object>
    {
        private readonly Dictionary<string, object> _inner;
        private readonly ParamBag _typedParams;

        public LegacyParamsDictionary(ParamBag typedParams, IDictionary<string, object>? source = null)
        {
            _typedParams = typedParams ?? throw new ArgumentNullException(nameof(typedParams));
            _inner = source?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? new Dictionary<string, object>();

            foreach (var kvp in _inner)
            {
                _typedParams.SetLegacy(kvp.Key, kvp.Value);
            }
        }

        public object this[string key]
        {
            get => _inner[key];
            set
            {
                _inner[key] = value;
                _typedParams.SetLegacy(key, value);
            }
        }

        public ICollection<string> Keys => _inner.Keys;

        public ICollection<object> Values => _inner.Values;

        public int Count => _inner.Count;

        public bool IsReadOnly => false;

        public void Add(string key, object value)
        {
            _inner.Add(key, value);
            _typedParams.SetLegacy(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _inner.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            var removed = _inner.Remove(key);
            if (removed)
            {
                _typedParams.RemoveLegacy(key);
            }
            return removed;
        }

        public bool TryGetValue(string key, out object value)
        {
            var found = _inner.TryGetValue(key, out var raw);
            value = raw;
            return found;
        }

        public void Add(KeyValuePair<string, object> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _inner.Clear();
            _typedParams.ClearLegacy();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return ((ICollection<KeyValuePair<string, object>>)_inner).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, object>>)_inner).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            var removed = ((ICollection<KeyValuePair<string, object>>)_inner).Remove(item);
            if (removed)
            {
                _typedParams.RemoveLegacy(item.Key);
            }
            return removed;
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _inner.GetEnumerator();
        }
    }
}


