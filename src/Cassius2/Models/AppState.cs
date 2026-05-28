using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using AIGenerator;
using BaseClasses.Model;
using StructuredNarrative.Model;

namespace Cassius2.Models;

public static class AppState
{
    public static Plot Plot { get; set; } = new();
    
    public static LlmAiGenerator AiGenerator { get; set; } = new();

    public static ProppGenerator ProppGenerator { get; set; } = new();
    
    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
    };

    public static event Action? PlotChanged;
    public static event Action<BaseClasses.Interface.IElement>? EditElementRequested;

    public static void NotifyPlotChanged()
    {
        PlotChanged?.Invoke();
    }

    public static void NotifyEditElementRequested(BaseClasses.Interface.IElement element)
    {
        EditElementRequested?.Invoke(element);
    }
}
