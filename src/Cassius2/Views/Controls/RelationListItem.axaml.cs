using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Text.Json;
using Cassius2.Models;
using Cassius2.ViewModels.Controls;
using Cassius2.Views.Windows;

namespace Cassius2.Views.Controls;

public partial class RelationListItem : UserControl
{
    public RelationListItem()
    {
        InitializeComponent();
    }

    private async void EditButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is not RelationListItemViewModel vm || vm.RelationData == null) return;
            if (TopLevel.GetTopLevel(this) is not Window window) return;
            
            var editWindow = new Windows.RelationEditWindow();
            editWindow.SetupForEdit(vm.RelationData.Target, vm.RelationData.Param, vm.Value != null ? JsonSerializer.Serialize(vm.Value, AppState.JsonOptions) : "null");
            
            bool result = await editWindow.ShowDialog<bool>(window);
            if (!result) return;
            vm.NotifyEdited(editWindow.ParsedValue);
            vm.Setup(new BaseClasses.Model.Relation(vm.RelationData.Source, vm.RelationData.Target, vm.RelationData.Param, editWindow.ParsedValue));
        }
        catch (Exception exception)
        {
            var exceptionWindow = new ExceptionWindow();
            exceptionWindow.LoadException(exception);
            await exceptionWindow.ShowDialog<bool>((TopLevel.GetTopLevel(this) as Window)!);
        }
    }
}
