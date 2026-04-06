using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BaseClasses.Interface;

namespace BaseClasses.Model.Params
{
    public class ParamBag
    {
        private const double Tolerance = 1e-9;
        private readonly Dictionary<IParamKey, object?> _params = new Dictionary<IParamKey, object?>();
        [Obsolete("Используйте ParamBag вместо LegacyParamsDictionary для новых разработок.")]
        private readonly Dictionary<string, IParamKey> _legacyKeys = new Dictionary<string, IParamKey>();

        public Dictionary<string, object?> AsDictionary() {
            return _params.ToDictionary(
                kvp => kvp.Key.ToString() 
                       ?? throw new InvalidOperationException("Param key string representation cannot be null."),
                kvp => kvp.Value);
        }
        
        public int Count => _params.Count;
        
        public ICollection<IParamKey> Keys => _params.Keys;
        
        public bool ContainsKey(IParamKey key)
        {
            return _params.ContainsKey(key);
        }

        public bool Remove(IParamKey key) => _params.Remove(key);
        
        public bool TryGetValue(IParamKey key, out object? value) => _params.TryGetValue(key, out value);

        public object? this[IParamKey key]
        {
            get => _params[key];
            set
            {
                if (value != null && key.ValueType != value.GetType())
                    throw new InvalidOperationException($"Value of type {value.GetType().Name} cannot be assigned " +
                                                        $"to key '{key}' with expected type {key.ValueType.Name}.");
                _params[key] = value;
            }
        }

        public void Add(IParamKey key, object value)
        {
            EnsureTypeMatch(key, value);
            _params.Add(key, value);
        }

        [Obsolete("Используйте Set<T>(ParamKey<T> key, T value) вместо SetLegacy для новых разработок.")]
        public void SetLegacy(string key, object? value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Legacy param key cannot be null or whitespace.", nameof(key));

            var valueType = value?.GetType() ?? typeof(object);
            if (_legacyKeys.TryGetValue(key, out var existingKey))
            {
                if (existingKey.ValueType != valueType)
                {
                    _params.Remove(existingKey);
                    existingKey = new LegacyParamKey(key, valueType);
                    _legacyKeys[key] = existingKey;
                }
            }
            else
            {
                existingKey = new LegacyParamKey(key, valueType);
                _legacyKeys[key] = existingKey;
            }

            EnsureTypeMatch(existingKey, value);
            _params[existingKey] = value;
        }

        [Obsolete("Используйте TryGet<T>(ParamKey<T> key, out T value) вместо TryGetLegacy " +
                  "для новых разработок.")]
        public bool TryGetLegacy(string key, out object? value)
        {
            value = null;
            return _legacyKeys.TryGetValue(key, out var legacyKey) &&
                   _params.TryGetValue(legacyKey, out value);
        }

        [Obsolete("Используйте ContainsKey(IParamKey key) вместо ContainsLegacy для новых разработок.")]
        public bool ContainsLegacy(string key)
        {
            return _legacyKeys.ContainsKey(key);
        }

        [Obsolete("Используйте Remove(IParamKey key) вместо RemoveLegacy для новых разработок.")]
        public bool RemoveLegacy(string key)
        {
            if (!_legacyKeys.TryGetValue(key, out var legacyKey))
                return false;
            _legacyKeys.Remove(key);
            return _params.Remove(legacyKey);
        }

        [Obsolete("Используйте Clear() вместо ClearLegacy для новых разработок.")]
        public void ClearLegacy()
        {
            foreach (var legacyKey in _legacyKeys.Values.ToList())
            {
                _params.Remove(legacyKey);
            }
            _legacyKeys.Clear();
        }
        
        public void Clear() => _params.Clear();
        
        public IEnumerable<KeyValuePair<IParamKey, object?>> Enumerate() => _params;

        public void Set<T>(ParamKey<T> key, T value)
        {
            EnsureTypeMatch(key, value);
            _params[key] = value;
        }

        public bool TryGet<T>(ParamKey<T> key, out T value)
        {
            value = default!;
            if (!_params.TryGetValue(key, out object? raw)) return false;

            switch (raw)
            {
                case null:
                    return default(T) is null;
                case T typed:
                    value = typed;
                    return true;
            }

            if (!TryConvertValue(raw, typeof(T), out var converted)) return false;
            value = (T)converted!;
            return true;

        }

        private static void EnsureTypeMatch(IParamKey key, object? value)
        {
            if (value == null) return;
            if (key.ValueType.IsInstanceOfType(value)) return;
            if (TryConvertValue(value, key.ValueType, out _)) return;
            throw new InvalidOperationException($"Value of type {value.GetType().Name} cannot be assigned " +
                                                $"to key '{key}' with expected type {key.ValueType.Name}.");
        }

        private static bool TryConvertValue(object? value, Type targetType, out object? result)
        {
            result = null;
            if (value == null) return true;
            
            var valueType = value.GetType();
            if (!targetType.IsAssignableFrom(valueType))
                return TryConvertPrimitive(value, targetType, out result) ||
                       TryConvertList(value, targetType, out result);
            result = value;
            return true;

        }

        private static bool TryConvertPrimitive(object value, Type targetType, out object? result)
        {
            result = null;
            switch (value)
            {
                case int i when targetType == typeof(double):
                    result = (double)i;
                    return true;
                case double d when targetType == typeof(int):
                    if (Math.Abs(Math.Truncate(d) - d) < Tolerance)
                    {
                        result = (int)d;
                        return true;
                    }
                    break;
            }
            return false;
        }

        private static bool TryConvertList(object value, Type targetType, out object? result)
        {
            result = null;
            
            if (!targetType.IsGenericType || targetType.GetGenericTypeDefinition() != typeof(List<>))
                return false;
            if (!(value is IEnumerable enumerable) || value is string)
                return false;
            
            var elementType = targetType.GetGenericArguments()[0];
            var list = (IList)Activator.CreateInstance(targetType)!;
            foreach (var item in enumerable)
            {
                if (!TryConvertValue(item, elementType, out var convertedItem))
                    return false;
                list.Add(convertedItem);
            }
            result = list;
            return true;
        }

        [Obsolete("Используйте ParamKey<T> вместо LegacyParamKey для новых разработок.")]
        private sealed class LegacyParamKey : IParamKey, IEquatable<LegacyParamKey>
        {
            public LegacyParamKey(string name, Type valueType)
            {
                Name = name;
                ValueType = valueType;
            }

            public string Name { get; }
            public Type ValueType { get; }
            public bool IsCollection => ValueType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(ValueType);
            public string Namespace => "Legacy";

            public bool Equals(LegacyParamKey? other)
            {
                if (other is null) return false;
                return Name == other.Name && ValueType == other.ValueType;
            }

            public override bool Equals(object? obj)
            {
                return Equals(obj as LegacyParamKey);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Name, ValueType);
            }

            public override string ToString() => Name;
        }
    }
}