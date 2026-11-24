using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Cassius2.ViewModels;
using Cassius2.Views.Windows;

namespace Cassius2.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OpenPlotMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;
        
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Choose file with plot",
                AllowMultiple = false
            });

            if (files.Count >= 1 && DataContext is MainWindowViewModel vm)
            {
                vm.OpenPlot(files[0].Path.LocalPath);
            }
        }
        catch (Exception exception)
        {
            var exceptionWindow = new ExceptionWindow();
            exceptionWindow.LoadException(exception);
            await exceptionWindow.ShowDialog<bool>((TopLevel.GetTopLevel(this) as Window)!);
        }
    }

    private async void SavePlotMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;
        
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Save file with plot",
                DefaultExtension = "json",
                SuggestedFileName = "plot.json"
            });

            if (file != null && DataContext is MainWindowViewModel vm)
            {
                vm.SavePlot(file.Path.LocalPath);
            }
        }
        catch (Exception exception)
        {
            var exceptionWindow = new ExceptionWindow();
            exceptionWindow.LoadException(exception);
            await exceptionWindow.ShowDialog<bool>((TopLevel.GetTopLevel(this) as Window)!);
        }
    }

    private void ExitMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void ExportJsonPlotMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;
        
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Export plot to JSON",
                DefaultExtension = "json",
                SuggestedFileName = "plot.json"
            });

            if (file != null && DataContext is MainWindowViewModel vm)
            {
                vm.ExportJsonPlot(file.Path.LocalPath);
            }
        }
        catch (Exception exception)
        {
            var exceptionWindow = new ExceptionWindow();
            exceptionWindow.LoadException(exception);
            await exceptionWindow.ShowDialog<bool>((TopLevel.GetTopLevel(this) as Window)!);
        }
    }

    private async void ExportTextPlotMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;
        
            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Export plot to plain text",
                DefaultExtension = "txt",
                SuggestedFileName = "plot.txt"
            });

            if (file != null && DataContext is MainWindowViewModel vm)
            {
                vm.ExportTextPlot(file.Path.LocalPath);
            }
        }
        catch (Exception exception)
        {
            var exceptionWindow = new ExceptionWindow();
            exceptionWindow.LoadException(exception);
            await exceptionWindow.ShowDialog<bool>((TopLevel.GetTopLevel(this) as Window)!);
        }
    }

    private async void AboutMenuItem_OnClick(object? sender, RoutedEventArgs e)
    {
        var aboutWindow = new AboutWindow();
        await aboutWindow.ShowDialog<bool>((TopLevel.GetTopLevel(this) as Window)!);
    }
}