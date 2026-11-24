using System;
using System.Collections;
using System.Collections.Generic;
using BaseClasses.Interface;

namespace BaseClasses.Model.Params
{
    /// <summary>
    /// Типизированный ключ параметра, который может быть использован для хранения и извлечения параметров
    /// с определенным типом значения.
    /// </summary>
    /// <typeparam name="T">Тип значения, связанного с этим ключом параметра.</typeparam>
    public sealed class ParamKey<T> : IParamKey, IEquatable<ParamKey<T>>
    {
        private const double Tolerance = 1e-9;
        public string Name { get; }
        public Type ValueType => typeof(T);
        public bool IsCollection => typeof(T) != typeof(string) &&
                                    typeof(IEnumerable).IsAssignableFrom(typeof(T));
        public string Namespace { get; }
        
        /// <summary>
        /// Конструктор для создания нового ключа параметра с заданным именем и пространством имен.
        /// </summary>
        /// <param name="name">Название ключа параметра. Не может быть null или состоять только из пробелов.</param>
        /// <param name="namespace">Пространство имен для организации ключей параметров. По умолчанию "base".</param>
        /// <exception cref="ArgumentException">Выбрасывается, если имя null или состоит только из пробелов.</exception>
        public ParamKey(string name, string @namespace = "Base")
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));
            Name = name;
            Namespace = @namespace;
        }
        
        public bool IsTypeMatch(object? value)
        {
            if (value == null) return true;
            return ValueType.IsInstanceOfType(value) || TryConvertValue(value, out _);
        }
        
        public bool TryConvertValue(object? value, out object? result)
        {
            result = null;
            if (value == null) return true;
            
            var inputType = value.GetType();
            if (!ValueType.IsAssignableFrom(inputType))
                return TryConvertPrimitive(value, out result) ||
                       TryConvertList(value, out result);
            result = value;
            return true;

        }

        /// <summary>
        /// Пытается выполнить неявное преобразование между примитивными типами (например, int и double).
        /// </summary>
        /// <param name="value">Значение, которое нужно попытаться преобразовать.</param>
        /// <param name="result">Параметр, в который будет записан результат преобразования, если оно успешно.</param>
        /// <returns>True, если преобразование было успешным, иначе False.</returns>
        private bool TryConvertPrimitive(object value, out object? result)
        {
            result = null;
            switch (value)
            {
                case int i when ValueType == typeof(double):
                    result = (double)i;
                    return true;
                case double d when ValueType == typeof(int):
                    if (Math.Abs(Math.Round(d) - d) < Tolerance)
                    {
                        result = (int)Math.Round(d);
                        return true;
                    }
                    break;
            }
            return false;
        }

        /// <summary>
        /// Пытается преобразовать перечисление в список, если тип значения является коллекцией.
        /// </summary>
        /// <param name="value">Значение, которое нужно попытаться преобразовать.</param>
        /// <param name="result">Параметр, в который будет записан результат преобразования, если оно успешно.</param>
        /// <returns>True, если преобразование было успешным, иначе False.</returns>
        private bool TryConvertList(object value, out object? result)
        {
            result = null;
            
            if (!ValueType.IsGenericType || ValueType.GetGenericTypeDefinition() != typeof(List<>))
                return false;
            if (!(value is IEnumerable enumerable) || value is string)
                return false;
            
            var elementType = ValueType.GetGenericArguments()[0];
            var listItemParamKey = (IParamKey)Activator.CreateInstance(typeof(ParamKey<>)
                .MakeGenericType(elementType), "temp", Namespace)!;
            var list = (IList)Activator.CreateInstance(ValueType)!;
            foreach (var item in enumerable)
            {
                if (!listItemParamKey.TryConvertValue(item, out var convertedItem))
                    return false;
                list.Add(convertedItem);
            }
            result = list;
            return true;
        }
        
        public override string ToString() => $"{Namespace}.{Name} ({ValueType.Name})";

        public override bool Equals(object? obj)
        {
            return Equals(obj as ParamKey<T>);
        }

        public bool Equals(ParamKey<T>? other)
        {
            if (other is null) return false;
            return Name == other.Name && Namespace == other.Namespace && ValueType == other.ValueType;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Namespace, ValueType);
        }
    }
}