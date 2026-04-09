using System.Text.Json;
using BaseClasses.Model;
using BaseClasses.Interface;
using static Procedural_v2.GenerationMethods;

namespace Procedural_v2;

public class ProceduralGenerator: IGenerator
{
    private Dictionary<string, List<string>> _traits = new Dictionary<string, List<string>>();
    private Dictionary<string, List<string>> _phobias = new Dictionary<string, List<string>>();
    private int[,]? _traitTable = null;
    private int[,]? _relatedTraitTable = null;
    private static readonly Random _random = new Random();
    
    private SelectedMethod? _selectedMethod;
    private delegate void SelectedMethod(params object[] parameters);
    
    public int MaxTraits { get; private set; }
    public int MaxPhobias { get; private set; }
    
    public ProceduralGenerator(int maxPossibleTraits, int maxPossiblePhobias, string optionsPath)
    {
        MaxTraits = maxPossibleTraits;
        MaxPhobias = maxPossiblePhobias;
        SetOptions(optionsPath);
    }

    public void SetGenerationMethod(GenerationMethods method)
    {
        switch (method) 
        {
         case ChaoticRandom:
             _selectedMethod = GenChaoticRandom;
             break;
         case LogicRandom:
             _selectedMethod = GenLogicRandom;
             break;
         case TwoParentsHalfRandom:
             _selectedMethod = GenTwoParentsHalfRandom;
             break;
         case TwoParentsHalf:
             _selectedMethod = GenTwoParentsHalfRandom;
             break;
         case TwoParentsLogicRandom:
             _selectedMethod = GenTwoParentsLogicRandom;
             break;
         case TwoParentsLogic:
             _selectedMethod = GenTwoParentsLogic;
             break;
         case InputTraits:
             _selectedMethod = GenInputTraits;
             break;
         default:
             throw new ArgumentOutOfRangeException(nameof(method), method, null);
        }
    }
    
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

    public Task<IElement> GenerateAsync(Plot plot, IElement preparedElement)
    {
        return null;
    }

    private void GenChaoticRandom(params object[] parameters)
    {
        IElement? element = parameters[0] as IElement;
        if (element == null) throw new ArgumentNullException(nameof(element));
        
        int? traitsCount = parameters[1] as int?;
        if (traitsCount == null || traitsCount > MaxTraits) traitsCount = MaxTraits;

        var possibleTraits = DeepClone(_traits);
        var futureTraits = new List<Dictionary<string, object>>();
            
        for (int i = 0; i < traitsCount; i++)
        {
            var traitKey = possibleTraits.Keys.ToList()[_random.Next(possibleTraits.Count)];
            
            futureTraits.Add(BuildTrait(possibleTraits, traitKey));
            possibleTraits.Remove(traitKey);
        }
        // TODO: add traitlist save
    }
    
    private void GenLogicRandom(params object[] parameters)
    {
        IElement? element = parameters[0] as IElement;
        if (element == null) throw new ArgumentNullException(nameof(element));
        
        int? traitsCount = parameters[1] as int?;
        if (traitsCount == null || traitsCount > MaxTraits) traitsCount = MaxTraits;
        
        var futureTraits = new List<Dictionary<string, object>>();
        
        for (int i = 0; i < traitsCount; i++)
        {
            var traitKey = _traits.Keys.ToList()[_random.Next(_traits.Count)];

            if (futureTraits.Count == 0 || CheckTraitTable(futureTraits, traitKey))
            {
                futureTraits.Add(BuildTrait(_traits, traitKey));
            }
            else
            {
                i--;
            }
        }
        // TODO: add traitlist save
    }
    private void GenTwoParentsHalfRandom(params object[] parameters)
    {
        throw new NotImplementedException();
    }
    private void GenTwoParentsHalf(params object[] parameters)
    {
        throw new NotImplementedException();
    }
    private void GenTwoParentsLogicRandom(params object[] parameters)
    {
        throw new NotImplementedException();
    }
    private void GenTwoParentsLogic(params object[] parameters)
    {
        throw new NotImplementedException();
    }
    private void GenInputTraits(params object[] parameters)
    {
        throw new NotImplementedException();
    }
    private void GenPhobias(params object[] parameters)
    {
        throw new NotImplementedException();
    }

    private static Dictionary<string, object> BuildTrait(Dictionary<string, List<string>> possibleTraits,
        string traitKey)
    {
        return new Dictionary<string, object>()
        {
            { "Title", Capitalize(traitKey) },
            { "Affection", Math.Round(_random.NextDouble(), 3) },
            { "Description", possibleTraits[traitKey][_random.Next(possibleTraits[traitKey].Count)] },
        };
    }
    
    private static T DeepClone<T>(T obj)
    {
        string json = JsonSerializer.Serialize(obj);
        return JsonSerializer.Deserialize<T>(json);
    }
    
    private static string Capitalize(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;
    
        return char.ToUpper(input[0]) + input[1..].ToLower();
    }
    
    private bool CheckTraitTable(List<Dictionary<string, object>> trait_list, string newTraitKey)
    {
        int column = _traits.Keys.ToList().IndexOf(newTraitKey);
        
        foreach (var trait in trait_list)
        {
            int row = _traits.Keys.ToList().IndexOf((trait["Title"] as string).ToLower());
            if (_traitTable[row, column] == 0)
            {
                return false;
            }
        }
        
        return true;
    }
}