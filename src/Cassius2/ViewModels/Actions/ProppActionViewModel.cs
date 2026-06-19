using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;
using Cassius2.Models;
using Cassius2.Views.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StructuredNarrative.Data;

namespace Cassius2.ViewModels.Actions;

public partial class ProppActionViewModel : ViewModelBase
{
    [ObservableProperty] private double _skipProbability;
    [ObservableProperty] private int _recursion = 10;
    [ObservableProperty] private string _loadedData = "";
    [ObservableProperty] private List<IElement> _generatedElements = [];

    public void LoadData(string path)
    {
        ProppFunctionRegistry.Load(path);
        LoadedData = $"Loaded {ProppFunctionRegistry.All.Count} functions " +
                     $"and {ProppFunctionRegistry.Roles.Count} roles from {path}";
    }

    [RelayCommand]
    public void Generate()
    {
        try
        {

            var oldElements = AppState.Plot.Elements.ToHashSet();
            AppState.ProppGenerator.SkipProbability = SkipProbability;
            AppState.ProppGenerator.GenerateChain(AppState.Plot, new Element(ElemType.Event), Recursion);
            GeneratedElements = AppState.Plot.Elements.Except(oldElements).ToList();
            AppState.NotifyPlotChanged();
        }
        catch (Exception exception)
        {
            var exceptionWindow = new ExceptionWindow();
            exceptionWindow.LoadException(exception);
            exceptionWindow.ShowDialog<bool>((TopLevel.GetTopLevel(
                Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow
                    : null) as Window)!);
        }
    }
}
