using BaseClasses.Enum;
using BaseClasses.Model;
using StructuredNarrative.Data;
using StructuredNarrative.Model;
using Xunit.Abstractions;

namespace StructuredNarrative.Tests;

public class ProppGeneratorTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ProppGeneratorTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task FullGeneration()
    {
        Plot plot = new();
        ProppFunctionRegistry.Load("/home/korolorol/src/ANG_GPO/src/StructuredNarrative/Data/Propp.jsonl");
        ProppGenerator generator = new() { SkipProbability = 0 };
        foreach (var role in ProppFunctionRegistry.Roles)
        {
            var roledCharacter = new Element(ElemType.Character, role);
            roledCharacter.Params.Add("ProppRole", role);
            plot.Add(roledCharacter);
        }
        var startEvent = (Element)await generator.GenerateChainAsync(plot, new Element(ElemType.Event));
        _testOutputHelper.WriteLine(plot.FullInfo());
    }
}
