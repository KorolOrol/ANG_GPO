using AIGenerator;
using BaseClasses.Enum;
using BaseClasses.Model;
using StructuredNarrative.Data;
using StructuredNarrative.Model;
using Xunit;
using Xunit.Abstractions;
using System;
using System.IO;

namespace StructuredNarrative.Tests;

public class ProppGeneratorTests(ITestOutputHelper testOutputHelper)
{
    private static string GetProppJsonlPath()
    {
        return Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "Data",
                "Propp.jsonl"));
    }

    private static string GetSystemPromptExamplePath()
    {
        return Path.GetFullPath(
            Path.Combine(
                AppContext.BaseDirectory,
                "..",
                "..",
                "..",
                "..",
                "..",
                "AI",
                "AIGenerator",
                "SystemPromptExample.json"));
    }

    [Fact]
    public async Task FullGeneration()
    {
        Plot plot = new();
        ProppFunctionRegistry.Load(GetProppJsonlPath());
        ProppGenerator generator = new() { SkipProbability = 0 };
        foreach (var role in ProppFunctionRegistry.Roles)
        {
            var roledCharacter = new Element(ElemType.Character, role);
            roledCharacter.Params.Add("ProppRole", role);
            plot.Add(roledCharacter);
        }

        var initialCount = plot.Elements.Count;

        await generator.GenerateChainAsync(plot, new Element(ElemType.Event), recursion:31);

        // Basic deterministic assertions: generation should not remove elements and should
        // produce at least one event element when no skipping is allowed.
        Assert.True(plot.Elements.Count >= initialCount);
        Assert.Contains(plot.Elements, e => e.Type == ElemType.Event);

        testOutputHelper.WriteLine(plot.FullInfo());
    }

    [Fact]
    public async Task GenerationWithSkipping()
    {
        Plot plot = new();
        ProppFunctionRegistry.Load(GetProppJsonlPath());
        ProppGenerator generator = new() { SkipProbability = 0.8 };
        foreach (var role in ProppFunctionRegistry.Roles)
        {
            var roledCharacter = new Element(ElemType.Character, role);
            roledCharacter.Params.Add("ProppRole", role);
            plot.Add(roledCharacter);
        }

        var initialCount = plot.Elements.Count;

        await generator.GenerateChainAsync(plot, new Element(ElemType.Event), recursion:31);

        // With skipping enabled, we at least expect generation not to remove elements.
        Assert.True(plot.Elements.Count >= initialCount);

        testOutputHelper.WriteLine(plot.FullInfo());
    }
    
    [Fact]
    public async Task GenerationWithAiUpgrade()
    {
        Plot plot = new();
        ProppFunctionRegistry.Load(GetProppJsonlPath());
        ProppGenerator proppGenerator = new() { SkipProbability = 0.8 };
        // foreach (var role in ProppFunctionRegistry.Roles)
        // {
        //     var roledCharacter = new Element(ElemType.Character, role);
        //     roledCharacter.Params.Add("ProppRole", role);
        //     plot.Add(roledCharacter);
        // }
        await proppGenerator.GenerateChainAsync(plot, new Element(ElemType.Event), recursion:10);
        testOutputHelper.WriteLine(plot.FullInfo());

        var llmAiGenerator =
            new LlmAiGenerator(GetSystemPromptExamplePath())
            {
                // TextAiGenerator =
                // {
                //     Endpoint = "https://neuroapi.host/v1",
                //     Model = "gpt-5-nano"
                // },
                TextAiGenerator =
                {
                    Endpoint = "http://localhost:1234/v1",
                    Model = "qwen3.5-4b"
                },
                AIPriority = true,
                UseStructuredOutput = true
            };
        // ((OpenAIGenerator)llmAiGenerator.TextAiGenerator).GetApiKeyFromEnvironment("NeuroApiKey");

        var countBeforeUpgrade = plot.Elements.Count;

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

        // Upgrading elements in place should not change the number of plot elements.
        Assert.Equal(countBeforeUpgrade, plot.Elements.Count);
    }
}
