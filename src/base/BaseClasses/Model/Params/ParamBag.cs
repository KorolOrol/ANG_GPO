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
    }
}