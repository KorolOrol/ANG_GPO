using System;
using System.Text.Json;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Cassius2.Models;
using Cassius2.ViewModels.Controls;
using Cassius2.Views.Windows;

namespace Cassius2.Views.Controls;

public partial class ParamListItem : UserControl
{
    public ParamListItem()
    {
        InitializeComponent();
    }

    private async void EditButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is not ParamListItemViewModel vm || vm.Key == null) return;
            if (TopLevel.GetTopLevel(this) is not Window window) return;
            
            var editWindow = new ParamEditWindow();
            editWindow.SetupForEdit(vm.Key,
                vm.Value != null ? JsonSerializer.Serialize(vm.Value, AppState.JsonOptions) : "null");
            
            bool result = await editWindow.ShowDialog<bool>(window);
            if (!result) return;
            vm.NotifyEdited(editWindow.ParsedValue);
            vm.Setup(vm.Key, editWindow.ParsedValue);
        }
        catch (Exception exception)
        {
            var exceptionWindow = new ExceptionWindow();
            exceptionWindow.LoadException(exception);
            await exceptionWindow.ShowDialog<bool>((TopLevel.GetTopLevel(this) as Window)!);
        }
    }
}