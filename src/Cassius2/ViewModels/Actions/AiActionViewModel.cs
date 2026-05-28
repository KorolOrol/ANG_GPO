using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AIGenerator.TextGenerator;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;
using Cassius2.Models;
using Cassius2.ViewModels.Controls;
using Cassius2.Views.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Cassius2.ViewModels.Actions;

public class GeneratorWrapper : ObservableObject
{
    public OpenAiGenerator Generator { get; }
    
    public bool UseEnvVar { get; set; } = false;
    public string EnvVarName { get; set; } = "OpenAIAPIKey";

    public GeneratorWrapper(OpenAiGenerator generator)
    {
        Generator = generator;
    }

    public string DisplayName => $"{Generator.Model} @ {Generator.Endpoint}";

    public void RefreshDisplayName()
    {
        OnPropertyChanged(nameof(DisplayName));
    }
}

public partial class AiActionViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<GeneratorWrapper> _generators = new();

    [ObservableProperty]
    private GeneratorWrapper? _selectedGenerator;

    [ObservableProperty] private string _editApiKey = "";
    [ObservableProperty] private string _editEndpoint = "";
    [ObservableProperty] private string _editModel = "";
    [ObservableProperty] private int _editMaxCompletionTokens = 8192;
    [ObservableProperty] private int _editSeed = -1;
    [ObservableProperty] private bool _editUseEnvVar = false;
    [ObservableProperty] private string _editEnvVarName = "OpenAIAPIKey";
    [ObservableProperty] private string _loadedPrompt = "";

    partial void OnSelectedGeneratorChanged(GeneratorWrapper? value)
    {
        if (value == null) return;
        var gen = value.Generator;
        EditUseEnvVar = value.UseEnvVar;
        EditEnvVarName = value.EnvVarName;
        EditApiKey = "";
        EditEndpoint = gen.Endpoint;
        EditModel = gen.Model;
        EditMaxCompletionTokens = gen.MaxCompletionTokens;
        EditSeed = gen.Seed;
    }

    [RelayCommand]
    private void AddGenerator()
    {
        var gen = new OpenAiGenerator { Model = "New Model", Endpoint = "http://localhost" };
        var wrapper = new GeneratorWrapper(gen);
        Generators.Add(wrapper);
        SelectedGenerator = wrapper;
    }

    [RelayCommand]
    private void RemoveGenerator()
    {
        if (SelectedGenerator != null && Generators.Count > 1)
        {
            Generators.Remove(SelectedGenerator);
            var first = Generators.FirstOrDefault();
            SelectedGenerator = first;
        }
    }

    [RelayCommand]
    private void UpdateGenerator()
    {
        if (SelectedGenerator != null)
        {
            var wrapper = SelectedGenerator;
            var gen = wrapper.Generator;

            wrapper.UseEnvVar = EditUseEnvVar;
            wrapper.EnvVarName = EditEnvVarName;

            if (wrapper.UseEnvVar)
            {
                if (!string.IsNullOrEmpty(EditEnvVarName))
                    gen.GetApiKeyFromEnvironment(EditEnvVarName);
            }
            else
            {
                if (!string.IsNullOrEmpty(EditApiKey))
                    gen.ApiKey = EditApiKey;
            }

            gen.Endpoint = EditEndpoint;
            gen.Model = EditModel;
            gen.MaxCompletionTokens = EditMaxCompletionTokens;
            gen.Seed = EditSeed;
            wrapper.RefreshDisplayName();
        }
    }

    [ObservableProperty]
    private bool _isNewElement = true;

    [ObservableProperty]
    private bool _isExistingElement;

    [ObservableProperty]
    private bool _isSingleGeneration = true;

    [ObservableProperty]
    private bool _isChainGeneration;

    [ObservableProperty]
    private ObservableCollection<IElement> _existingElements = new();

    [ObservableProperty]
    private IElement? _selectedExistingElement;

    [ObservableProperty]
    private ObservableCollection<ElemType> _elementTypes = new((ElemType[])Enum.GetValues(typeof(ElemType)));

    [ObservableProperty]
    private ElemType _selectedElementType = ElemType.Character;

    [ObservableProperty]
    private ElementEditorViewModel _editorViewModel = new();

    [ObservableProperty]
    private string _generationStatus = "";

    public AiActionViewModel()
    {
        Generators.Add(new GeneratorWrapper(new OpenAiGenerator 
        { 
            Model = "gpt-3.5-turbo",
            Endpoint = "https://api.openai.com/v1/chat/completions"
        }));
        Generators.Add(new GeneratorWrapper(new OpenAiGenerator 
        { 
            Model = "gpt-4",
            Endpoint = "https://api.openai.com/v1/chat/completions"
        }));
        Generators.Add(new GeneratorWrapper(new OpenAiGenerator 
        { 
            Model = "local-model",
            Endpoint = "http://localhost:1234/v1/chat/completions"
        }));
        SelectedGenerator = Generators.First();

        LoadExistingElements();
        AppState.PlotChanged += LoadExistingElements;
    }

    private void LoadExistingElements()
    {
        ExistingElements.Clear();
        foreach (var el in AppState.Plot.Elements)
        {
            ExistingElements.Add(el);
        }
        SelectedExistingElement = ExistingElements.FirstOrDefault();
    }

    public void LoadPrompt(string path)
    {
        try
        {
            AppState.AiGenerator.LoadPromptTemplates(path);
            LoadedPrompt = "Loaded prompt: " + System.IO.Path.GetFileName(path);
        }
        catch (Exception exception)
        {
            var exceptionWindow = new ExceptionWindow();
            exceptionWindow.LoadException(exception);
            exceptionWindow.ShowDialog<bool>((TopLevel.GetTopLevel(
                Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow
                    : null) as Window)!);
             LoadedPrompt = "Loading prompt exception: " + exception.Message;
        }
    }

    [RelayCommand]
    private async Task GenerateAsync()
    {
        if (SelectedGenerator == null) return;

        if (IsExistingElement && SelectedExistingElement == null)
        {
            GenerationStatus = "Error: No existing element selected.";
            return;
        }

        AppState.AiGenerator.TextAiGenerator = SelectedGenerator.Generator;
        GenerationStatus = "Generation...";

        try
        {
            var baseElement = IsExistingElement ? SelectedExistingElement : new Element(SelectedElementType);

            IElement newElement;

            if (IsChainGeneration)
            {
                newElement = await AppState.AiGenerator.GenerateChainAsync(AppState.Plot, baseElement!);
            }
            else
            {
                newElement = await AppState.AiGenerator.GenerateAsync(AppState.Plot, baseElement!);
            }

            if (newElement is Element el)
            {
                EditorViewModel.LoadElement(el);
            }
            AppState.NotifyPlotChanged();
            GenerationStatus = "Generation completed!";
        }
        catch (Exception exception)
        {
            var exceptionWindow = new ExceptionWindow();
            exceptionWindow.LoadException(exception);
            await exceptionWindow.ShowDialog<bool>((TopLevel.GetTopLevel(
                Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow
                    : null) as Window)!);
            GenerationStatus = "Generation error: " + exception.Message;
        }
    }
}
