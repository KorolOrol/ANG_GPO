using System;
using Avalonia.Controls;
using Cassius2.Views.Windows;

namespace Cassius2.Views.Actions;

public partial class AiAction : UserControl
{
    public AiAction()
    {
        InitializeComponent();
    }

    private async void BtnLoadPrompt_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;
        
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
            {
                Title = "Выберите файл с промптами",
                AllowMultiple = false
            });

            if (files.Count >= 1 && DataContext is ViewModels.Actions.AiActionViewModel vm)
            {
                vm.LoadPrompt(files[0].Path.LocalPath);
            }
        }
        catch (Exception exception)
        {
            var exceptionWindow = new ExceptionWindow();
            exceptionWindow.LoadException(exception);
            await exceptionWindow.ShowDialog<bool>((TopLevel.GetTopLevel(this) as Window)!);
        }
    }
}