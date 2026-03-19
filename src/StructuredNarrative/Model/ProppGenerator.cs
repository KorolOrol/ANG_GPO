using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Services;
using StructuredNarrative.Data;

namespace StructuredNarrative.Model
{
    public class ProppGenerator : IChainGenerator
    {
        /// <summary>
        /// Генератор случайных чисел для выбора функций и принятия решений о пропуске опциональных функций.
        /// </summary>
        private static readonly ThreadLocal<Random> _Random = new ThreadLocal<Random>(() => new Random());
        
        /// <summary>
        /// Вероятность пропуска опциональной функции Проппа при генерации цепочки.
        /// </summary>
        public double SkipProbability { get; set; } = 0.2;

        /// <summary>
        /// Генерация цепочки событий на основе функций Проппа.
        /// </summary>
        /// <param name="plot">Сюжет</param>
        /// <param name="preparedElement">Начальное событие,
        /// к которому будет добавляться цепочка функций Проппа. Должно быть типа Event.</param>
        /// <param name="generationQueue">Внутренняя очередь для рекурсивной генерации,
        /// обычно не передаётся при первом вызове.</param>
        /// <param name="recursion">Максимальное количество функций Проппа в цепочке (глубина рекурсии).</param>
        /// <returns>Элемент с добавленной цепочкой событий по функциям Проппа.</returns>
        /// <exception cref="ArgumentException">Если preparedElement не является событием.</exception>
        public Task<IElement> GenerateChainAsync(Plot plot,
            IElement preparedElement,
            Queue<(IElement, IElement, int)> generationQueue = null,
            int recursion = 3)
        {
            if (plot == null)
                throw new ArgumentNullException(nameof(plot));

            if (preparedElement == null) 
                throw new ArgumentNullException(nameof(preparedElement));
            if (preparedElement.Type != ElemType.Event)
                throw new ArgumentException($"Element {preparedElement.Type} is not an event");
            if (ProppFunctionRegistry.All.Count == 0)
                throw new InvalidOperationException("ProppFunctionRegistry is empty. Load functions before generating.");
            var from = ProppFunctionRegistry.All.Min(f => f.Order);
            var to = ProppFunctionRegistry.All.Max(f => f.Order);
            var requiredFunctions = new List<ProppFunction>();
            var chosenFunctions = new List<ProppFunction>();
            for (var i = from; i <= to; i++)
            {
                if (chosenFunctions.Count == recursion) break;
                var functions = ProppFunctionRegistry.ByOrder(i);
                FilterFunctionsByRequiredPreviousFunctions(functions, chosenFunctions);
                if (functions.Count == 0) continue;

                // Если среди обязательных есть функции текущего порядка, выбирать нужно только из них.
                if (TryChooseRequiredFunctionsByCurrentOrder(functions, requiredFunctions, chosenFunctions, i))
                    continue;

                if (requiredFunctions.Count > 0 &&
                    FilterAndChooseFunctionsByRequiredNextFunctions(functions, requiredFunctions, chosenFunctions)) 
                    continue;

                ChooseFunction(functions, chosenFunctions, requiredFunctions);
            }

            var firstFunction = chosenFunctions.FirstOrDefault();
            if (firstFunction != null)
            {
                var functionEvent = firstFunction.CreateEventSkeleton(plot);
                Merger.Merge(preparedElement, functionEvent, false);
                plot.Add(preparedElement);
            }

            foreach (var function in chosenFunctions.Skip(1))
            {
                var functionEvent = function.CreateEventSkeleton(plot);
                plot.Add(functionEvent);
            }

            return Task.FromResult(preparedElement);
        }
    
        /// <summary>
        /// Удаляет из списка функций те, которые имеют в RequiredPreviousFunctions функции, не входящие в chosenFunctions.
        /// </summary>
        /// <param name="functions">Список функций для фильтрации.</param>
        /// <param name="chosenFunctions">Список уже выбранных функций, которые могут быть необходимы для текущих.</param>
        private static void FilterFunctionsByRequiredPreviousFunctions(List<ProppFunction> functions,
            List<ProppFunction> chosenFunctions)
        {
            foreach (var func in functions.ToList().Where(func =>
                         func.RequiredPreviousFunctions.Count != 0 &&
                         func.RequiredPreviousFunctions.All(reqFunc =>
                             chosenFunctions.FirstOrDefault(f => f.Symbol == reqFunc) == null)))
            {
                functions.Remove(func);
            }
        }

        /// <summary>
        /// Если среди обязательных функций есть функции текущего порядка, выбирает случайную из них,
        /// добавляет её в цепочку и обновляет список обязательных функций.
        /// </summary>
        /// <param name="functions">Список доступных функций текущего порядка.</param>
        /// <param name="requiredFunctions">Список обязательных функций, которые должны быть включены в цепочку.</param>
        /// <param name="chosenFunctions">Список уже выбранных функций, к которому будет добавлена выбранная функция.</param>
        /// <param name="currentOrder">Порядковый номер функций, которые нужно проверить на обязательность.</param>
        /// <returns></returns>
        private static bool TryChooseRequiredFunctionsByCurrentOrder(
            List<ProppFunction> functions,
            List<ProppFunction> requiredFunctions,
            List<ProppFunction> chosenFunctions,
            int currentOrder)
        {
            var requiredCurrentOrder = requiredFunctions
                .Where(f => f.Order == currentOrder)
                .ToList();

            if (requiredCurrentOrder.Count == 0)
                return false;

            var availableRequiredFunctions = functions
                .Where(f => requiredCurrentOrder.Any(req => req.Symbol == f.Symbol))
                .ToList();

            if (availableRequiredFunctions.Count == 0)
                return false;

            var selectedRequiredFunction = availableRequiredFunctions[_Random.Value!.Next(availableRequiredFunctions.Count)];
            chosenFunctions.Add(selectedRequiredFunction);
            AddRequiredNextFunctions(selectedRequiredFunction, requiredFunctions);
            RemoveRequiredFunctionsByOrder(requiredFunctions, currentOrder);
            return true;
        }

        /// <summary>
        /// Если среди доступных функций есть те, которые являются обязательными для уже выбранных функций
        /// (т.е. входят в их RequiredNextFunctions), выбирает случайную из них,
        /// добавляет её в цепочку и обновляет список обязательных функций.
        /// </summary>
        /// <param name="functions">Список доступных функций текущего порядка.</param>
        /// <param name="requiredFunctions">Список обязательных функций, которые должны быть включены в цепочку.</param>
        /// <param name="chosenFunctions">Список уже выбранных функций, к которому будет добавлена выбранная функция.</param>
        /// <returns></returns>
        private static bool FilterAndChooseFunctionsByRequiredNextFunctions(List<ProppFunction> functions,
            List<ProppFunction> requiredFunctions, List<ProppFunction> chosenFunctions)
        {
            var foundReqFunctions = functions.FindAll(f
                => requiredFunctions.Any(req => req.Symbol == f.Symbol));

            if (foundReqFunctions.Count > 0)
            {
                var selectedReqFunction = foundReqFunctions[_Random.Value!.Next(foundReqFunctions.Count)];
                chosenFunctions.Add(selectedReqFunction);
                AddRequiredNextFunctions(selectedReqFunction, requiredFunctions);
                foreach (var reqFunc in foundReqFunctions)
                    requiredFunctions.RemoveAll(f => f.Symbol == reqFunc.Symbol);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Добавляет в список обязательных функций функции,
        /// символы которых указаны в RequiredNextFunctions у переданной функции.
        /// </summary>
        /// <param name="function">Функция, у которой нужно проверить RequiredNextFunctions.</param>
        /// <param name="requiredFunctions">Список обязательных функций, в который будут добавлены найденные функции.</param>
        private static void AddRequiredNextFunctions(ProppFunction function, List<ProppFunction> requiredFunctions)
        {
            foreach (var requiredSymbol in function.RequiredNextFunctions)
            {
                var requiredFunction = ProppFunctionRegistry.BySymbol(requiredSymbol);
                if (requiredFunction == null) continue;
                if (requiredFunctions.All(f => f.Symbol != requiredFunction.Symbol))
                    requiredFunctions.Add(requiredFunction);
            }
        }

        /// <summary>
        /// Удаляет из списка обязательных функций все функции с указанным порядковым номером.
        /// </summary>
        /// <param name="requiredFunctions">Список обязательных функций,
        /// из которого будут удалены функции с указанным порядковым номером.</param>
        /// <param name="order">Порядковый номер функций, которые нужно удалить из списка обязательных.</param>
        private static void RemoveRequiredFunctionsByOrder(List<ProppFunction> requiredFunctions, int order)
        {
            requiredFunctions.RemoveAll(f => f.Order == order);
        }

        /// <summary>
        /// Выбирает случайную функцию из списка доступных функций,
        /// добавляет её в цепочку и обновляет список обязательных функций.
        /// </summary>
        /// <param name="functions">Список доступных функций текущего порядка,
        /// из которых будет выбрана функция для добавления в цепочку.</param>
        /// <param name="chosenFunctions">Список уже выбранных функций, к которому будет добавлена выбранная функция.</param>
        /// <param name="requiredFunctions">Список обязательных функций,
        /// который будет обновлён на основе RequiredNextFunctions у выбранной функции.</param>
        private void ChooseFunction(List<ProppFunction> functions, List<ProppFunction> chosenFunctions,
            List<ProppFunction> requiredFunctions)
        {
            if (functions.First().IsOptional && _Random.Value!.NextDouble() < SkipProbability) return;
            var function = functions[_Random.Value!.Next(functions.Count)];
            chosenFunctions.Add(function);
            AddRequiredNextFunctions(function, requiredFunctions);
        }
    }
}