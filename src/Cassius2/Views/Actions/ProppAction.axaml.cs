using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Cassius2.ViewModels.Actions;
using Cassius2.Views.Windows;

namespace Cassius2.Views.Actions;

public partial class ProppAction : UserControl
{
    public ProppAction()
    {
        InitializeComponent();
    }
    
    private async void BtnLoadData_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel == null) return;
        
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Выберите файл с промптами",
                AllowMultiple = false
            });

            if (files.Count >= 1 && DataContext is ProppActionViewModel vm)
            {
                vm.LoadData(files[0].Path.LocalPath);
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