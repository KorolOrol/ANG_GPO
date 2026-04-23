using System.Text.Json;
using BaseClasses.Model;
using BaseClasses.Interface;
using BaseClasses.Model.Params;
using static ProceduralGenerator.GenerationMethods;

namespace ProceduralGenerator;

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
    
    private List<string> _traitKeys = new List<string>();
    private List<string> _phobiaKeys = new List<string>();
    
    private const int MaxAttempts = 1000;

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
                futureTraits = GenChaoticRandom();
                break;
            case LogicRandom:
                futureTraits = GenLogicRandom();
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
            _traitKeys = _traits.Keys.ToList();
        }

        if (rootElement.TryGetProperty("phobias", out var phobiasElement))
        {
            _phobias = ParseTraits(phobiasElement);
            _phobiaKeys = _phobias.Keys.ToList();
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
                var descString = desc.GetString();
                if (!string.IsNullOrEmpty(descString))
                {
                    traitValue.Add(descString);
                }
            }
            
            if (traitValue.Count > 0)
            {
                output.Add(traitName, traitValue);
            }
        }

        return output;
    }

    private static int[,] ParseTable(JsonElement tableElement)
    {
        var rows = tableElement.GetArrayLength();
        var cols = rows > 0 ? tableElement[0].GetArrayLength() : 0;
        
        var output = new int[rows, cols];

        for (var i = 0; i < rows; i++)
        {
            var row = tableElement[i];
            for (var j = 0; j < cols; j++)
            {
                output[i, j] = row[j].GetInt32();
            }
        }
        
        return output;
    }

    private List<Dictionary<string, object>> GenChaoticRandom()
    {
        var possibleTraits = DeepClone(_traits);
        var possibleKeys = possibleTraits.Keys.ToList();
        var futureTraits = new List<Dictionary<string, object>>();
        
        var traitsToGenerate = Math.Min(NumberOfTraits, possibleKeys.Count);
        
        for (int i = 0; i < traitsToGenerate; i++)
        {
            var randomIndex = Random.Next(possibleKeys.Count);
            var traitKey = possibleKeys[randomIndex];
            
            futureTraits.Add(BuildTrait(possibleTraits, traitKey));
            possibleTraits.Remove(traitKey);
            possibleKeys.RemoveAt(randomIndex);
        }

        return futureTraits;
    }
    
    private List<Dictionary<string, object>> GenLogicRandom()
    {
        var futureTraits = new List<Dictionary<string, object>>();
        var attempts = 0;
        
        while (futureTraits.Count < NumberOfTraits && attempts < MaxAttempts)
        {
            var traitKey = _traitKeys[Random.Next(_traitKeys.Count)];

            if (futureTraits.Count == 0 || CheckTraitTable(futureTraits, traitKey))
            {
                futureTraits.Add(BuildTrait(_traits, traitKey));
                attempts = 0;
            }
            
            attempts++;
            
            if (attempts >= MaxAttempts && futureTraits.Count < NumberOfTraits)
            {
                var fallbackKey = _traitKeys.FirstOrDefault(k => futureTraits.All(t => t["Title"] as string != Capitalize(k)));
                
                if (fallbackKey != null)
                {
                    futureTraits.Add(BuildTrait(_traits, fallbackKey));
                }
                attempts = 0;
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
            return GenTwoParentsHalf(combinedTraits);
        }
        else
        {
            var availableParentTraits = new List<Dictionary<string, object>>(combinedTraits);
            var attempts = 0;
            
            for (int i = 0; i < combinedTraits.Count / 2 && futureTraits.Count < NumberOfTraits && attempts < MaxAttempts; i++)
            {
                var randomIndex = Random.Next(availableParentTraits.Count);
                var selectedTrait = availableParentTraits[randomIndex];
                var newTraitKey = selectedTrait["Title"] as string;
                
                if (!string.IsNullOrEmpty(newTraitKey) && 
                    (futureTraits.Count == 0 || CheckTraitTable(futureTraits, newTraitKey)))
                {
                    futureTraits.Add(BuildTrait(_traits, newTraitKey));
                    availableParentTraits.RemoveAt(randomIndex);
                    attempts = 0;
                }
                else
                {
                    i--;
                    attempts++;
                }
            }

            while (futureTraits.Count < NumberOfTraits)
            {
                var traitKey = _traitKeys[Random.Next(_traitKeys.Count)];
                if (futureTraits.All(t => t["Title"] as string != Capitalize(traitKey)))
                {
                    futureTraits.Add(BuildTrait(_traits, traitKey));
                }
            }

            while (futureTraits.Count > NumberOfTraits)
            {
                futureTraits.RemoveAt(Random.Next(futureTraits.Count));
            }
        }
        return futureTraits;
    }
    
    private List<Dictionary<string, object>> GenTwoParentsHalf(Plot plot, IElement preparedElement)
    {
        var combinedTraits = GetParentsTraits(plot, preparedElement);
        return GenTwoParentsHalf(combinedTraits);
    }
    
    private List<Dictionary<string, object>> GenTwoParentsHalf(List<Dictionary<string, object>> traitList)
    {
        var futureTraits = new List<Dictionary<string, object>>();
        var availableTraits = new List<Dictionary<string, object>>(traitList);
        var attempts = 0;

        var traitsToInherit = traitList.Count / 2;
        
        while (futureTraits.Count < traitsToInherit && availableTraits.Count > 0 && attempts < MaxAttempts)
        {
            var randomIndex = Random.Next(availableTraits.Count);
            var selectedTrait = availableTraits[randomIndex];
            
            if (selectedTrait["Title"] is string newTraitKey &&
                (futureTraits.Count == 0 || CheckTraitTable(futureTraits, newTraitKey)))
            {
                if (futureTraits.All(t => t["Title"] as string != Capitalize(newTraitKey)))
                {
                    futureTraits.Add(BuildTrait(_traits, newTraitKey));
                    attempts = 0;
                }
                availableTraits.RemoveAt(randomIndex);
            }
            else
            {
                attempts++;
            }
        }
        
        return futureTraits;
    }
    
    private List<Dictionary<string, object>> GenTwoParentsLogicRandom(Plot plot, IElement preparedElement)
    {
        var combinedTraits = GetParentsTraits(plot, preparedElement);
        combinedTraits = combinedTraits.OrderByDescending(GetAffectionValue).ToList();

        var futureTraits = new List<Dictionary<string, object>>();
        var availableTraits = new List<Dictionary<string, object>>(combinedTraits);
        var attempts = 0;
        
        while (availableTraits.Count > 0 && futureTraits.Count < NumberOfTraits && attempts < MaxAttempts)
        {
            var probability = Random.NextDouble();
            var selectedTrait = availableTraits[0];
            
            if (selectedTrait["Title"] is string newTraitKey &&
                (futureTraits.Count == 0 || CheckTraitTable(futureTraits, newTraitKey)) && 
                probability <= 0.85)
            {
                if (futureTraits.All(t => t["Title"] as string != Capitalize(newTraitKey)))
                {
                    futureTraits.Add(BuildTrait(_traits, newTraitKey));
                    attempts = 0;
                }
            }
            
            availableTraits.RemoveAt(0);
            attempts++;
        }

        while (futureTraits.Count < NumberOfTraits)
        {
            var traitKey = _traitKeys[Random.Next(_traitKeys.Count)];

            if (futureTraits.Count == 0 || CheckTraitTable(futureTraits, traitKey))
            {
                if (futureTraits.All(t => t["Title"] as string != Capitalize(traitKey)))
                {
                    futureTraits.Add(BuildTrait(_traits, traitKey));
                }
            }
        }
        
        return futureTraits;
    }
    
    private List<Dictionary<string, object>> GenTwoParentsLogic(Plot plot, IElement preparedElement)
    {
        var combinedTraits = GetParentsTraits(plot, preparedElement);
        combinedTraits = combinedTraits.OrderByDescending(GetAffectionValue).ToList();

        var futureTraits = new List<Dictionary<string, object>>();
        var availableTraits = new List<Dictionary<string, object>>(combinedTraits);
        
        while (availableTraits.Count > 0 && futureTraits.Count < NumberOfTraits)
        {
            var probability = Random.NextDouble();
            var selectedTrait = availableTraits[0];
            
            if (selectedTrait["Title"] is string newTraitKey &&
                (futureTraits.Count == 0 || CheckTraitTable(futureTraits, newTraitKey)) && 
                probability <= 0.85)
            {
                if (futureTraits.All(t => t["Title"] as string != Capitalize(newTraitKey)))
                {
                    futureTraits.Add(BuildTrait(_traits, newTraitKey));
                }
            }
            
            availableTraits.RemoveAt(0);
        }
        
        return futureTraits;
    }
    
    private List<Dictionary<string, object>> GenPhobias()
    {
        var futurePhobias = new List<Dictionary<string, object>>();
        var availablePhobias = new List<string>(_phobiaKeys);
        
        for (var i = 0; i < Math.Min(NumberOfPhobias, availablePhobias.Count); i++)
        {
            var randomIndex = Random.Next(availablePhobias.Count);
            var phobiaKey = availablePhobias[randomIndex];
            
            futurePhobias.Add(BuildTrait(_phobias, phobiaKey));
            availablePhobias.RemoveAt(randomIndex);
        }
        
        return futurePhobias;
    }

    private static Dictionary<string, object> BuildTrait(Dictionary<string, List<string>> possibleTraits, string traitKey)
    {
        var descriptions = possibleTraits[traitKey];
        var description = descriptions.Count > 0 ? descriptions[Random.Next(descriptions.Count)] : string.Empty;
        
        return new Dictionary<string, object>()
        {
            { "Title", Capitalize(traitKey) },
            { "Affection", Math.Round(Random.NextDouble(), 3) },
            { "Description", description },
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
    
    private double GetAffectionValue(Dictionary<string, object> trait)
    {
        if (trait.TryGetValue("Affection", out var value))
        {
            try
            {
                return Convert.ToDouble(value);
            }
            catch
            {
                return 0.0;
            }
        }
        return 0.0;
    }
    
    private bool CheckTraitTable(List<Dictionary<string, object>> traitList, string newTraitKey)
    {
        if (_traitTable == null) return true;
        
        var newTraitIndex = _traitKeys.IndexOf(newTraitKey);
        if (newTraitIndex == -1) return true;
        
        foreach (var trait in traitList)
        {
            var existingTraitTitle = trait["Title"] as string;
            if (string.IsNullOrEmpty(existingTraitTitle)) continue;
            
            var existingTraitKey = existingTraitTitle.ToLower();
            var existingTraitIndex = _traitKeys.IndexOf(existingTraitKey);
            
            if (existingTraitIndex == -1) continue;
            
            // Проверяем, не выходим ли за границы таблицы
            if (existingTraitIndex < _traitTable.GetLength(0) && 
                newTraitIndex < _traitTable.GetLength(1))
            {
                if (_traitTable[existingTraitIndex, newTraitIndex] == 0)
                {
                    return false;
                }
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

        return parents.Count switch
        {
            < 2 => throw new ArgumentException("Character doesn't have one or both parent elements"),
            > 2 => throw new ArgumentException("Character have more then two parent elements"),
            _ => parents
        };
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