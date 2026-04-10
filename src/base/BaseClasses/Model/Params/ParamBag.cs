using System;
using System.Collections.Generic;
using System.Linq;
using BaseClasses.Interface;

namespace BaseClasses.Model.Params
{
    /// <summary>
    /// Типизированный словарь параметров элемента истории.
    /// </summary>
    public class ParamBag
    {
        /// <summary>
        /// Внутреннее хранилище параметров, обеспечивающее типовую безопасность при добавлении и извлечении значений.
        /// </summary>
        private readonly Dictionary<IParamKey, object?> _params = new Dictionary<IParamKey, object?>();

        /// <summary>
        /// Преобразует содержимое ParamBag в словарь со строковыми ключами и объектными значениями,
        /// пригодный для сериализации или других целей, требующих простых типовых представлений.
        /// </summary>
        /// <returns>Словарь, где ключами являются строковые представления IParamKey,
        /// а значениями - соответствующие объекты.</returns>
        /// <exception cref="InvalidOperationException">Если строковое представление
        /// ключа параметра равно null.</exception>
        public Dictionary<string, object?> AsDictionary() {
            return _params.ToDictionary(
                kvp => kvp.Key.ToString() 
                       ?? throw new InvalidOperationException("Param key string representation cannot be null."),
                kvp => kvp.Value);
        }
        
        /// <summary>
        /// Количество параметров, хранящихся в ParamBag.
        /// </summary>
        public int Count => _params.Count;
        
        /// <summary>
        /// Коллекция ключей параметров, хранящихся в ParamBag.
        /// </summary>
        public ICollection<IParamKey> Keys => _params.Keys;
        
        /// <summary>
        /// Проверяет, содержит ли ParamBag параметр с указанным ключом.
        /// </summary>
        /// <param name="key">Ключ параметра, наличие которого нужно проверить.</param>
        /// <returns>True, если ParamBag содержит параметр с указанным ключом, иначе False.</returns>
        public bool ContainsKey(IParamKey key)
        {
            return _params.ContainsKey(key);
        }

        /// <summary>
        /// Удаляет параметр с указанным ключом из ParamBag.
        /// </summary>
        /// <param name="key">Ключ параметра, который нужно удалить.</param>
        /// <returns>True, если параметр был успешно удален, иначе False.</returns>
        public bool Remove(IParamKey key) => _params.Remove(key);
        
        /// <summary>
        /// Получает значение параметра с указанным ключом.
        /// </summary>
        /// <param name="key">Ключ параметра, значение которого нужно получить.</param>
        /// <param name="value">Параметр, в который будет записано значение, если ключ существует.</param>
        /// <returns>True, если ParamBag содержит параметр с указанным ключом
        /// и значение было успешно получено, иначе False.</returns>
        public bool TryGetValue(IParamKey key, out object? value) => _params.TryGetValue(key, out value);

        /// <summary>
        /// Индексатор для доступа к параметрам по ключу.
        /// </summary>
        /// <param name="key">Ключ параметра, значение которого нужно получить или установить.</param>
        /// <returns>Значение параметра, связанного с указанным ключом.</returns>
        /// <exception cref="InvalidOperationException">Если при установке значения тип нового значения
        /// не соответствует типу, определенному в ключе параметра.</exception>
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

        /// <summary>
        /// Добавляет новый параметр в ParamBag.
        /// </summary>
        /// <param name="key">Ключ параметра, который нужно добавить.</param>
        /// <param name="value">Значение параметра, которое нужно добавить.</param>
        public void Add(IParamKey key, object value)
        {
            key.IsTypeMatch(value);
            _params.Add(key, value);
        }

        /// <summary>
        /// Очищает все параметры из ParamBag, удаляя все ключи и связанные с ними значения.
        /// </summary>
        public void Clear() => _params.Clear();

        /// <summary>
        /// Перечисляет все пары ключ-значение, хранящиеся в ParamBag.
        /// </summary>
        /// <returns>Перечисление всех пар ключ-значение, хранящихся в ParamBag.</returns>
        public IEnumerable<KeyValuePair<IParamKey, object?>> Enumerate() => _params.AsEnumerable();

        /// <summary>
        /// Устанавливает значение параметра с указанным типизированным ключом.
        /// </summary>
        /// <param name="key">Типизированный ключ параметра, значение которого нужно установить.</param>
        /// <param name="value">Значение параметра, которое нужно установить. Должно соответствовать типу,
        /// определенному в ключе параметра.</param>
        /// <typeparam name="T">Тип значения, связанного с ключом параметра.</typeparam>
        public void Set<T>(ParamKey<T> key, T value)
        {
            key.IsTypeMatch(value);
            _params[key] = value;
        }

        /// <summary>
        /// Пытается получить значение параметра с указанным типизированным ключом.
        /// </summary>
        /// <param name="key">Типизированный ключ параметра, значение которого нужно получить.</param>
        /// <param name="value">Параметр, в который будет записано значение,
        /// если ключ существует и значение может быть приведено к типу T.</param>
        /// <typeparam name="T">Тип значения, связанного с ключом параметра.</typeparam>
        /// <returns>True, если ParamBag содержит параметр с указанным ключом и значение было успешно получено
        /// и приведено к типу T, иначе False.</returns>
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

            if (!key.TryConvertValue(raw, out var converted)) return false;
            value = (T)converted!;
            return true;
        }
    }
}