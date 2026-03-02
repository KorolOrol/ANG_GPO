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
            foreach (var func in functions.ToList().Where(func =>
                         func.RequiredPreviousFunctions.Count != 0 &&
                         func.RequiredPreviousFunctions.All(reqFunc =>
                             chosenFunctions.FirstOrDefault(f => f.Symbol == reqFunc) == null)))
            {
                functions.Remove(func);
            }
            if (functions.Count == 0) continue;
            if (requiredFunctions.Count > 0)
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

                    continue;
                }
            }

            if (functions.First().IsOptional && _Random.NextDouble() < SkipProbability) continue;
            var function = functions[_Random.Next(functions.Count)];
            chosenFunctions.Add(function);
            requiredFunctions.AddRange(function.RequiredNextFunctions);
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
}