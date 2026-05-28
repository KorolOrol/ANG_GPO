using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using AIGenerator;
using BaseClasses.Model;

namespace Cassius2.Models;

public static class AppState
{
    public static Plot Plot { get; set; } = new Plot();
    
    public static LlmAiGenerator AiGenerator { get; set; } = new LlmAiGenerator();
    
    public static JsonSerializerOptions JsonOptions { get; } = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
    };

    public static event Action? PlotChanged;

    public static void NotifyPlotChanged()
    {
        PlotChanged?.Invoke();
    }
}
