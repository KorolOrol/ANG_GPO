using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using BaseClasses.Model;
using BaseClasses.Services;
using Cassius2.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Cassius2.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isViewActionVisible = true;

    [ObservableProperty]
    private bool _isAiActionVisible;

    [ObservableProperty]
    private bool _isProppActionVisible;

    public MainWindowViewModel()
    {
        AppState.EditElementRequested += (element) => ShowViewAction();
    }

    [RelayCommand]
    private void ShowViewAction()
    {
        IsViewActionVisible = true;
        IsAiActionVisible = false;
        IsProppActionVisible = false;
    }

    [RelayCommand]
    private void ShowAiAction()
    {
        IsViewActionVisible = false;
        IsAiActionVisible = true;
        IsProppActionVisible = false;
    }

    [RelayCommand]
    private void ShowProppAction()
    {
        IsViewActionVisible = false;
        IsAiActionVisible = false;
        IsProppActionVisible = true;
    }

    [RelayCommand]
    public void NewPlot()
    {
        AppState.Plot = new();
        AppState.NotifyPlotChanged();
    }

    public void OpenPlot(string path)
    {
        var plot = Serializer.Deserialize<Plot>(path);
        if (plot != null)
        {
            AppState.Plot = plot;
            AppState.NotifyPlotChanged();
        }
    }

    public void SavePlot(string path)
    {
        var options = new JsonSerializerOptions(AppState.JsonOptions)
        {
            WriteIndented = false,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            Converters = { new ParamBagJsonConverter(), new ElementJsonConverter() }
        };
        Serializer.Options = options;
        Serializer.Serialize(AppState.Plot, path);
    }
    
    public void ExportJsonPlot(string path)
    {
        var options = new JsonSerializerOptions(AppState.JsonOptions)
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            Converters = { new ParamBagJsonConverter(), new ElementJsonConverter() }
        };
        Serializer.Options = options;
        Serializer.Serialize(AppState.Plot, path);
    }

    public void ExportTextPlot(string path)
    {
        Serializer.Print(AppState.Plot, path);
    }
}
