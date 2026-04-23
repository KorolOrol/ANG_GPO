using System.Text.Json;
using BaseClasses.Model;
using BaseClasses.Interface;
using BaseClasses.Model.Params;
using static Procedural_v2.GenerationMethods;

namespace Procedural_v2;

public class ProceduralGenerator : IGenerator
{
    #region static fields

    private static Random Random => Random.Shared;

    #endregion

    #region readonly fields

    private readonly ParamKey<List<Dictionary<string, object>>> _traitParamKey = new("Traits", "Procedural");
    private readonly ParamKey<List<Dictionary<string, object>>> _phobiasParamKey = new("Phobias", "Procedural");
    
    #endregion

    #region private fields

    private Dictionary<string, List<string>> _traits = new Dictionary<string, List<string>>();
    private Dictionary<string, List<string>> _phobias = new Dictionary<string, List<string>>();
    private int[,]? _traitTable = null;
    private int[,]? _relatedTraitTable = null;

    #endregion

    #region public fields

    public GenerationMethods GenerationMethod { get; set; } = LogicRandom;
    public int NumberOfTraits { get; private set; }
    public int NumberOfPhobias { get; private set; }

    #endregion

    #region constructors

    public ProceduralGenerator(int numberOfPossibleTraits, int numberOfPossiblePhobias, string optionsPath)
    {
        NumberOfTraits = numberOfPossibleTraits;
        NumberOfPhobias = numberOfPossiblePhobias;
        SetOptions(optionsPath);
    }

    #endregion

    #region public methods

    public Task<IElement> GenerateAsync(Plot plot, IElement preparedElement)
    {
        return Task.FromResult(Generate(plot, preparedElement));
    }

    public IElement Generate(Plot plot, IElement preparedElement)
    {
        List<Dictionary<string, object>> futureTraits;
        switch (GenerationMethod)
        {
            case ChaoticRandom:
                futureTraits = GenChaoticRandom(plot, preparedElement);
                break;
            case LogicRandom:
                futureTraits = GenLogicRandom(plot, preparedElement);
                break;
            case TwoParentsHalfRandom:
                futureTraits = GenTwoParentsHalfRandom(plot, preparedElement);
                break;
            case TwoParentsHalf:
                futureTraits = GenTwoParentsHalf(plot, preparedElement);
                break;
            case TwoParentsLogicRandom:
                futureTraits = GenTwoParentsLogicRandom(plot, preparedElement);
                break;
            case TwoParentsLogic:
                futureTraits = GenTwoParentsLogic(plot, preparedElement);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        preparedElement.Params.Set(_traitParamKey, futureTraits);
        preparedElement.Params.Set(_phobiasParamKey, GenPhobias());
        return preparedElement;
    }

    #endregion

    #region private methods

    private void SetOptions(string optionsPath)
    {
        var json = File.ReadAllText(optionsPath);
        var options = JsonDocument.Parse(json);
        var rootElement = options.RootElement;
        
        if (rootElement.TryGetProperty("traits", out var traitsElement))
        {
            _traits = ParseTraits(traitsElement);
        }

        if (rootElement.TryGetProperty("phobias", out var phobiasElement))
        {
            _phobias = ParseTraits(phobiasElement);
        }

        if (rootElement.TryGetProperty("traitTable", out var traitTableElement))
        {
            _traitTable = ParseTable(traitTableElement);
        }

        if (rootElement.TryGetProperty("relatedTraitTable", out var relatedTraitTable))
        {
            _relatedTraitTable = ParseTable(relatedTraitTable);
        }
    }

    private static Dictionary<string, List<string>> ParseTraits(JsonElement traitsElement)
    {
        var output = new Dictionary<string, List<string>>();
        foreach (var traitElement in traitsElement.EnumerateObject())
        {
            var traitName = traitElement.Name;
            var traitDescriptions = traitElement.Value;

            var traitValue = new List<string>();
            foreach (var desc in traitDescriptions.EnumerateArray())
            {
                traitValue.Add(desc.GetString());
            }
            
            output.Add(traitName, traitValue);
        }

        return output;
    }

    private static int[,] ParseTable(JsonElement tableElement)
    {
        var rows = tableElement.GetArrayLength();
        var cols = tableElement.GetArrayLength();
        
        var output = new int[rows, cols];

        for (var i = 0; i < rows; i++)
        {
            var row = tableElement[i];
            for (var j = 0; j < cols; j++)
            {
                output[i, j] = row.GetInt32();
            }
        }
        
        return output;
    }

    private List<Dictionary<string, object>> GenChaoticRandom(Plot plot, IElement preparedElement)
    {
        if (preparedElement == null) throw new ArgumentNullException(nameof(preparedElement));

        var possibleTraits = DeepClone(_traits);
        var futureTraits = new List<Dictionary<string, object>>();
            
        for (int i = 0; i < NumberOfTraits; i++)
        {
            var traitKey = possibleTraits.Keys.ToList()[Random.Next(possibleTraits.Count)];
            
            futureTraits.Add(BuildTrait(possibleTraits, traitKey));
            possibleTraits.Remove(traitKey);
        }

        return futureTraits;
    }
    
    private List<Dictionary<string, object>> GenLogicRandom(Plot plot, IElement preparedElement)
    {
        if (preparedElement == null) throw new ArgumentNullException(nameof(preparedElement));
        
        var futureTraits = new List<Dictionary<string, object>>();
        
        for (int i = 0; i < NumberOfTraits; i++)
        {
            var traitKey = _traits.Keys.ToList()[Random.Next(_traits.Count)];

            if (futureTraits.Count == 0 || CheckTraitTable(futureTraits, traitKey))
            {
                futureTraits.Add(BuildTrait(_traits, traitKey));
            }
            else
            {
                i--;
            }
        }
        return futureTraits;
    }
    
    private List<Dictionary<string, object>> GenTwoParentsHalfRandom(Plot plot, IElement preparedElement)
    {
        var combinedTraits = GetParentsTraits(plot, preparedElement);
        
        var traitsCount = combinedTraits.Count / 2;

        var futureTraits = new List<Dictionary<string, object>>();
        if (traitsCount == NumberOfTraits)
        {
            GenTwoParentsHalf(plot, preparedElement, combinedTraits);
        }
        else
        {
            for (int i = 0; i < combinedTraits.Count / 2; i++)
            {
                var newTraitKey = combinedTraits[Random.Next(combinedTraits.Count)]["Title"] as string;
                if (futureTraits.Count == 0 || CheckTraitTable(futureTraits, newTraitKey))
                {
                    futureTraits.Add(BuildTrait(_traits, newTraitKey));
                }
                else
                {
                    i--;
                }
            }

            if (futureTraits.Count > NumberOfTraits)
            {
                futureTraits.RemoveAt(Random.Next(futureTraits.Count));
            }
            else
            {
                for (var i = futureTraits.Count; i < NumberOfTraits; i++)
                {
                    futureTraits.Add(BuildTrait(_traits, _traits.Keys.ToList()[Random.Next(_traits.Count)]));
                }
            }
        }
        return futureTraits;
    }
    
    private List<Dictionary<string, object>> GenTwoParentsHalf(Plot plot, IElement preparedElement)
    {
        var combinedTraits = GetParentsTraits(plot, preparedElement);
        
        return GenTwoParentsHalf(plot, preparedElement, combinedTraits);
    }
    
    private List<Dictionary<string, object>> GenTwoParentsHalf(Plot plot, IElement preparedElement, List<Dictionary<string, object>> traitList)
    {
        var futureTraits = new List<Dictionary<string, object>>();
        
        for (var i = 0; i < traitList.Count / 2; i++)
        {
            if (traitList.ElementAt(Random.Next(traitList.Count))["Title"] is string newTraitKey &&
                (futureTraits.Count == 0 || CheckTraitTable(futureTraits, newTraitKey)))
            {
                futureTraits.Add(BuildTrait(_traits, newTraitKey));
            }
            else
            {
                i--;
            }
        }
        return futureTraits;
    }
    
    private List<Dictionary<string, object>> GenTwoParentsLogicRandom(Plot plot, IElement preparedElement)
    {
        var combinedTraits = GetParentsTraits(plot, preparedElement);
        combinedTraits = combinedTraits.OrderBy(x => Convert.ToDouble(x["Affection"])).ToList();

        var futureTraits = new List<Dictionary<string, object>>();
        for (var i = 0; combinedTraits.Count != 0 && i < combinedTraits.Count / 2; i++)
        {
            if (futureTraits.Count != NumberOfTraits)
            {
                var probability = Math.Round(Random.NextDouble(), 3);
                if (combinedTraits[i]["Title"] is string newTraitKey &&
                    (futureTraits.Count == 0 || CheckTraitTable(futureTraits, newTraitKey)) && 
                    probability <= 0.85d)
                {
                    futureTraits.Add(BuildTrait(_traits, newTraitKey));
                }
                else
                {
                    combinedTraits.RemoveAt(i);
                    i--;
                }
            }
        }

        for (var i = futureTraits.Count; i < NumberOfTraits; i++)
        {
            var traitKey = _traits.Keys.ToList()[Random.Next(_traits.Count)];

            if (futureTraits.Count == 0 || CheckTraitTable(futureTraits, traitKey))
            {
                futureTraits.Add(BuildTrait(_traits, traitKey));
            }
            else
            {
                i--;
            }
        }
        return futureTraits;
    }
    
    private List<Dictionary<string, object>> GenTwoParentsLogic(Plot plot, IElement preparedElement)
    {
        var combinedTraits = GetParentsTraits(plot, preparedElement);
        combinedTraits = combinedTraits.OrderBy(x => Convert.ToDouble(x["Affection"])).ToList();

        var futureTraits = new List<Dictionary<string, object>>();
        for (var i = 0; i < combinedTraits.Count / 2; i++)
        {
            var probability = Math.Round(Random.NextDouble(), 3);
            if (combinedTraits[i]["Title"] is string newTraitKey &&
                (futureTraits.Count == 0 || CheckTraitTable(futureTraits, newTraitKey)) && 
                probability <= 0.85d)
            {
                futureTraits.Add(BuildTrait(_traits, newTraitKey));
            }
            else
            {
                combinedTraits.RemoveAt(i);
                i--;
            }
        }
        return futureTraits;
    }
    
    private List<Dictionary<string, object>> GenPhobias()
    {
        var futurePhobias = new List<Dictionary<string, object>>();
        for (var i = 0; i < NumberOfPhobias; i++)
        {
            futurePhobias.Add(BuildTrait(_phobias, _phobias.Keys.ToList()[Random.Next(_phobias.Count)]));
        }
        return futurePhobias;
    }

    private static Dictionary<string, object> BuildTrait(Dictionary<string, List<string>> possibleTraits, string traitKey)
    {
        return new Dictionary<string, object>()
        {
            { "Title", Capitalize(traitKey) },
            { "Affection", Math.Round(Random.NextDouble(), 3) },
            { "Description", possibleTraits[traitKey][Random.Next(possibleTraits[traitKey].Count)] },
        };
    }
    
    private static T? DeepClone<T>(T obj)
    {
        var json = JsonSerializer.Serialize(obj);
        return JsonSerializer.Deserialize<T>(json);
    }
    
    private static string Capitalize(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
    
        return char.ToUpper(input[0]) + input[1..].ToLower();
    }
    
    private bool CheckTraitTable(List<Dictionary<string, object>> traitList, string newTraitKey)
    {
        int column = _traits.Keys.ToList().IndexOf(newTraitKey);
        
        foreach (var trait in traitList)
        {
            int row = _traits.Keys.ToList().IndexOf((trait["Title"] as string)?.ToLower());
            if (_traitTable[row, column] == 0)
            {
                return false;
            }
        }
        
        return true;
    }

    private List<Dictionary<string, object>> GetTraitsFromElement(IElement element)
    {
        if (element.Params.ContainsKey(_traitParamKey) && 
            element.Params.TryGet<List<Dictionary<string, object>>>(_traitParamKey, out var traitList))
        {
            return traitList;
        }
        else
        {
            throw new ArgumentException("Element doesn't have traits parameter.");
        }
    }
    
    private List<IElement> GetParents(Plot plot, IElement child)
    {
        var parents = plot.Relations.Where(x => Equals(x.Target, child) &&
                                                x.Param.Name.ToLower() == "child")
            .ToList()
            .Select(x => x.Source)
            .ToList();
        
        if (parents.Count < 2)
        {
            throw new ArgumentException("Character doesn't have one or both parent elements");
        }
        else if (parents.Count > 2)
        {
            throw new  ArgumentException("Character have more then two parent elements");
        }
        return parents;
    }

    private List<Dictionary<string, object>> GetParentsTraits(Plot plot, IElement child)
    {
        var parents = GetParents(plot, child);

        var firstTraitList = GetTraitsFromElement(parents[0]);
        var secondTraitList = GetTraitsFromElement(parents[1]);

        List<Dictionary<string, object>> combinedTraits = new List<Dictionary<string, object>>();
        combinedTraits.AddRange(firstTraitList);
        combinedTraits.AddRange(secondTraitList);
        
        return combinedTraits;
    }

    #endregion
}