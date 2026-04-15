using System;
using System.Collections.Generic;
using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;

namespace BaseClasses.Services.Binds
{
    /// <summary>
    /// Сервис для связывания элементов истории.
    /// </summary>
    public class Binder
    {
        /// <summary>
        /// Стратегии связывания для каждой комбинации типов элементов и ключа параметра.
        /// </summary>
        private readonly Dictionary<RelationRoute, RelationBindingStrategy> _bindStrategies =
            new Dictionary<RelationRoute, RelationBindingStrategy>();
        
        /// <summary>
        /// Стратегии отвязывания для каждой комбинации типов элементов и ключа параметра.
        /// </summary>
        private readonly Dictionary<RelationRoute, RelationBindingStrategy> _unbindStrategies =
            new Dictionary<RelationRoute, RelationBindingStrategy>();
        
        /// <summary>
        /// Регистрация стратегии связывания и отвязывания для конкретной комбинации типов элементов и ключа параметра.
        /// </summary>
        /// <param name="sourceType">Тип источника.</param>
        /// <param name="targetType">Тип цели.</param>
        /// <param name="paramKey">Ключ параметра.</param>
        /// <param name="bindStrategy">Стратегия связывания.</param>
        /// <param name="unbindStrategy">Стратегия отвязывания.</param>
        public void Register(ElemType sourceType,
            ElemType targetType,
            IParamKey paramKey,
            RelationBindingStrategy bindStrategy,
            RelationBindingStrategy unbindStrategy)
        {
            var route = new RelationRoute(sourceType, targetType, paramKey);
            Register(route, bindStrategy, unbindStrategy);
        }
        
        /// <summary>
        /// Регистрация стратегии связывания и отвязывания для конкретной комбинации типов элементов и ключа параметра.
        /// </summary>
        /// <param name="route">Комбинация типов элементов и ключа параметра.</param>
        /// <param name="bindStrategy">Стратегия связывания.</param>
        /// <param name="unbindStrategy">Стратегия отвязывания.</param>
        /// <exception cref="ArgumentNullException">Если bindStrategy или unbindStrategy равны null.</exception>
        public void Register(RelationRoute route,
            RelationBindingStrategy bindStrategy,
            RelationBindingStrategy unbindStrategy)
        {
            if (bindStrategy is null) throw new ArgumentNullException(nameof(bindStrategy));
            if (unbindStrategy is null) throw new ArgumentNullException(nameof(unbindStrategy));
            _bindStrategies[route] = bindStrategy;
            _unbindStrategies[route] = unbindStrategy;
        }
        
        /// <summary>
        /// Удаление зарегистрированных стратегий связывания и отвязывания
        /// для конкретной комбинации типов элементов и ключа параметра.
        /// </summary>
        /// <param name="sourceType">Тип источника.</param>
        /// <param name="targetType">Тип цели.</param>
        /// <param name="paramKey">Ключ параметра.</param>
        /// <returns>True, если хотя бы одна из стратегий была удалена, иначе False.</returns>
        public bool Unregister(ElemType sourceType, ElemType targetType, IParamKey paramKey)
        {
            var route = new RelationRoute(sourceType, targetType, paramKey);
            return Unregister(route);
        }

        /// <summary>
        /// Удаление зарегистрированных стратегий связывания и отвязывания
        /// для конкретной комбинации типов элементов и ключа параметра.
        /// </summary>
        /// <param name="route">Комбинация типов элементов и ключа параметра.</param>
        /// <returns>True, если хотя бы одна из стратегий была удалена, иначе False.</returns>
        public bool Unregister(RelationRoute route)
        {
            var removedBind = _bindStrategies.Remove(route);
            var removedUnbind = _unbindStrategies.Remove(route);
            return removedBind || removedUnbind;
        }

        /// <summary>
        /// Выполняет связывание между элементами на основе зарегистрированных стратегий для их типов и ключа параметра.
        /// </summary>
        /// <param name="source">Источник связи.</param>
        /// <param name="target">Приемник связи.</param>
        /// <param name="paramKey">Параметр, описывающий связь.</param>
        /// <param name="value">Значение параметра, описывающее связь.</param>
        /// <param name="plot">История, в которой происходит связывание.</param>
        public void Bind(IElement source, IElement target, IParamKey paramKey, object? value, Plot plot)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (!paramKey.IsTypeMatch(value))
                throw new ArgumentException($"Value {value} does not match the type defined in paramKey {paramKey}");
            var route = new RelationRoute(source.Type, target.Type, paramKey);
            if (_bindStrategies.TryGetValue(route, out var strategy))
                strategy(source, target, paramKey, value, plot);
            else
                throw new ArgumentException($"No binding strategy registered for route: {route}");
        }

        /// <summary>
        /// Выполняет отвязывание между элементами на основе зарегистрированных стратегий
        /// для их типов и ключа параметра.
        /// </summary>
        /// <param name="source">Источник связи.</param>
        /// <param name="target">Приемник связи.</param>
        /// <param name="paramKey">Параметр, описывающий связь.</param>
        /// <param name="plot">История, в которой происходит отвязывание.</param>
        public void Unbind(IElement source, IElement target, IParamKey paramKey, Plot plot)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (target is null) throw new ArgumentNullException(nameof(target));
            var route = new RelationRoute(source.Type, target.Type, paramKey);
            if (_unbindStrategies.TryGetValue(route, out var strategy))
            {
                strategy(source, target, paramKey, null, plot);
            }
        }
    }
}
