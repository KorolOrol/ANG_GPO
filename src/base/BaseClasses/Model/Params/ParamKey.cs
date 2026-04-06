using System;
using System.Collections;
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
        
        public override string ToString() => $"{Namespace}:{Name} ({ValueType.Name})";

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