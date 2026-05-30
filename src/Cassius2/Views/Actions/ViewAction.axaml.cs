using System;
using Avalonia;
using Avalonia.Controls;
using BaseClasses.Interface;
using Cassius2.Models;
using Cassius2.ViewModels.Actions;
using Cassius2.Views.Windows;

namespace Cassius2.Views.Actions;

public partial class ViewAction : UserControl
{
    private bool _isHandlingSelection;

    public ViewAction()
    {
        InitializeComponent();
        EditorControl.ElementUpdated += EditorControl_ElementUpdated;
    }

    private void EditorControl_ElementUpdated()
    {
        if (DataContext is not ViewActionViewModel vm) return;
        vm.RefreshElements();
        AppState.NotifyPlotChanged();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DataContextProperty && change.NewValue is ViewActionViewModel vm)
        {
            vm.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(vm.SelectedElement))
                {
                    OnSelectedElementChanged(vm.SelectedElement);
                }
            };
        }
    }

    private async void OnSelectedElementChanged(IElement? element)
    {
        try
        {
            if (_isHandlingSelection) return;

            if (EditorControl.IsVisible && EditorControl.HasChanges())
            {
                if (TopLevel.GetTopLevel(this) is Window window)
                {
                    var confirmDialog = new ConfirmWindow();
                    bool discard = await confirmDialog.ShowDialog<bool>(window);
                    if (!discard)
                    {
                        _isHandlingSelection = true;
                        if (DataContext is ViewActionViewModel vm)
                        {
                            vm.SelectedElement = element;
                        }
                        _isHandlingSelection = false;
                        return;
                    }
                }
            }

            if (element != null)
            {
                EditorControl.IsVisible = true;
                EditorControl.LoadElement(element);
            }
            else
            {
                EditorControl.IsVisible = false;
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