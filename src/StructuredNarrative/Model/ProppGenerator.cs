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
        var requiredFunctions = new List<string>();
        var chosenFunctions = new List<ProppFunction>();
        for (var i = from; i <= to; i++)
        {
            var functions = ProppFunctionRegistry.ByOrder(i);
            FilterFunctionsByRequiredPreviousFunctions(functions, chosenFunctions);
            if (functions.Count == 0) continue;
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

    private static bool FilterAndChooseFunctionsByRequiredNextFunctions(List<ProppFunction> functions, List<string> requiredFunctions, List<ProppFunction> chosenFunctions)
    {
        var foundReqFunctions = functions.FindAll(f
            => requiredFunctions.Contains(f.Symbol));

        if (foundReqFunctions.Count > 0)
        {
            var selectedReqFunction = foundReqFunctions[_Random.Next(foundReqFunctions.Count)];
            chosenFunctions.Add(selectedReqFunction);
            requiredFunctions.AddRange(selectedReqFunction.RequiredNextFunctions);
            foreach (var reqFunc in foundReqFunctions)
            {
                requiredFunctions.Remove(reqFunc.Symbol);
            }

            return true;
        }

        return false;
    }

    private void ChooseFunction(List<ProppFunction> functions, List<ProppFunction> chosenFunctions, List<string> requiredFunctions)
    {
        if (functions.First().IsOptional && _Random.NextDouble() < SkipProbability) return;
        var function = functions[_Random.Next(functions.Count)];
        chosenFunctions.Add(function);
        requiredFunctions.AddRange(function.RequiredNextFunctions);
    }
}