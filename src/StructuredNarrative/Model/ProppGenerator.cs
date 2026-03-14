using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Services;
using StructuredNarrative.Data;

namespace StructuredNarrative.Model;

public class ProppGenerator : IChainGenerator
{
    private static readonly Random _Random = new();
    
    public double SkipProbability { get; set; } = 0.2;

    public Task<IElement> GenerateChainAsync(Plot plot,
        IElement preparedElement,
        Queue<(IElement, IElement, int)> generationQueue = null,
        int recursion = 3)
    {
        ArgumentNullException.ThrowIfNull(preparedElement);
        if (preparedElement.Type != ElemType.Event)
            throw new ArgumentException($"Element {preparedElement.Type} is not an event");
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
    
    private static void FilterFunctionsByRequiredPreviousFunctions(List<ProppFunction> functions, List<ProppFunction> chosenFunctions)
    {
        foreach (var func in functions.ToList().Where(func =>
                     func.RequiredPreviousFunctions.Count != 0 &&
                     func.RequiredPreviousFunctions.All(reqFunc =>
                         chosenFunctions.FirstOrDefault(f => f.Symbol == reqFunc) == null)))
        {
            functions.Remove(func);
        }
    }

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

        var selectedRequiredFunction = availableRequiredFunctions[_Random.Next(availableRequiredFunctions.Count)];
        chosenFunctions.Add(selectedRequiredFunction);
        AddRequiredNextFunctions(selectedRequiredFunction, requiredFunctions);
        RemoveRequiredFunctionsByOrder(requiredFunctions, currentOrder);
        return true;
    }

    private static bool FilterAndChooseFunctionsByRequiredNextFunctions(List<ProppFunction> functions, List<ProppFunction> requiredFunctions, List<ProppFunction> chosenFunctions)
    {
        var foundReqFunctions = functions.FindAll(f
            => requiredFunctions.Any(req => req.Symbol == f.Symbol));

        if (foundReqFunctions.Count > 0)
        {
            var selectedReqFunction = foundReqFunctions[_Random.Next(foundReqFunctions.Count)];
            chosenFunctions.Add(selectedReqFunction);
            AddRequiredNextFunctions(selectedReqFunction, requiredFunctions);
            foreach (var reqFunc in foundReqFunctions)
                requiredFunctions.RemoveAll(f => f.Symbol == reqFunc.Symbol);

            return true;
        }

        return false;
    }

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

    private static void RemoveRequiredFunctionsByOrder(List<ProppFunction> requiredFunctions, int order)
    {
        requiredFunctions.RemoveAll(f => f.Order == order);
    }

    private void ChooseFunction(List<ProppFunction> functions, List<ProppFunction> chosenFunctions, List<ProppFunction> requiredFunctions)
    {
        if (functions.First().IsOptional && _Random.NextDouble() < SkipProbability) return;
        var function = functions[_Random.Next(functions.Count)];
        chosenFunctions.Add(function);
        AddRequiredNextFunctions(function, requiredFunctions);
    }
}