using AIGenerator;
using BaseClasses.Enum;
using BaseClasses.Model;
using StructuredNarrative.Data;
using StructuredNarrative.Model;
using Xunit.Abstractions;

namespace StructuredNarrative.Tests;

public class ProppGeneratorTests(ITestOutputHelper testOutputHelper)
{
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
        await generator.GenerateChainAsync(plot, new Element(ElemType.Event));
        testOutputHelper.WriteLine(plot.FullInfo());
    }

    [Fact]
    public async Task GenerationWithSkipping()
    {
        Plot plot = new();
        ProppFunctionRegistry.Load("/home/korolorol/src/ANG_GPO/src/StructuredNarrative/Data/Propp.jsonl");
        ProppGenerator generator = new() { SkipProbability = 0.8 };
        foreach (var role in ProppFunctionRegistry.Roles)
        {
            var roledCharacter = new Element(ElemType.Character, role);
            roledCharacter.Params.Add("ProppRole", role);
            plot.Add(roledCharacter);
        }
        await generator.GenerateChainAsync(plot, new Element(ElemType.Event));
        testOutputHelper.WriteLine(plot.FullInfo());
    }
    
    [Fact]
    public async Task GenerationWithAiUpgrade()
    {
        Plot plot = new();
        ProppFunctionRegistry.Load("/home/korolorol/src/ANG_GPO/src/StructuredNarrative/Data/Propp.jsonl");
        ProppGenerator proppGenerator = new() { SkipProbability = 0.8 };
        foreach (var role in ProppFunctionRegistry.Roles)
        {
            var roledCharacter = new Element(ElemType.Character, role);
            roledCharacter.Params.Add("ProppRole", role);
            plot.Add(roledCharacter);
        }
        await proppGenerator.GenerateChainAsync(plot, new Element(ElemType.Event));
        testOutputHelper.WriteLine(plot.FullInfo());
        var llmAiGenerator =
            new LlmAiGenerator("/home/korolorol/src/ANG_GPO/src/AI/AIGenerator/SystemPromptExample.json")
                {
                    TextAiGenerator =
                    {
                        Endpoint = "http://127.0.0.1:1234/v1",
                        Model = "qwen3.5-4b"
                    },
                    AIPriority = true,
                    UseStructuredOutput = true
                };
        for (int i = 0; i < plot.Elements.Count; i++)
        {
            try
            {
                var element = plot.Elements[i];
                await llmAiGenerator.GenerateAsync(plot, element);
                testOutputHelper.WriteLine($"Upgraded {i} from {plot.Elements.Count}:\n" + element.FullInfo());
            }
            catch (Exception ex)
            {
                testOutputHelper.WriteLine($"Error upgrading element {i}: {ex.Message}");
                i--;
            }
        }
    }
}
